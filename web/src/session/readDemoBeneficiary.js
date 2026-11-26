import { ApiError } from './ApiError';

// Resolves to the one beneficiary profile { id, reference, displayName, isFamily } matching displayName.
export async function readDemoBeneficiary(request, displayName) {
  const response = await request('/api/v1/beneficiaries');
  const profiles = await response.json();
  const matches = profiles.filter(profile => profile.displayName === displayName);
  if (matches.length !== 1) throw new ApiError('unexpected', null);
  return matches[0];
} 