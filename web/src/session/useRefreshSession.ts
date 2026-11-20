import { Dispatch, SetStateAction, useCallback } from 'react';
import { ApiError } from './ApiError';
import { readUser } from './readUser';
import { SessionState } from './sessionTypes';
import { currentSessionEpoch } from './sessionEpoch';

export function useRefreshSession(setState: Dispatch<SetStateAction<SessionState>>, generation: { current: number }) {
  return useCallback(async () => {
    const current = ++generation.current;
    const epoch = currentSessionEpoch();
    try {
      const user = await readUser();
      if (generation.current === current && currentSessionEpoch() === epoch) setState({ status: 'authenticated', user });
    } catch (error) {
      if (generation.current === current && (currentSessionEpoch() === epoch ||
          (error instanceof ApiError && error.sessionEpoch === currentSessionEpoch()))) {
        setState({ status: error instanceof ApiError && error.status === 401 ? 'anonymous' : 'unavailable' });
      }
    }
  }, [generation, setState]);
}
