export async function loadCharities(request) {
  const charities = await (await request('/api/v1/recipients')).json();
  if (!Array.isArray(charities)) throw new Error('Invalid charity list response');
  return charities;
}

export async function loadServiceChoices(request, recipientId) {
  return (await request(`/api/v1/recipients/${recipientId}/services`)).json();
}

export async function loadCapacity(request, recipientId) {
  return (await request(`/api/v1/recipients/${recipientId}/capacity`)).json();
}
