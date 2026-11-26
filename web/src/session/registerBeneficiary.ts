import { apiFetch } from './api';
import { BeneficiarySignup } from './demoTypes';

export type BeneficiarySignupInput = {
  displayName: string;
  email: string;
  password: string;
  contactDetails?: string;
  isFamily: boolean;
};

export async function registerBeneficiary(input: BeneficiarySignupInput): Promise<BeneficiarySignup> {
  const { contactDetails, ...rest } = input;
  const body = contactDetails?.trim() ? { ...rest, contactDetails: contactDetails.trim() } : rest;
  const response = await apiFetch('/api/v1/auth/register/beneficiary', {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
  });
  return response.json() as Promise<BeneficiarySignup>;
}