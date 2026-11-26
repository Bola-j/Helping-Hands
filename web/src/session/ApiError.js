// kind is one of: 'network', 'unauthorized', 'forbidden', 'invalid', 'conflict',
// 'rate-limited', 'unavailable', 'unexpected', 'session-changed'.
export class ApiError extends Error {
  sessionEpoch;

  constructor(kind, status, response) {
    super(`Request failed: ${kind}`);
    this.kind = kind;
    this.status = status;
    this.response = response;
    this.name = 'ApiError';
  }
}

export function responseError(response) {
  const kinds = { 400: 'invalid', 401: 'unauthorized', 403: 'forbidden', 409: 'conflict', 429: 'rate-limited' };
  return new ApiError(kinds[response.status] ?? (response.status >= 500 ? 'unavailable' : 'unexpected'), response.status, response);
}

export function isSessionChanged(error) {
  return error instanceof ApiError && error.status === 409 && error.response?.headers.get('X-Session-Changed') === 'true';
}