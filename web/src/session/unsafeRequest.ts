import { ApiError, isSessionChanged } from './ApiError';
import { currentSessionEpoch, invalidatePendingRequests } from './sessionEpoch';
import { boundedSignal } from './boundedSignal';
import { dispatchUnsafe } from './dispatchUnsafe';

let previous: Promise<void> = Promise.resolve();

export function unsafeRequest(path: string, init: RequestInit): Promise<Response> {
  const epoch = currentSessionEpoch();
  const endpoint = new URL(path, 'https://local.invalid').pathname;
  const operation = previous.then(async () => {
    if (epoch !== currentSessionEpoch()) throw new ApiError('session-changed', null);
    const bounded = boundedSignal(init.signal);
    try {
      return await dispatchUnsafe(path, init, bounded.signal, epoch);
    } catch (error) {
      if (isSessionChanged(error) && currentSessionEpoch() === epoch) {
        invalidatePendingRequests();
        error.sessionEpoch = currentSessionEpoch();
      }
      if (error instanceof ApiError && error.status === 401 && endpoint !== '/api/v1/auth/login' && currentSessionEpoch() === epoch) {
        invalidatePendingRequests();
        error.sessionEpoch = currentSessionEpoch();
      }
      throw error;
    } finally {
      bounded.cleanup();
    }
  });
  previous = operation.then(() => undefined, () => undefined);
  return operation;
}
