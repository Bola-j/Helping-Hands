import { useSession } from '../session/SessionProvider';
import { CoordinatorMatchingWorkspace } from './CoordinatorMatchingWorkspace.jsx';

export function CoordinatorMatchingPage() {
  const { state, request } = useSession();
  const allowed = state.status === 'authenticated' && state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role));
  return <main className="matching-page">
    <header className="matching-topbar"><a className="matching-brand" href="/">Helping Hands</a><a href="/login">Account</a></header>
    <section className="matching-content">
      <p className="matching-eyebrow">Coordinator workspace</p>
      {state.status === 'loading' && <p role="status">Checking your session…</p>}
      {state.status === 'unavailable' && <p role="alert" className="matching-error">The session service is unavailable. Try again when it is running.</p>}
      {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with a coordinator or administrator account to match requests.</p><a href="/login">Go to sign in</a></>}
      {state.status === 'authenticated' && !allowed && <><h1>Access not available</h1><p>This workspace is for coordinators and administrators.</p><a href="/">Return home</a></>}
      {allowed && <CoordinatorMatchingWorkspace request={request} user={state.user} />}
    </section>
  </main>;
}
