import { useSession } from '../session/SessionProvider';
import { CoordinatorStandardsWorkspace } from './CoordinatorStandardsWorkspace';
import './catalog.css';

export function CoordinatorStandardsPage() {
  const { state, request } = useSession();
  if (state.status === 'loading') return <CatalogShell><p role="status">Checking your session…</p></CatalogShell>;
  if (state.status === 'unavailable') return <CatalogShell><p role="alert">The session service is unavailable. Try again when it is running.</p></CatalogShell>;
  if (state.status === 'anonymous') return <CatalogShell><h1>Sign in required</h1><p>Sign in with a coordinator or administrator account to manage standards.</p><a href="/login">Go to sign in</a></CatalogShell>;
  if (!state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role))) return <CatalogShell><h1>Access not available</h1><p>This workspace is for coordinators and administrators.</p><a href="/">Return home</a></CatalogShell>;
  return <CoordinatorStandardsWorkspace request={request} user={state.user} />;
}

function CatalogShell({ children }) {
  return <main className="catalog-page"><header className="catalog-topbar"><a href="/" className="catalog-brand">Helping Hands</a><a href="/login">Account</a></header><section className="catalog-content">{children}</section></main>;
}
