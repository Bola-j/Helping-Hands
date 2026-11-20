export type SessionUser = { id: string; displayName: string; email: string; roles: string[] };
export type SessionState =
  | { status: 'loading' }
  | { status: 'anonymous' }
  | { status: 'unavailable' }
  | { status: 'authenticated'; user: SessionUser };

export type Session = {
  state: SessionState;
  refresh: () => Promise<void>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  request: (path: string, init?: RequestInit) => Promise<Response>;
};
