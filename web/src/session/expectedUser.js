import { ApiError } from './ApiError';

// state is { status: 'authenticated', user: { id, displayName, email, roles } } or another status.
export function withExpectedUser(state, init = {}) {
  if (state.status !== 'authenticated') throw new ApiError('session-changed', null);
  const headers = new Headers(init.headers);
  headers.set('X-Expected-User-Id', state.user.id);
  return { ...init, headers };
}