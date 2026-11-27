export async function loadCoordinatorRequests(request) {
  const requests = [];
  let cursor = '';
  do {
    const query = new URLSearchParams({ limit: '100' });
    if (cursor) query.set('cursor', cursor);
    const response = await request(`/api/v1/requests?${query}`);
    requests.push(...await response.json());
    cursor = response.headers.get('X-Next-Cursor') ?? '';
  } while (cursor);
  return requests;
}

export async function loadRequestForMatching(request, requestId) {
  const [detailResponse, progressResponse] = await Promise.all([
    request(`/api/v1/requests/${requestId}`),
    request(`/api/v1/requests/${requestId}/progress`),
  ]);
  return { details: await detailResponse.json(), progress: await progressResponse.json() };
}
