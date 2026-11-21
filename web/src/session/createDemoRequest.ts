import { Session } from './sessionTypes';
import { DemoRequest } from './demoTypes';

export async function createDemoRequest(request: Session['request'], beneficiaryId: string, description: string): Promise<DemoRequest> {
  const response = await request('/api/v1/requests/single-need', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ beneficiaryId, description, authorityConfirmed: true, sharingAgreed: true }),
  });
  return response.json() as Promise<DemoRequest>;
}
