import { ApiError } from './ApiError';
import { SessionState } from './sessionTypes';

export function withExpectedUser(state: SessionState, init: RequestInit = {}): RequestInit {
  if (state.status !== 'authenticated') throw new ApiError('session-changed', null);
  const headers = new Headers(init.headers);
  headers.set('X-Expected-User-Id', state.user.id);
  return { ...init, headers };
}
