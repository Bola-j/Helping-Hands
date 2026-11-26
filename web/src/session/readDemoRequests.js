// Resolves to a list of { id, reference, description, status, createdAt }.
export async function readDemoRequests(request) {
  const response = await request('/api/v1/requests');
  const items = await response.json();
  if (!Array.isArray(items)) throw new Error('Invalid request list response');
  return items;
}