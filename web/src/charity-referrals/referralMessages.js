import { ApiError } from '../session/ApiError.js';

export function referralError(error) {
  if (error instanceof ApiError && error.status === 401) return 'Your sign-in ended. Sign in again to manage referrals.';
  if (error instanceof ApiError && error.status === 403) return 'Active charity representative access is required.';
  if (error instanceof ApiError && error.status === 404) return 'This referral is not available to your charity account.';
  if (error instanceof ApiError && error.status === 409) return 'This report changed or the referral is no longer open. Refresh the referrals and check the current status.';
  if (error instanceof ApiError && error.status === 400) return 'The report was not accepted. Check the positive quantity, description, assigned limit, and evidence-file requirements.';
  if (error instanceof ApiError && (error.kind === 'network' || error.kind === 'unavailable')) return 'The referral service is unavailable. Check that the local services are running, then try again.';
  return 'The referral action could not be completed. Try again.';
}

export function validateOutcomeReport(quantity, description, available) {
  const amount = Number(quantity);
  if (!Number.isFinite(amount) || amount <= 0 || amount > available || Math.round(amount * 1000) !== amount * 1000) return 'Enter a positive quantity up to the remaining assigned amount, using at most three decimal places.';
  if (!description.trim()) return 'Describe the delivery and receipt before saving the report.';
  return '';
}

export function validateEvidence(file) {
  if (!file) return 'Choose a PDF, JPEG, or PNG evidence file.';
  if (!['application/pdf', 'image/jpeg', 'image/png'].includes(file.type)) return 'Evidence must be a PDF, JPEG, or PNG file.';
  if (file.size <= 0 || file.size > 10 * 1024 * 1024) return 'Evidence must be larger than 0 bytes and no more than 10 MiB.';
  return '';
}
