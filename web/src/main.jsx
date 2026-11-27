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
import { CoordinatorFollowUpClosurePage } from './requests/CoordinatorFollowUpClosurePage.jsx';
import './requests/follow-up-closure.css';
import { BeneficiaryRequestPage } from './requests/BeneficiaryRequestPage.jsx';
import './requests/beneficiary-request.css';
import { CharityServicesPage } from './recipients/CharityServicesPage.jsx';
import './recipients/charity-services.css';
import { CoordinatorMatchingPage } from './referrals/CoordinatorMatchingPage.jsx';
import './referrals/matching.css';
import { BeneficiaryRequestProgressPage } from './progress/BeneficiaryRequestProgressPage.jsx';
import './progress/request-progress.css';
import { CharityReferralPage } from './charity-referrals/CharityReferralPage.jsx';
import './charity-referrals/charity-referrals.css';
import './ui-system.css';

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
        <div className="hero-actions">
          <a className="button" href="/register/beneficiary">Create a beneficiary account</a>
          <a className="button button-secondary" href="/how-it-works">See how it works</a>
        </div>
      </section>
      <section className="home-paths" aria-labelledby="home-paths-heading">
        <div className="home-section-heading"><p className="eyebrow">Choose your next step</p><h2 id="home-paths-heading">Support, coordinated clearly</h2><p>Start with the path that fits you. You can return here at any time.</p></div>
        <div className="home-path-grid">
          <article className="home-path-card"><span className="home-path-number">01</span><h3>Request support</h3><p>Share what help is needed and follow the request as it moves through review.</p><a href="/register/beneficiary">Create an account <span aria-hidden="true">→</span></a></article>
          <article className="home-path-card"><span className="home-path-number">02</span><h3>Coordinate assistance</h3><p>Review requests, match needs with services, and follow up on reported outcomes.</p><a href="/login">Sign in to your workspace <span aria-hidden="true">→</span></a></article>
          <article className="home-path-card"><span className="home-path-number">03</span><h3>Partner as a charity</h3><p>Submit an application, manage service capacity, and report on assigned referrals.</p><a href="/register/charity">Register your charity <span aria-hidden="true">→</span></a></article>
        </div>
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
        <ol className="process-steps">
          <li><span>01</span><div><h2>Describe the need</h2><p>A beneficiary submits a request and can review its progress.</p></div></li>
          <li><span>02</span><div><h2>Review and match</h2><p>A coordinator reviews each need and connects it with a suitable charity service.</p></div></li>
          <li><span>03</span><div><h2>Follow up</h2><p>The charity reports on assigned work, and the coordinator reviews the outcome and request history.</p></div></li>
        </ol>
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
  if (path === '/coordinator/follow-up') return <SessionProvider><CoordinatorFollowUpClosurePage /></SessionProvider>;
  if (path === '/coordinator/matching') return <SessionProvider><CoordinatorMatchingPage /></SessionProvider>;
  if (path === '/requests/new') return <SessionProvider><BeneficiaryRequestPage /></SessionProvider>;
  if (path === '/requests/progress') return <SessionProvider><BeneficiaryRequestProgressPage /></SessionProvider>;
  if (path === '/charity/referrals') return <SessionProvider><CharityReferralPage /></SessionProvider>;
  if (path === '/charity/services') return <SessionProvider><CharityServicesPage /></SessionProvider>;
  return <HomePage />;
}

createRoot(document.getElementById('root')).render(<App />);
