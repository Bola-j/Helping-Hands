export async function saveServiceChoices(request, recipientId, services) {
  const response = await request(`/api/v1/recipients/${recipientId}/services`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ services }),
  });
  return response.json();
}

export async function saveDailyCapacity(request, recipientId, charityServiceId, available) {
  const response = await request(`/api/v1/recipients/${recipientId}/capacity`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ capacities: [{ charityServiceId, available: Number(available) }] }),
  });
  return response.json();
}
