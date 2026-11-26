import { useSession } from '../session/SessionProvider';
import { CharityServicesWorkspace } from './CharityServicesWorkspace.jsx';

export function CharityServicesPage() {
  const { state, request } = useSession();
  return <main className="charity-services-page">
    <header className="charity-services-topbar"><a href="/" className="charity-services-brand">Helping Hands</a><a href="/login">Account</a></header>
    <section className="charity-services-content">
      {state.status === 'loading' && <p role="status">Checking your session…</p>}
      {state.status === 'unavailable' && <p role="alert" className="charity-services-error">The session service is unavailable. Try again when it is running.</p>}
      {state.status === 'anonymous' && <><h1>Sign in required</h1><p>Sign in with your charity representative account to manage supported services and capacity.</p><a href="/login">Go to sign in</a></>}
      {state.status === 'authenticated' && !state.user.roles.includes('RecipientRepresentative') && <><h1>Charity access required</h1><p>This workspace is for charity representatives.</p><a href="/">Return home</a></>}
      {state.status === 'authenticated' && state.user.roles.includes('RecipientRepresentative') && <CharityServicesWorkspace request={request} user={state.user} />}
    </section>
  </main>;
}
