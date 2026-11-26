export async function loadRecipient(request, id) {
  const response = await request(`/api/v1/recipients/${id}`);
  return response.json();
}

export async function uploadApplicationFile(request, id, file) {
  const form = new FormData();
  form.append('file', file);
  return request(`/api/v1/recipients/${id}/attachments`, { method: 'POST', body: form });
}

export async function decideApplication(request, app, decision, reason) {
  return request(`/api/v1/recipients/${app.id}/decisions`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ decision, reason: reason || null, version: app.version }),
  });
}

