import { ReactNode, useEffect, useRef, useState } from 'react';
import { SessionContext } from './SessionContext';
import { SessionState } from './sessionTypes';
import { useLogin, useLogout } from './useAuthActions';
import { useRefreshSession } from './useRefreshSession';
import { useSessionRequest } from './useSessionRequest';

export { useSession } from './SessionContext';

export function SessionProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<SessionState>({ status: 'loading' });
  const currentState = useRef(state);
  currentState.current = state;
  const generation = useRef(0);
  const refresh = useRefreshSession(setState, generation);
  const login = useLogin(setState, generation);
  const logout = useLogout(setState, generation, currentState);
  const request = useSessionRequest(setState, generation, currentState);
  useEffect(() => { void refresh(); return () => { generation.current++; }; }, [refresh]);
  return <SessionContext.Provider value={{ state, refresh, login, logout, request }}>{children}</SessionContext.Provider>;
}
