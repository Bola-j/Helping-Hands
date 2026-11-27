export async function loadAssignedReferrals(request) {
  const response = await request('/api/v1/referrals');
  const items = await response.json();
  if (!Array.isArray(items)) throw new Error('Invalid assigned referral response');
  return items;
}

export async function loadOutcomes(request) {
  const response = await request('/api/v1/outcomes');
  const items = await response.json();
  if (!Array.isArray(items)) throw new Error('Invalid outcome response');
  return items;
}

export async function loadReferralProgress(request, requestId) {
  const response = await request(`/api/v1/requests/${requestId}/progress`);
  return response.json();
}
