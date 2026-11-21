export type BeneficiaryProfile = {
  id: string;
  reference: string;
  displayName: string;
  isFamily: boolean;
};

export type DemoRequest = {
  id: string;
  reference: string;
  description: string;
  status: string;
  createdAt: string;
};

export type BeneficiarySignup = {
  accountId: string;
  beneficiaryId: string;
  reference: string;
  displayName: string;
  email: string;
};
