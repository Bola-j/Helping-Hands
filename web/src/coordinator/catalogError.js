import { ApiError } from '../session/ApiError';

export async function describeCatalogError(error) {
  if (error instanceof ApiError) {
    if (error.status === 401) return 'Your session ended. Sign in again to continue.';
    if (error.status === 403) return 'Coordinator or Administrator access is required.';
    if ([400, 409, 428].includes(error.status) && error.response) return await problemTitle(error.response) ?? 'Check the values and current version, then try again.';
    return error.kind === 'network' ? 'The catalog service could not be reached.' : 'The catalog request failed.';
  }
  return error instanceof Error ? error.message : 'The catalog request failed.';
}

async function problemTitle(response) {
  const problem = await response.clone().json().catch(() => null);
  return typeof problem?.title === 'string' ? problem.title : null;
}
