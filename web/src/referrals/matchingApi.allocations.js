export async function loadEligibleCharities(request, requestId, need) {
  const query = new URLSearchParams({
    requestId,
    needId: need.id,
    serviceCategoryId: need.categoryId,
    serviceVersionId: need.serviceVersionId,
    standardVersionId: need.standardVersionId,
  });
  const response = await request(`/api/v1/matching/charities?${query}`);
  return response.json();
}

export async function submitSplitAllocation(request, requestId, needId, version, allocations) {
  const response = await request(`/api/v1/requests/${requestId}/needs/${needId}/referrals`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ version, allocations }),
  });
  return response.json();
}
