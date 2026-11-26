import { ApiError } from './ApiError';

export function authErrorMessage(error: unknown, action: 'signup' | 'login'): string {
  if (!(error instanceof ApiError)) return 'Something went wrong. Try again.';
  switch (error.kind) {
    case 'network':
    case 'unavailable':
      return 'The service is unavailable. Check that the local services are running, then try again.';
    case 'invalid':
      return action === 'signup' ? 'Check the fields below and try again.' : 'Enter a valid email and password.';
    case 'unauthorized':
      return action === 'login' ? 'Email or password is incorrect.' : 'Your session has expired. Sign in again.';
    case 'conflict':
      return action === 'signup'
        ? 'An account with this email may already exist. Sign in or use a different email.'
        : 'Your session changed. Refresh the page and try again.';
    case 'forbidden':
      return 'This account does not have permission to do that.';
    case 'rate-limited':
      return 'Too many attempts. Wait a moment before trying again.';
    case 'session-changed':
      return 'Your session changed. Refresh the page and try again.';
    default:
      return 'Something went wrong. Try again.';
  }
}