export async function loadOwnRequests(request) {
  const items = [];
  let cursor = '';
  do {
    const query = new URLSearchParams({ limit: '100' });
    if (cursor) query.set('cursor', cursor);
    const response = await request(`/api/v1/requests?${query}`);
    items.push(...await response.json());
    cursor = response.headers.get('X-Next-Cursor') ?? '';
  } while (cursor);
  return items;
}

export async function loadRequestProgress(request, requestId) {
  const response = await request(`/api/v1/requests/${requestId}/progress`);
  return response.json();
}
