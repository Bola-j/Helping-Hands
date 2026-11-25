// request is session.request. Resolves to { id, reference, description, status, createdAt }.
export async function createDemoRequest(request, beneficiaryId, description) {
  const response = await request('/api/v1/requests/single-need', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ beneficiaryId, description, authorityConfirmed: true, sharingAgreed: true }),
  });
  return response.json();
}