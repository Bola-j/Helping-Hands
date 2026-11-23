import { useState } from 'react';
import { useBeneficiaryDemo } from './session/useBeneficiaryDemo';

export function DemoPage() {
  const demo = useBeneficiaryDemo();
  const [mode, setMode] = useState('register');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [description, setDescription] = useState('');
  const [authorityConfirmed, setAuthorityConfirmed] = useState(false);
  const [sharingAgreed, setSharingAgreed] = useState(false);
  const status = demo.session.status;
  const isBeneficiary = status === 'authenticated' && demo.session.user.roles.includes('Beneficiary');
  const canSubmitRequest = description.trim().length >= 10 && authorityConfirmed && sharingAgreed;

  function submitAuth(event) {
    event.preventDefault();
    if (mode === 'register') {
      demo.register(name.trim(), email.trim(), password).then(created => { if (created) setMode('login'); });
    } else {
      demo.login(email.trim(), password);
    }
  }

  function submitRequest(event) {
    event.preventDefault();
    if (!canSubmitRequest) return;
    demo.submitRequest(description.trim()).then(saved => {
      if (saved) {
        setDescription('');
        setAuthorityConfirmed(false);
        setSharingAgreed(false);
      }
    });
  }

  return (
    <main className="page">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Helping Hands home">Helping Hands</a>
        <nav aria-label="Main navigation"><a href="/how-it-works">How it works</a></nav>
      </header>
      <section className="content">
        <p className="eyebrow">Fictional data demo</p>
        <h1>Beneficiary request demo</h1>
        <p>Create a demo account, sign in, submit one request for a single need, and see its reference and status.</p>

        {demo.error && <p role="alert" className="demo-error">{demo.error}</p>}
        {demo.notice && <p role="status" className="demo-notice">{demo.notice}</p>}
        {status === 'loading' && <p>Checking your session…</p>}
        {status === 'unavailable' && <p className="demo-error">The session service is unavailable. Check that the API is running, then reload this page.</p>}

        {status === 'anonymous' && (
          <form className="demo-card" onSubmit={submitAuth}>
            <div className="demo-modes">
              <button type="button" aria-pressed={mode === 'register'} onClick={() => setMode('register')}>Create account</button>
              <button type="button" aria-pressed={mode === 'login'} onClick={() => setMode('login')}>Sign in</button>
            </div>
            {mode === 'register' && (
              <label>Display name<input value={name} onChange={e => setName(e.target.value)} autoComplete="name" required /></label>
            )}
            <label>Email<input type="email" value={email} onChange={e => setEmail(e.target.value)} autoComplete="email" required /></label>
            <label>Password<input type="password" value={password} onChange={e => setPassword(e.target.value)} autoComplete={mode === 'register' ? 'new-password' : 'current-password'} required /></label>
            <button type="submit" disabled={demo.busy}>{mode === 'register' ? 'Create beneficiary account' : 'Sign in'}</button>
            <small>Use fictional details. Registration and sign-in are separate steps.</small>
          </form>
        )}

        {status === 'authenticated' && !isBeneficiary && (
          <div className="demo-card">
            <h2>This demo is for beneficiary accounts</h2>
            <p>Sign out and use a fictional beneficiary account to continue.</p>
            <button type="button" onClick={demo.logout} disabled={demo.busy}>Sign out</button>
          </div>
        )}

        {isBeneficiary && (
          <div className="demo-card">
            <h2>Welcome, {demo.session.user.displayName}</h2>
            <p>Beneficiary reference: {demo.profile?.reference ?? 'Loading…'}</p>
            <button type="button" onClick={demo.logout} disabled={demo.busy}>Sign out</button>

            <form onSubmit={submitRequest}>
              <h3>Submit a single-need request</h3>
              <label>Describe the one need
                <textarea value={description} onChange={e => setDescription(e.target.value)} rows={4} placeholder="Describe the support needed (at least 10 characters)" />
              </label>
              <label className="demo-check">
                <input type="checkbox" checked={authorityConfirmed} onChange={e => setAuthorityConfirmed(e.target.checked)} />
                I am authorized to make this request.
              </label>
              <label className="demo-check">
                <input type="checkbox" checked={sharingAgreed} onChange={e => setSharingAgreed(e.target.checked)} />
                I agree that these details can be shared with relevant support organizations.
              </label>
              <button type="submit" disabled={!canSubmitRequest || demo.busy}>Submit request</button>
            </form>

            <h3>Your requests</h3>
            {demo.requests.length === 0 ? <p>No requests yet.</p> : (
              <ul className="demo-list">
                {demo.requests.map(r => (
                  <li key={r.id}><strong>{r.reference}</strong> — {r.status}<br />{r.description}</li>
                ))}
              </ul>
            )}
            <small>One need per request. Multi-need requests and charity or coordinator workflows are outside this demo.</small>
          </div>
        )}
      </section>
    </main>
  );
}