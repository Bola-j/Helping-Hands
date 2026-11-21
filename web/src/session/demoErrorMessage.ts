import { ApiError } from './ApiError';

export function demoErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) return 'The request could not be completed. Try again.';
  if (error.kind === 'network' || error.kind === 'unavailable') return 'The demo API is unavailable. Check that the local services are running, then try again.';
  if (error.kind === 'invalid') return 'Check the required fields and both confirmations, then try again.';
  if (error.kind === 'unauthorized' || error.kind === 'session-changed') return 'Your sign-in has expired or changed. Sign in again to continue.';
  if (error.kind === 'forbidden') return 'This account does not have permission to submit a beneficiary request.';
  if (error.kind === 'conflict') return 'The request changed before it was saved. Refresh the page and try again.';
  if (error.kind === 'rate-limited') return 'Too many attempts. Wait a moment before trying again.';
  return 'The demo could not complete that action. Try again.';
}
