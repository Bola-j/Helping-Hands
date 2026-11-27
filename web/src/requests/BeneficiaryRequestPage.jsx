import { useSession } from '../session/SessionProvider';
import { BeneficiaryRequestWorkspace } from './BeneficiaryRequestWorkspace.jsx';

export function BeneficiaryRequestPage() {
  const { state, request } = useSession();
  return <main className="beneficiary-request-page">
    <header className="beneficiary-request-topbar"><a className="beneficiary-request-brand" href="/">Helping Hands</a><nav><a href="/requests/progress">My request progress</a><a href="/login">Account</a></nav></header>
    <section className="beneficiary-request-content">
      {state.status === 'loading' && <p role="status">Checking your session…</p>}
      {state.status === 'unavailable' && <p role="alert" className="beneficiary-request-error">The service is unavailable. Check that the local services are running, then try again.</p>}
      {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with your beneficiary account to make a request.</p><a href="/login">Go to sign in</a></>}
      {state.status === 'authenticated' && !state.user.roles.includes('Beneficiary') && <><h1>Beneficiary access required</h1><p>This form is for signed-in beneficiaries.</p><a href="/">Return home</a></>}
      {state.status === 'authenticated' && state.user.roles.includes('Beneficiary') && <BeneficiaryRequestWorkspace request={request} user={state.user} />}
    </section>
  </main>;
}
