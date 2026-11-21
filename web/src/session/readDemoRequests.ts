import { Session } from './sessionTypes';
import { DemoRequest } from './demoTypes';

export async function readDemoRequests(request: Session['request']): Promise<DemoRequest[]> {
  const response = await request('/api/v1/requests');
  const items = await response.json();
  if (!Array.isArray(items)) throw new Error('Invalid request list response');
  return items as DemoRequest[];
}
