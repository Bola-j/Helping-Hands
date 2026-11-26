import { ApiError } from './ApiError';
import { fetchRequest } from './fetchRequest';
import { currentSessionEpoch, invalidatePendingRequests, observeSession } from './sessionEpoch';

export async function dispatchUnsafe(path, init, signal, epoch) {
  if (signal.aborted) throw new ApiError('network', null);
  const response = await fetchRequest('/api/v1/auth/antiforgery', { signal });
  const token = await response.json().catch(() => null);
  if (signal.aborted) throw new ApiError('network', null);
  if (typeof token?.requestToken !== 'string' || token.headerName !== 'X-CSRF-TOKEN') throw new ApiError('unexpected', null);
  const headers = new Headers(init.headers);
  headers.set(token.headerName, token.requestToken);
  if (epoch !== currentSessionEpoch()) throw new ApiError('session-changed', null);
  const result = await fetchRequest(path, { ...init, signal, headers });
  const endpoint = new URL(path, 'https://local.invalid').pathname;
  if (endpoint === '/api/v1/auth/login' || endpoint === '/api/v1/auth/logout') invalidatePendingRequests();
  if (endpoint === '/api/v1/auth/login') observeSession(currentSessionEpoch(), true);
  return result;
}