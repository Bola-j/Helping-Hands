export async function createOutcome(request, referralId, reportedQuantity, description) {
  const response = await request(`/api/v1/referrals/${referralId}/outcomes`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reportedQuantity: Number(reportedQuantity), description }),
  });
  return response.json();
}

export async function updateOutcome(request, outcome, reportedQuantity, description) {
  const response = await request(`/api/v1/outcomes/${outcome.id}`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reportedQuantity: Number(reportedQuantity), description, version: outcome.version }),
  });
  return response.json();
}

export async function uploadOutcomeEvidence(request, outcomeId, file) {
  const form = new FormData();
  form.append('file', file);
  const response = await request(`/api/v1/outcomes/${outcomeId}/attachments`, { method: 'POST', body: form });
  return response.json();
}

export async function submitOutcome(request, outcome) {
  const response = await request(`/api/v1/outcomes/${outcome.id}/submit`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ version: outcome.version }),
  });
  return response.json();
}
