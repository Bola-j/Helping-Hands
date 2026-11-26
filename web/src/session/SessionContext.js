import { createContext, useContext } from 'react';

// The session value is { state, refresh, login, logout, request }.
// state is { status: 'loading' | 'anonymous' | 'unavailable' }
// or { status: 'authenticated', user: { id, displayName, email, roles } }.
export const SessionContext = createContext(null);

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error('SessionProvider is missing');
  return session;
}