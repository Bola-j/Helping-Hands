export type ApiErrorKind = 'network' | 'unauthorized' | 'forbidden' | 'invalid' | 'conflict' | 'rate-limited' | 'unavailable' | 'unexpected' | 'session-changed';

export class ApiError extends Error {
  sessionEpoch?: number;
  constructor(public readonly kind: ApiErrorKind, public readonly status: number | null, public readonly response?: Response) {
    super(`Request failed: ${kind}`);
    this.name = 'ApiError';
  }
}

export function responseError(response: Response): ApiError {
  const kinds: Record<number, ApiErrorKind> = { 400: 'invalid', 401: 'unauthorized', 403: 'forbidden', 409: 'conflict', 429: 'rate-limited' };
  return new ApiError(kinds[response.status] ?? (response.status >= 500 ? 'unavailable' : 'unexpected'), response.status, response);
}

export function isSessionChanged(error: unknown): error is ApiError {
  return error instanceof ApiError && error.status === 409 && error.response?.headers.get('X-Session-Changed') === 'true';
}
