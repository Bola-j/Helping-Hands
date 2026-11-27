import { useSession } from '../session/SessionProvider';
import { CoordinatorFollowUpClosureWorkspace } from './CoordinatorFollowUpClosureWorkspace.jsx';

export function CoordinatorFollowUpClosurePage() {
  const { state, request } = useSession();
  const coordinator = state.status === 'authenticated' && state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role));
  return <main className="follow-up-page">
    <header className="follow-up-topbar"><a href="/" className="follow-up-brand">Helping Hands</a><nav aria-label="Coordinator navigation"><a href="/coordinator/requests">Review requests</a><a href="/coordinator/standards">Service standards</a><a href="/coordinator/matching">Match and refer</a><a href="/coordinator/applications">Charity applications</a><a href="/login">Account</a></nav></header>
    <section className="follow-up-content">
      {state.status === 'loading' && <p role="status">Checking your session…</p>}
      {state.status === 'unavailable' && <p role="alert" className="follow-up-error">The session service is unavailable. Try again when it is running.</p>}
      {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with a coordinator or administrator account to follow up and close requests.</p><a href="/login">Go to sign in</a></>}
      {state.status === 'authenticated' && !coordinator && <><h1>Access not available</h1><p>This workspace is for coordinators and administrators.</p><a href="/">Return home</a></>}
      {coordinator && <CoordinatorFollowUpClosureWorkspace request={request} user={state.user} />}
    </section>
  </main>;
}
