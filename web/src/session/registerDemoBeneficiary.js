import { apiFetch } from './api';

// Resolves to { accountId, beneficiaryId, reference, displayName, email }.
export async function registerDemoBeneficiary(displayName, email, password) {
  const response = await apiFetch('/api/v1/auth/register/beneficiary', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ displayName, email, password, isFamily: false }),
  });
  return response.json();
}