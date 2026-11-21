import { ApiError } from './ApiError';
import { Session } from './sessionTypes';
import { BeneficiaryProfile } from './demoTypes';

export async function readDemoBeneficiary(request: Session['request'], displayName: string): Promise<BeneficiaryProfile> {
  const response = await request('/api/v1/beneficiaries');
  const profiles = await response.json() as BeneficiaryProfile[];
  const matches = profiles.filter(profile => profile.displayName === displayName);
  if (matches.length !== 1) throw new ApiError('unexpected', null);
  return matches[0];
}
