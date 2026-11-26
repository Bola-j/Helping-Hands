import { describeCatalogError } from './catalogError';

export async function catalogRequest(request, path, options = {}) {
  const headers = new Headers({ Accept: 'application/json' });
  if (options.body !== undefined) headers.set('Content-Type', 'application/json');
  if (options.etag) headers.set('If-Match', options.etag);
  try {
    const response = await request(path, {
      method: options.method ?? 'GET', headers,
      ...(options.body === undefined ? {} : { body: JSON.stringify(options.body) }),
    });
    return { data: response.status === 204 ? null : await response.json(), etag: response.headers.get('ETag') };
  } catch (error) {
    throw new Error(await describeCatalogError(error));
  }
}

export async function catalogPage(request, path) {
  const items = [];
  let cursor = '';
  do {
    const separator = path.includes('?') ? '&' : '?';
    const result = await catalogRequest(request, `${path}${separator}limit=100${cursor}`);
    items.push(...(result.data.items ?? result.data.Items ?? []));
    cursor = result.data.nextCursor ?? result.data.NextCursor;
    cursor = cursor ? `&cursor=${encodeURIComponent(cursor)}` : '';
  } while (cursor);
  return items;
}

export async function catalogCommand(request, path, method, body, etagPath = path) {
  const current = await catalogRequest(request, etagPath);
  return catalogRequest(request, path, { method, body, etag: current.etag });
}

