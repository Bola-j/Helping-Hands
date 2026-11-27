import { SessionProvider } from './session/SessionProvider';
import React from 'react';
import { createRoot } from 'react-dom/client';
import './site.css';
import { DemoPage } from './DemoPage.jsx';
import { BeneficiarySignupPage } from './auth/BeneficiarySignupPage.jsx';
import { CharitySignupPage } from './auth/CharitySignupPage.jsx';
import { LoginPage } from './auth/LoginPage.jsx';
import { CoordinatorStandardsPage } from './coordinator/CoordinatorStandardsPage.jsx';
import { CharityApplicationPage } from './applications/CharityApplicationPage.jsx';
import { CoordinatorApplicationsPage } from './applications/CoordinatorApplicationsPage.jsx';
import './applications/applications.css';
import { CoordinatorRequestReviewPage } from './requests/CoordinatorRequestReviewPage.jsx';
import './requests/request-review.css';
import { BeneficiaryRequestPage } from './requests/BeneficiaryRequestPage.jsx';
import './requests/beneficiary-request.css';
import { CharityServicesPage } from './recipients/CharityServicesPage.jsx';
import './recipients/charity-services.css';
import { CoordinatorMatchingPage } from './referrals/CoordinatorMatchingPage.jsx';
import './referrals/matching.css';

function HomePage() {
  return (
    <main className="page">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Helping Hands home">Helping Hands</a>
        <nav aria-label="Main navigation">
          <a href="/how-it-works">How it works</a> <a href="/demo">Demo</a> <a href="/login">Sign in</a>
        </nav>
      </header>
      <section className="hero">
        <p className="eyebrow">Community care, coordinated</p>
        <h1>Coordinate requests for assistance.</h1>
        <p>Helping Hands connects people who need support with charities ready to help.</p>
        <a className="button" href="/how-it-works">See how it works</a> <a className="button" href="/register/beneficiary">Create an account</a>
      </section>
    </main>
  );
}

function HowItWorksPage() {
  return (
    <main className="page">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Helping Hands home">Helping Hands</a>
        <nav aria-label="Main navigation">
          <a href="/">Home</a> <a href="/demo">Demo</a> <a href="/login">Sign in</a>
        </nav>
      </header>
      <section className="content">
        <p className="eyebrow">A clear path to support</p>
        <h1>How it works</h1>
        <p>A person makes a request and a coordinator helps connect it with an approved charity.</p>
      </section>
    </main>
  );
}

function App() {
  const path = window.location.pathname.replace(/\/+$/, '') || '/';
  if (path === '/how-it-works') return <HowItWorksPage />;
  if (path === '/demo') return <SessionProvider><DemoPage /></SessionProvider>;
  if (path === '/register/beneficiary') return <BeneficiarySignupPage />;
  if (path === '/register/charity') return <CharitySignupPage />;
  if (path === '/login') return <SessionProvider><LoginPage /></SessionProvider>;
  if (path === '/coordinator/standards') return <SessionProvider><CoordinatorStandardsPage /></SessionProvider>;
  if (path === '/charity/application') return <SessionProvider><CharityApplicationPage /></SessionProvider>;
  if (path === '/coordinator/applications') return <SessionProvider><CoordinatorApplicationsPage /></SessionProvider>;
  if (path === '/coordinator/requests') return <SessionProvider><CoordinatorRequestReviewPage /></SessionProvider>;
  if (path === '/coordinator/matching') return <SessionProvider><CoordinatorMatchingPage /></SessionProvider>;
  if (path === '/requests/new') return <SessionProvider><BeneficiaryRequestPage /></SessionProvider>;
  if (path === '/charity/services') return <SessionProvider><CharityServicesPage /></SessionProvider>;
  return <HomePage />;
}

createRoot(document.getElementById('root')).render(<App />);
