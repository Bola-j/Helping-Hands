import { ApiError } from '../session/ApiError';

export function charityServicesError(error, operation) {
  if (error instanceof ApiError && error.status === 401) return 'Your sign-in ended. Sign in again to continue.';
  if (error instanceof ApiError && error.status === 403) return 'A charity representative account is required for this action.';
  if (error instanceof ApiError && error.status === 404) return 'This charity is not linked to your account.';
  if (error instanceof ApiError && error.status === 409) return 'The charity data changed while you were saving. Refresh and try again.';
  if (error instanceof ApiError && error.status === 400) return 'The update was not accepted. Check the selected standards and nonnegative quantities.';
  if (error instanceof ApiError && (error.kind === 'network' || error.kind === 'unavailable')) return `The service could not ${operation}. Check that the local services are running, then try again.`;
  return `The charity's services or capacity could not ${operation}. Try again.`;
}
