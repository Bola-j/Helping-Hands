export function ApplicationShell({ children }) {
  return <main className="application-page"><header className="application-topbar"><a href="/" className="application-brand">Helping Hands</a><a href="/login">Account</a></header><section className="application-content">{children}</section></main>;
}

export function ApplicationGate({ state, allowedRoles, children, prompt }) {
  if (state.status === 'loading') return <ApplicationShell><p role="status">Checking your session…</p></ApplicationShell>;
  if (state.status === 'unavailable') return <ApplicationShell><p role="alert">The session service is unavailable. Try again when it is running.</p></ApplicationShell>;
  if (state.status === 'anonymous') return <ApplicationShell><h1>Sign in required</h1><p>{prompt}</p><a href="/login">Go to sign in</a></ApplicationShell>;
  if (!state.user.roles.some(role => allowedRoles.includes(role))) return <ApplicationShell><h1>Access not available</h1><p>Your account cannot open this page.</p><a href="/">Return home</a></ApplicationShell>;
  return children;
}
