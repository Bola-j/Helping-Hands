let epoch = 0;
let authenticated = false;

export function currentSessionEpoch(): number { return epoch; }
export function invalidatePendingRequests(): void { epoch++; authenticated = false; }
export function observeSession(expected: number, isAuthenticated: boolean): boolean {
  if (expected !== epoch) return false;
  if (!isAuthenticated && authenticated) { invalidatePendingRequests(); return true; }
  else authenticated = isAuthenticated;
  return false;
}
