import { useSession } from '../session/SessionProvider';
import { CharityReferralWorkspace } from './CharityReferralWorkspace.jsx';

export function CharityReferralPage() {
  const { state, request } = useSession();
  const representative = state.status === 'authenticated' && state.user.roles.includes('RecipientRepresentative');
  return <main className="charity-referrals-page"><header className="charity-referrals-topbar"><a className="charity-referrals-brand" href="/">Helping Hands</a><nav><a href="/charity/services">Services and capacity</a><a href="/login">Account</a></nav></header><section className="charity-referrals-content">
    {state.status === 'loading' && <p role="status">Checking your session…</p>}
    {state.status === 'unavailable' && <p role="alert" className="charity-referrals-alert">The session service is unavailable. Try again when it is running.</p>}
    {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with an active charity representative account to manage assigned referrals.</p><a href="/login">Go to sign in</a></>}
    {state.status === 'authenticated' && !representative && <><h1>Charity access required</h1><p>This workspace is for charity representatives.</p><a href="/">Return home</a></>}
    {representative && <CharityReferralWorkspace request={request} user={state.user} />}
  </section></main>;
}
