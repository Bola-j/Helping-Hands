import { apiFetch } from './api';

export type CharitySignupInput = {
  representativeDisplayName: string;
  email: string;
  password: string;
  organizationName: string;
  contactDetails: string;
  registrationInformation?: string;
};

export type CharitySignup = { accountId: string; recipientId: string; reference: string };

export async function registerCharity(input: CharitySignupInput): Promise<CharitySignup> {
  const { registrationInformation, ...rest } = input;
  const body = registrationInformation?.trim() ? { ...rest, registrationInformation: registrationInformation.trim() } : rest;
  const response = await apiFetch('/api/v1/auth/register/charity', {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
  });
  return response.json() as Promise<CharitySignup>;
}