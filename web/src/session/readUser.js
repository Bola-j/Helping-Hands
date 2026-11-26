import { ApiError } from './ApiError';
import { apiFetch } from './api';

// Resolves to { id, displayName, email, roles }.
export async function readUser() {
  const response = await apiFetch('/api/v1/auth/me');
  const user = await response.json().catch(() => null);
  if (!user || typeof user.id !== 'string' || typeof user.displayName !== 'string' ||
      typeof user.email !== 'string' || !Array.isArray(user.roles) || !user.roles.every((role) => typeof role === 'string')) {
    throw new ApiError('unexpected', null);
  }
  return user;
}