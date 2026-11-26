import { useState } from 'react';
import { ApplicationDetails } from './ApplicationDetails';
import { ApplicationShell } from './ApplicationGate';
import { decideApplication } from './applicationApi';
import { applicationError } from './applicationError';
import { useRecipientApplications } from './useRecipientApplications';

export function CoordinatorApplicationsWorkspace({ request, user }) {
  const data = useRecipientApplications(request);
  const [reason, setReason] = useState(''); const [busy, setBusy] = useState(false);
  async function decide(decision) {
    if (!data.application || (decision === 'Rejected' && !reason.trim())) { data.setError('Enter a reason before rejecting this application.'); return; }
    setBusy(true); data.setError('');
    try { await decideApplication(request, data.application, decision, reason.trim()); setReason(''); await data.refresh(); }
    catch (error) { data.setError(await applicationError(error, 'The decision could not be saved. Refresh and try again.')); }
    finally { setBusy(false); }
  }
  return <ApplicationShell><p className="application-eyebrow">Coordinator workspace</p><h1>Charity applications</h1><p className="application-lead">Review submitted details and documents, then record an approval or a reasoned rejection.</p>
    {data.error && <p className="application-error" role="alert">{data.error}</p>}{data.loading && <p role="status">Loading applications…</p>}
    <div className="application-layout"><nav className="application-card" aria-label="Applications"><h2>Applications</h2>{data.items.length === 0 && !data.loading && <p>No applications to review.</p>}
      <ul className="application-list">{data.items.map(item => <li key={item.id}><button className={item.id === data.selectedId ? 'is-selected' : ''} onClick={() => { data.setSelectedId(item.id); data.setError(''); }}>
        <strong>{item.displayName}</strong><span>{item.reference} · {item.approvalStatus}</span></button></li>)}</ul></nav>
      <div>{data.application && <><ApplicationDetails application={data.application} />{data.application.approvalStatus === 'Pending' && <section className="application-card application-form"><h2>Record a decision</h2>
        <label htmlFor="decision-reason">Reason (required for rejection)</label><textarea id="decision-reason" maxLength="2000" value={reason} onChange={event => setReason(event.target.value)} />
        <div className="application-actions"><button disabled={busy} onClick={() => void decide('Approved')}>Approve</button><button className="button-reject" disabled={busy || !reason.trim()} onClick={() => void decide('Rejected')}>Reject</button></div></section>}</>}</div>
    </div><p className="application-lead">Signed in as {user.displayName}</p></ApplicationShell>;
}
