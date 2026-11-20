import { ApiError } from './ApiError';
import { apiFetch } from './api';
import { SessionUser } from './sessionTypes';

export async function readUser(): Promise<SessionUser> {
  const response = await apiFetch('/api/v1/auth/me');
  const user = await response.json().catch(() => null);
  if (!user || typeof user.id !== 'string' || typeof user.displayName !== 'string' ||
      typeof user.email !== 'string' || !Array.isArray(user.roles) || !user.roles.every((role: unknown) => typeof role === 'string')) {
    throw new ApiError('unexpected', null);
  }
  return user;
}
