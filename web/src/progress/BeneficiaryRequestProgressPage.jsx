import { useSession } from '../session/SessionProvider';
import { BeneficiaryRequestProgressWorkspace } from './BeneficiaryRequestProgressWorkspace.jsx';

export function BeneficiaryRequestProgressPage() {
  const { state, request } = useSession();
  const beneficiary = state.status === 'authenticated' && state.user.roles.includes('Beneficiary');
  return <main className="request-progress-page">
    <header className="request-progress-topbar"><a className="request-progress-brand" href="/">Helping Hands</a><nav><a href="/requests/new">New request</a><a href="/login">Account</a></nav></header>
    <section className="request-progress-content">
      {state.status === 'loading' && <p role="status">Checking your session…</p>}
      {state.status === 'unavailable' && <p role="alert" className="request-progress-error">The session service is unavailable. Try again when it is running.</p>}
      {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with your beneficiary account to see your request progress.</p><a href="/login">Go to sign in</a></>}
      {state.status === 'authenticated' && !beneficiary && <><h1>Beneficiary access required</h1><p>This page only shows requests linked to a beneficiary account.</p><a href="/">Return home</a></>}
      {beneficiary && <BeneficiaryRequestProgressWorkspace request={request} user={state.user} />}
    </section>
  </main>;
}
