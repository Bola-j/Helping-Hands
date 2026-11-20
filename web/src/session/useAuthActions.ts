import { Dispatch, SetStateAction, useCallback } from 'react';
import { ApiError, isSessionChanged } from './ApiError';
import { apiFetch } from './api';
import { readUser } from './readUser';
import { SessionState } from './sessionTypes';
import { withExpectedUser } from './expectedUser';
import { currentSessionEpoch } from './sessionEpoch';

export function useLogin(setState: Dispatch<SetStateAction<SessionState>>, generation: { current: number }) {
  return useCallback(async (email: string, password: string) => {
    await apiFetch('/api/v1/auth/login', {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email, password }),
    });
    const current = ++generation.current;
    try {
      const user = await readUser();
      if (generation.current === current) setState({ status: 'authenticated', user });
    } catch (error) {
      if (generation.current === current) setState({ status: error instanceof ApiError && error.status === 401 ? 'anonymous' : 'unavailable' });
      throw error;
    }
  }, [generation, setState]);
}

export function useLogout(setState: Dispatch<SetStateAction<SessionState>>, generation: { current: number }, state: { current: SessionState }) {
  return useCallback(async () => {
    const current = generation.current;
    try {
      await apiFetch('/api/v1/auth/logout', withExpectedUser(state.current, { method: 'POST' }));
      generation.current++;
      setState({ status: 'anonymous' });
    } catch (error) {
      if (generation.current === current && isSessionChanged(error) && error.sessionEpoch === currentSessionEpoch()) {
        generation.current++;
        setState({ status: 'unavailable' });
      }
      if (generation.current === current && error instanceof ApiError && error.status === 401 && error.sessionEpoch === currentSessionEpoch()) {
        generation.current++;
        setState({ status: 'anonymous' });
      }
      throw error;
    }
  }, [generation, setState, state]);
}
