import { useCallback, useEffect, useState } from 'react';
import { useSession } from './SessionProvider';
import { registerDemoBeneficiary } from './registerDemoBeneficiary';
import { readDemoBeneficiary } from './readDemoBeneficiary';
import { readDemoRequests } from './readDemoRequests';
import { createDemoRequest } from './createDemoRequest';
import { demoErrorMessage } from './demoErrorMessage';

export function useBeneficiaryDemo() {
  const session = useSession();
  const { request, login: signIn, logout: signOut } = session;
  const [profile, setProfile] = useState(null);
  const [requests, setRequests] = useState([]);
  const [signup, setSignup] = useState(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const reload = useCallback(async () => {
    if (session.state.status !== 'authenticated') return;
    const beneficiary = await readDemoBeneficiary(request, session.state.user.displayName);
    const ownRequests = await readDemoRequests(request);
    setProfile(beneficiary);
    setRequests(ownRequests);
  }, [request, session.state]);
  useEffect(() => { void reload().catch(cause => setError(demoErrorMessage(cause))); }, [reload]);
  const run = useCallback(async (action) => {
    setBusy(true); setError(''); setNotice('');
    try { await action(); return true; } catch (cause) { setError(demoErrorMessage(cause)); return false; }
    finally { setBusy(false); }
  }, []);
  const register = useCallback((name, email, password) => run(async () => {
    const result = await registerDemoBeneficiary(name, email, password);
    setSignup(result); setNotice(`Registration complete. Your beneficiary reference is ${result.reference}. Sign in to continue.`);
  }), [run]);
  const login = useCallback((email, password) => run(async () => {
    await signIn(email, password);
    setSignup(null); setNotice('You are signed in. Your requests are shown below.');
  }), [run, signIn]);
  const logout = useCallback(() => run(async () => {
    await signOut(); setProfile(null); setRequests([]); setNotice('You are signed out.');
  }), [run, signOut]);
  const submitRequest = useCallback((description) => run(async () => {
    if (!profile) throw new Error('Your beneficiary profile is not available. Refresh the page and sign in again.');
    const created = await createDemoRequest(request, profile.id, description);
    setNotice(`Request ${created.reference} submitted with status: ${created.status}.`);
    await reload();
  }), [profile, reload, request, run]);
  return { session: session.state, profile, requests, signup, busy, error, notice, register, login, logout, submitRequest };
}