export function DemoPage() {
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
      </section>
    </main>
  );
}