import { ApiError, isSessionChanged } from './ApiError';
import { fetchRequest } from './fetchRequest';
import { unsafeRequest } from './unsafeRequest';
import { currentSessionEpoch, invalidatePendingRequests, observeSession } from './sessionEpoch';

export { ApiError } from './ApiError';

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const url = new URL(path, 'https://local.invalid');
  if (!path.startsWith('/') || path.startsWith('//') || url.origin !== 'https://local.invalid' || !/^\/api\/v1(?:\/|$)/.test(url.pathname)) {
    throw new ApiError('invalid', null);
  }
  const method = (init.method ?? 'GET').toUpperCase();
  if (['GET', 'HEAD', 'OPTIONS', 'TRACE'].includes(method)) {
    const epoch = currentSessionEpoch();
    try {
      const response = await fetchRequest(path, init);
      if (url.pathname === '/api/v1/auth/me') observeSession(epoch, true);
      return response;
    } catch (error) {
      if (isSessionChanged(error) && currentSessionEpoch() === epoch) {
        invalidatePendingRequests();
        error.sessionEpoch = currentSessionEpoch();
      }
      if (error instanceof ApiError && error.status === 401 && observeSession(epoch, false)) error.sessionEpoch = currentSessionEpoch();
      throw error;
    }
  }
  return unsafeRequest(path, init);
}
