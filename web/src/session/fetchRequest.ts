import { ApiError, responseError } from './ApiError';
import { boundedSignal } from './boundedSignal';

export async function fetchRequest(path: string, init: RequestInit = {}): Promise<Response> {
  let response: Response;
  const bounded = boundedSignal(init.signal);
  try {
    response = await fetch(path, { ...init, signal: bounded.signal, credentials: 'same-origin', cache: 'no-store' });
  } catch {
    throw new ApiError('network', null);
  } finally {
    bounded.cleanup();
  }
  if (!response.ok) throw responseError(response);
  return response;
}
