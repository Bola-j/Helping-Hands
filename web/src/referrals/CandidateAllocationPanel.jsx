import { useState } from 'react';

export function CandidateAllocationPanel({ need, matchData, remaining, busy, onSubmit }) {
  const [quantities, setQuantities] = useState({});
  const [scope, setScope] = useState('');
  const [error, setError] = useState('');
  const candidates = matchData?.candidates ?? [];

  function changeQuantity(id, value) { setQuantities(current => ({ ...current, [id]: value })); }
  function submit(event) {
    event.preventDefault();
    const legs = candidates.map(item => ({ candidate: item, quantity: Number(quantities[item.recipientId] ?? 0) })).filter(item => item.quantity > 0);
    const total = legs.reduce((sum, item) => sum + item.quantity, 0);
    if (!legs.length) { setError('Enter an amount for at least one charity.'); return; }
    if (!scope.trim()) { setError('Describe what each charity is being asked to provide.'); return; }
    if (legs.some(item => !Number.isFinite(item.quantity) || Math.round(item.quantity * 1000) !== item.quantity * 1000 || item.quantity > item.candidate.availableCapacity)) { setError('Each amount must use up to three decimal places and stay within that charity’s available capacity.'); return; }
    if (total > remaining) { setError(`The total cannot exceed the ${remaining} units still unassigned.`); return; }
    setError('');
    void onSubmit(legs, scope.trim());
  }

  if (!need) return <section className="matching-card"><h2>Select an accepted need</h2><p>Choose an accepted need to see exact-version matches.</p></section>;
  if (need.status !== 'Accepted') return <section className="matching-card"><h2>{need.serviceName} · {need.standardName}</h2><p className="matching-explain">This need is {need.status.toLowerCase()}. Only accepted needs can be assigned.</p></section>;
  if (remaining <= 0) return <section className="matching-card"><h2>{need.serviceName} · {need.standardName}</h2><p className="matching-success">All {need.quantity} units are already assigned.</p></section>;

  return <section className="matching-card matching-candidates" aria-live="polite">
    <div className="matching-panel-title"><div><p className="matching-eyebrow">Exact standard · {need.categoryCode}</p><h2>{need.serviceName}</h2></div><span className="matching-pill">{remaining} units to assign</span></div>
    <p>{need.standardName} · service v{need.serviceVersionNumber} · standard v{need.standardVersionNumber}</p>
    <p className="matching-date">Matching for Cairo date {matchData?.cairoDate ?? '…'} · No standard conversion is made.</p>
    {matchData === null && <p role="status">Checking charities with today’s exact service and standard version…</p>}
    {matchData?.unavailable && <p className="matching-explain">The match list could not be loaded. Use Refresh after checking the API connection.</p>}
    {matchData && !matchData.unavailable && !candidates.length && <p className="matching-explain">No eligible charities have positive capacity for this exact service and standard today. A service without today’s Cairo-date update is frozen for new assignments; zero-capacity or differently versioned services are not eligible.</p>}
    {error && <p role="alert" className="matching-error">{error}</p>}
    {matchData && candidates.length > 0 && <form onSubmit={submit}>
      <div className="matching-candidate-list">{candidates.map(candidate => <article key={candidate.recipientId} className="matching-candidate">
        <div><h3>{candidate.charityName}</h3><p>{candidate.serviceName} v{candidate.serviceVersionNumber} · {candidate.unitStandardName} v{candidate.standardVersionNumber}</p><p>Available today: <strong>{candidate.availableCapacity}</strong> · updated {new Date(candidate.capacityUpdatedAt).toLocaleString()}</p></div>
        <label>Assign units<input type="number" min="0" max={Math.min(remaining, candidate.availableCapacity)} step="0.001" value={quantities[candidate.recipientId] ?? ''} onChange={event => changeQuantity(candidate.recipientId, event.target.value)} disabled={busy} /></label>
      </article>)}</div>
      <label className="matching-scope">Allocation details<textarea rows="3" maxLength="2000" value={scope} onChange={event => setScope(event.target.value)} disabled={busy} placeholder="Describe the support to be provided" /></label>
      <div className="matching-submit-row"><p>Split totals can’t exceed {remaining} unassigned units.</p><button type="submit" disabled={busy}>{busy ? 'Saving…' : 'Create allocation'}</button></div>
    </form>}
  </section>;
}
