import React from 'react';
import { createRoot } from 'react-dom/client';
import './site.css';

function HomePage() {
  return (
    <main className="page">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Helping Hands home">Helping Hands</a>
        <nav aria-label="Main navigation"><a href="/how-it-works">How it works</a></nav>
      </header>
      <section className="hero">
        <p className="eyebrow">Community care, coordinated</p>
        <h1>Coordinate requests for assistance.</h1>
        <p>Helping Hands connects people who need support with charities ready to help.</p>
        <a className="button" href="/how-it-works">See how it works</a>
      </section>
    </main>
  );
}

function HowItWorksPage() {
  return (
    <main className="page">
      <header className="topbar">
        <a className="brand" href="/" aria-label="Helping Hands home">Helping Hands</a>
        <nav aria-label="Main navigation"><a href="/">Home</a></nav>
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
  return window.location.pathname === '/how-it-works' ? <HowItWorksPage /> : <HomePage />;
}

createRoot(document.getElementById('root')).render(<App />);
