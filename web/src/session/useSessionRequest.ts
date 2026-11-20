import { Dispatch, SetStateAction, useCallback } from 'react';
import { ApiError, isSessionChanged } from './ApiError';
import { apiFetch } from './api';
import { SessionState } from './sessionTypes';
import { currentSessionEpoch, invalidatePendingRequests } from './sessionEpoch';
import { withExpectedUser } from './expectedUser';

export function useSessionRequest(setState: Dispatch<SetStateAction<SessionState>>, generation: { current: number }, state: { current: SessionState }) {
  return useCallback(async (path: string, init?: RequestInit) => {
    const current = generation.current;
    const epoch = currentSessionEpoch();
    try {
      return await apiFetch(path, withExpectedUser(state.current, init));
    } catch (error) {
      if (generation.current === current && isSessionChanged(error) && error.sessionEpoch === currentSessionEpoch()) {
        generation.current++;
        setState({ status: 'unavailable' });
      }
      if (generation.current === current && error instanceof ApiError && error.status === 401 &&
          (currentSessionEpoch() === epoch || error.sessionEpoch === currentSessionEpoch())) {
        if (currentSessionEpoch() === epoch) invalidatePendingRequests();
        generation.current++;
        setState({ status: 'anonymous' });
      }
      throw error;
    }
  }, [generation, setState, state]);
}
