let epoch = 0;
let authenticated = false;

export function currentSessionEpoch() { return epoch; }
export function invalidatePendingRequests() { epoch++; authenticated = false; }
export function observeSession(expected, isAuthenticated) {
  if (expected !== epoch) return false;
  if (!isAuthenticated && authenticated) { invalidatePendingRequests(); return true; }
  else authenticated = isAuthenticated;
  return false;
}