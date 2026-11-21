import { apiFetch } from './api';
import { BeneficiarySignup } from './demoTypes';

export async function registerDemoBeneficiary(displayName: string, email: string, password: string): Promise<BeneficiarySignup> {
  const response = await apiFetch('/api/v1/auth/register/beneficiary', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ displayName, email, password, isFamily: false }),
  });
  return response.json() as Promise<BeneficiarySignup>;
}
