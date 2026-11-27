import { useSession } from '../session/SessionProvider';
import { CoordinatorRequestReviewWorkspace } from './CoordinatorRequestReviewWorkspace';

export function CoordinatorRequestReviewPage() {
  const { state, request } = useSession();
  return <main className="request-review-page">
    <header className="request-review-topbar"><a href="/" className="request-review-brand">Helping Hands</a><nav><a href="/coordinator/matching">Match &amp; refer</a> · <a href="/login">Account</a></nav></header>
    <section className="request-review-content">
      {state.status === 'loading' && <p role="status">Checking your session…</p>}
      {state.status === 'unavailable' && <p role="alert" className="request-review-error">The session service is unavailable. Try again when it is running.</p>}
      {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with a coordinator or administrator account to review requests.</p><a href="/login">Go to sign in</a></>}
      {state.status === 'authenticated' && !state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role)) && <><h1>Access not available</h1><p>This workspace is for coordinators and administrators.</p><a href="/">Return home</a></>}
      {state.status === 'authenticated' && state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role)) && <CoordinatorRequestReviewWorkspace request={request} user={state.user} />}
    </section>
  </main>;
}
