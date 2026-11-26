import { useState } from 'react';
import { AuthShell } from './AuthShell';
import { Field, FormAlert, PasswordField, SubmitButton } from './fields';
import { validEmail } from './validation';
import { useSession } from '../session/SessionProvider';
import { authErrorMessage } from '../session/authErrorMessage';

export function LoginPage() {
  const { state, login, logout } = useSession();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [formError, setFormError] = useState('');
  const [busy, setBusy] = useState(false);

  async function onSubmit(event) {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const found = { email: validEmail(email), password: password ? '' : 'Enter your password.' };
    setErrors(found);
    setFormError('');
    if (found.email || found.password) {
      requestAnimationFrame(() => form.querySelector('[aria-invalid="true"]')?.focus());
      return;
    }
    setBusy(true);
    try {
      await login(email.trim(), password);
    } catch (error) {
      setFormError(authErrorMessage(error, 'login'));
    } finally {
      setPassword('');
      setBusy(false);
    }
  }

  async function onSignOut() {
    setBusy(true);
    setFormError('');
    try {
      await logout();
    } catch {
      setFormError('Signing out did not complete. Try again.');
    } finally {
      setBusy(false);
    }
  }

  let body;
  if (state.status === 'loading') {
    body = <p>Checking your session…</p>;
  } else if (state.status === 'authenticated') {
    body = (
      <section aria-labelledby="signedin-heading">
        <h2 id="signedin-heading">You are signed in</h2>
        <p className="auth-identity">
          <strong>{state.user.displayName}</strong>
          <span>{state.user.email}</span>
          {state.user.roles.length > 0 && <span>Role: {state.user.roles.join(', ')}</span>}
        </p>
        <FormAlert>{formError}</FormAlert>
        <div className="auth-actions">
          <a className="auth-button" href="/demo">Open the demo</a>
          {state.user.roles.includes('Beneficiary') && <a className="auth-button auth-button-quiet" href="/requests/new">Make a support request</a>}
          {state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role)) && <a className="auth-button auth-button-quiet" href="/coordinator/standards">Manage services and standards</a>}
          {state.user.roles.includes('RecipientRepresentative') && <a className="auth-button auth-button-quiet" href="/charity/application">View charity application</a>}
          {state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role)) && <a className="auth-button auth-button-quiet" href="/coordinator/applications">Review charity applications</a>}
          {state.user.roles.some(role => ['Coordinator', 'Administrator'].includes(role)) && <a className="auth-button auth-button-quiet" href="/coordinator/requests">Review assistance requests</a>}
          <button type="button" className="auth-button auth-button-quiet" onClick={onSignOut} disabled={busy}>
            {busy ? 'Signing out…' : 'Sign out'}
          </button>
        </div>
      </section>
    );
  } else {
    body = (
      <form onSubmit={onSubmit} noValidate>
        <h2>Your account</h2>
        {state.status === 'unavailable' && (
          <p className="auth-notice">
            The service could not be reached. Check that the local services are running, then sign in.
          </p>
        )}
        <FormAlert>{formError}</FormAlert>
        <Field id="email" type="email" label="Email" autoComplete="username"
          value={email} error={errors.email}
          onChange={v => { setEmail(v); setErrors(e => ({ ...e, email: '' })); }} />
        <PasswordField id="password" label="Password" autoComplete="current-password"
          value={password} error={errors.password}
          onChange={v => { setPassword(v); setErrors(e => ({ ...e, password: '' })); }} />
        <SubmitButton busy={busy} label="Sign in" busyLabel="Signing in…" />
        <p className="auth-switch">
          New here? <a href="/register/beneficiary">Create an account</a> or <a href="/register/charity">register a charity</a>.
        </p>
      </form>
    );
  }

  return <AuthShell active="login">{body}</AuthShell>;
}
