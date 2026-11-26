import './auth.css';

const TABS = [
  { href: '/register/beneficiary', key: 'beneficiary', label: 'I need support' },
  { href: '/register/charity', key: 'charity', label: 'I represent a charity' },
  { href: '/login', key: 'login', label: 'Sign in' },
];

const ASIDE = {
  beneficiary: {
    title: 'Get support from approved charities',
    intro: 'Create an account to send requests for help. A coordinator connects each request with an approved charity.',
    steps: ['Create your account.', 'Sign in.', 'Send a request for support.'],
  },
  charity: {
    title: 'Register your charity',
    intro: 'Tell us about your organization. Every application is reviewed before the charity becomes active.',
    steps: ['Submit your application.', 'A coordinator reviews it.', 'Once approved, your charity is active.'],
  },
  login: {
    title: 'Welcome back',
    intro: 'Sign in with the email and password you registered with.',
    steps: null,
  },
};

export function AuthShell({ active, children }) {
  const aside = ASIDE[active];
  return (
    <div className="auth-page">
      <header className="auth-topbar">
        <a className="auth-brand" href="/" aria-label="Helping Hands home">Helping Hands</a>
        <nav aria-label="Main navigation">
          <a href="/how-it-works">How it works</a>
          <a href="/demo">Demo</a>
        </nav>
      </header>
      <main className="auth-main">
        <section className="auth-aside">
          <h1>{aside.title}</h1>
          <p>{aside.intro}</p>
          {aside.steps && (
            <ol>
              {aside.steps.map(step => <li key={step}>{step}</li>)}
            </ol>
          )}
        </section>
        <section className="auth-card">
          <nav className="auth-tabs" aria-label="Account">
            {TABS.map(tab => (
              <a
                key={tab.key}
                href={tab.href}
                aria-current={tab.key === active ? 'page' : undefined}
              >
                {tab.label}
              </a>
            ))}
          </nav>
          <div className="auth-card-body">{children}</div>
        </section>
      </main>
    </div>
  );
}