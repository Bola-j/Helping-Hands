import { ApiError } from '../session/ApiError.js';

export function requestListError(error) {
  if (error instanceof ApiError && error.status === 401) return 'Your sign-in ended. Sign in again to view your requests.';
  if (error instanceof ApiError && error.status === 403) return 'This account cannot view beneficiary requests.';
  if (error instanceof ApiError && (error.kind === 'network' || error.kind === 'unavailable')) return 'Your requests could not be loaded. Check that the local services are running, then try again.';
  return 'Your requests could not be loaded. Try again.';
}

export function requestProgressError(error) {
  if (error instanceof ApiError && error.status === 401) return 'Your sign-in ended. Sign in again to view progress.';
  if (error instanceof ApiError && error.status === 403) return 'This account cannot view that request.';
  if (error instanceof ApiError && error.status === 404) return 'That request is not available to this account.';
  if (error instanceof ApiError && (error.kind === 'network' || error.kind === 'unavailable')) return 'Progress could not be loaded. Check that the local services are running, then try again.';
  return 'Progress could not be loaded. Try again.';
}
