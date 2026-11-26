import { ApiError } from '../session/ApiError';

export async function loadCurrentCatalog(request) {
  const items = [];
  let cursor = '';
  do {
    const query = new URLSearchParams({ limit: '100' });
    if (cursor) query.set('cursor', cursor);
    const page = await (await request(`/api/v1/catalog/current?${query}`)).json();
    if (!Array.isArray(page.items)) throw new ApiError('unexpected', null);
    items.push(...page.items);
    cursor = page.nextCursor ?? '';
  } while (cursor);
  return items;
}

export async function submitMultiNeedRequest(request, beneficiaryId, description, needs) {
  const response = await request('/api/v1/requests', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ beneficiaryId, description: description.trim(), authorityConfirmed: true, sharingAgreed: true, needs }),
  });
  const created = await response.json();
  if (typeof created.reference !== 'string' || typeof created.status !== 'string') throw new ApiError('unexpected', null);
  return created;
}
