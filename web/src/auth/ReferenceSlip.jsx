import { useEffect, useRef } from 'react';

// Shown after a successful signup. Focus moves to the heading so screen
// readers announce the result.
export function ReferenceSlip({ tone, heading, reference, status, action, children }) {
  const headingRef = useRef(null);
  useEffect(() => { headingRef.current?.focus(); }, []);
  return (
    <section className={`auth-slip auth-slip-${tone}`} aria-labelledby="slip-heading">
      <h2 id="slip-heading" ref={headingRef} tabIndex={-1}>{heading}</h2>
      <div className="auth-slip-ref">
        <span className="auth-slip-label">Reference</span>
        <strong>{reference || 'Not returned'}</strong>
        {status && <span className="auth-badge">{status}</span>}
      </div>
      <div className="auth-slip-body">{children}</div>
      {action}
    </section>
  );
}