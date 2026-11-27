import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../session/ApiError';

export function CoordinatorFollowUpClosureWorkspace({ request, user }) {
  const [items, setItems] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [details, setDetails] = useState(null);
  const [progress, setProgress] = useState(null);
  const [history, setHistory] = useState([]);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const loadRequests = useCallback(async () => {
    const all = [];
    let cursor = '';
    do {
      const query = new URLSearchParams({ limit: '100' });
      if (cursor) query.set('cursor', cursor);
      const response = await request(`/api/v1/requests?${query}`);
      all.push(...await response.json());
      cursor = response.headers.get('X-Next-Cursor') ?? '';
    } while (cursor);
    return all;
  }, [request]);

  const loadRequest = useCallback(async id => {
    const [detailResponse, progressResponse, historyResponse] = await Promise.all([
      request(`/api/v1/requests/${id}`), request(`/api/v1/requests/${id}/progress`), request(`/api/v1/requests/${id}/history`),
    ]);
    setDetails(await detailResponse.json());
    setProgress(await progressResponse.json());
    setHistory(await historyResponse.json());
  }, [request]);

  const refresh = useCallback(async (preferredId = '') => {
    setLoading(true); setError('');
    try {
      const requests = await loadRequests();
      setItems(requests);
      const target = requests.some(item => item.id === preferredId) ? preferredId : requests[0]?.id ?? '';
      setSelectedId(target);
      if (target) await loadRequest(target);
      else { setDetails(null); setProgress(null); setHistory([]); }
    } catch (failure) { setError(await explainError(failure, 'Requests could not be loaded.')); }
    finally { setLoading(false); }
  }, [loadRequests, loadRequest]);

  useEffect(() => { void refresh(); }, [refresh]);

  async function selectRequest(id) {
    setSelectedId(id); setDetails(null); setProgress(null); setHistory([]); setError(''); setNotice('');
    try { await loadRequest(id); }
    catch (failure) { setError(await explainError(failure, 'Request details could not be loaded.')); }
  }

  async function reviewOutcome(outcome, form, decision) {
    const reason = String(new FormData(form).get('reason') ?? '').trim();
    if (decision !== 'Accepted' && !reason) {
      setError(decision === 'Clarification' ? 'Explain what needs clarification before sending the outcome back.' : 'Enter a reason before marking the outcome unresolved.');
      return;
    }
    setBusy(true); setError(''); setNotice('');
    try {
      await request(`/api/v1/outcomes/${outcome.id}/reviews`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ decision, reason: reason || null, version: outcome.version }),
      });
      setNotice(`${outcome.reference}: ${decision === 'Clarification' ? 'clarification requested' : decision.toLowerCase()}.`);
      await refresh(details.id);
    } catch (failure) { setError(await explainError(failure, 'The outcome review could not be saved. Refresh and try again.')); }
    finally { setBusy(false); }
  }

  async function closeRequest() {
    if (!details || !closureReadiness(progress).ready) return;
    if (!window.confirm(`Close request ${details.reference}? This records a coordinator closure in the request history.`)) return;
    setBusy(true); setError(''); setNotice('');
    try {
      await request(`/api/v1/requests/${details.id}/closure`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ version: details.version }),
      });
      setNotice(`${details.reference} was closed.`);
      await refresh(details.id);
    } catch (failure) { setError(await explainError(failure, 'The request could not be closed. Refresh to see the latest quantities and decisions.')); }
    finally { setBusy(false); }
  }

  const readiness = closureReadiness(progress, details?.status);
  return <>
    <p className="follow-up-eyebrow">Coordinator workspace</p>
    <h1>Follow-up and closure</h1>
    <p className="follow-up-lead">Review assigned work, reported quantities, private evidence and decisions. Close a request only after every required need is accepted and fulfilled.</p>
    <p className="follow-up-signed">Signed in as {user.displayName}</p>
    {error && <p role="alert" className="follow-up-error">{error}</p>}
    {notice && <p role="status" className="follow-up-notice">{notice}</p>}
    <div className="follow-up-layout">
      <aside className="follow-up-card" aria-label="Request list">
        <div className="follow-up-heading"><h2>Requests</h2><button type="button" onClick={() => void refresh(selectedId)} disabled={loading}>Refresh</button></div>
        {loading && <p role="status">Loading requests…</p>}
        {!loading && items.length === 0 && <p>No requests are available.</p>}
        <ul className="follow-up-list">{items.map(item => <li key={item.id}><button type="button" aria-current={item.id === selectedId ? 'true' : undefined} onClick={() => void selectRequest(item.id)}><strong>{item.reference}</strong><span>{item.status}</span><small>{item.needs?.length ?? 0} needs</small></button></li>)}</ul>
      </aside>
      <section className="follow-up-detail" aria-live="polite">
        {!details && !loading && <div className="follow-up-card"><h2>Select a request</h2><p>Choose a request to review its assignments, evidence, quantities and history.</p></div>}
        {details && progress && <>
          <article className="follow-up-card">
            <div className="follow-up-heading"><div><p className="follow-up-eyebrow">{details.reference}</p><h2>{details.status}</h2></div><span className="follow-up-pill">{details.isMultiNeed ? 'Multi-need' : 'Single-need'}</span></div>
            <p className="follow-up-description">{details.description}</p>
            <h3>Needs, assignments and reported work</h3>
            {progress.needs?.map((need, index) => <NeedFollowUp key={need.needId ?? `legacy-${index}`} need={need} requestStatus={details.status} busy={busy} onReview={reviewOutcome} />)}
          </article>
          <article className="follow-up-card">
            <div className="follow-up-heading"><h2>Closure check</h2><span className={`follow-up-pill ${readiness.ready ? 'follow-up-ready' : ''}`}>{details.status === 'Completed' ? 'Already completed' : readiness.ready ? 'Ready to close' : 'Cannot close yet'}</span></div>
            {details.status === 'Completed' ? <p className="follow-up-help">This request is already completed. Its closure event appears in request history.</p> : readiness.ready ? <p>Every required need is accepted and the accepted quantity meets the requested quantity. Closing records this coordinator action in the request history.</p> : <><p>The request stays open until it is in a closable state and each required need has an accepted decision and enough accepted quantity.</p><ul className="follow-up-blockers">{readiness.blockers.map(item => <li key={item}>{item}</li>)}</ul></>}
            {details.status !== 'Completed' && <button type="button" className="follow-up-close" disabled={busy || !readiness.ready} onClick={() => void closeRequest()}>Close request</button>}
          </article>
          <article className="follow-up-card"><h2>Request history</h2>
            {!history.length && !progress.history?.length ? <p>No request history is available.</p> : <ol className="follow-up-history">{[...history.map(entry => ({ entityType: 'Request', ...entry })), ...(progress.history ?? [])]
              .filter((entry, index, all) => all.findIndex(other => other.action === entry.action && other.occurredAt === entry.occurredAt) === index)
              .sort((a, b) => new Date(a.occurredAt) - new Date(b.occurredAt))
              .map((entry, index) => <li key={`${entry.action}-${entry.occurredAt}-${index}`}><strong>{entry.entityType ? `${entry.entityType}: ` : ''}{entry.action}</strong>{entry.reason && <p>{entry.reason}</p>}<time>{formatDate(entry.occurredAt)}</time></li>)}</ol>}
          </article>
        </>}
      </section>
    </div>
  </>;
}

function NeedFollowUp({ need, requestStatus, busy, onReview }) {
  return <section className="follow-up-need">
    <div className="follow-up-heading"><h4>{need.service || 'Assistance need'}{need.standard ? ` · ${need.standard}` : ''}</h4><span className="follow-up-pill">{need.status || 'Status unavailable'}</span></div>
    <dl className="follow-up-quantities"><div><dt>Requested</dt><dd>{formatQuantity(need.requiredQuantity)}</dd></div><div><dt>Assigned</dt><dd>{formatQuantity(need.assignedQuantity)}</dd></div><div><dt>Accepted</dt><dd>{formatQuantity(need.acceptedQuantity)}</dd></div><div><dt>Still needed</dt><dd>{formatQuantity(need.remainingQuantity)}</dd></div></dl>
    {!need.assignments?.length && <p className="follow-up-help">No charity assignment is recorded for this need.</p>}
    {need.assignments?.map(assignment => <article className="follow-up-assignment" key={assignment.id}>
      <div className="follow-up-heading"><strong>{assignment.reference}</strong><span className="follow-up-pill">{assignment.status}</span></div>
      <p>{assignment.scopeDescription || 'No assignment description provided.'}</p>
      <p><strong>Assigned:</strong> {formatQuantity(assignment.assignedQuantity)} · <strong>Accepted:</strong> {formatQuantity(assignment.acceptedQuantity)}</p>
      {!assignment.outcomes?.length && <p className="follow-up-help">No delivery report has been submitted for this assignment.</p>}
      {assignment.outcomes?.map(outcome => <OutcomeReport key={outcome.id} outcome={outcome} requestStatus={requestStatus} busy={busy} onReview={onReview} />)}
    </article>)}
  </section>;
}

function OutcomeReport({ outcome, requestStatus, busy, onReview }) {
  const canReview = outcome.status === 'Submitted' && requestStatus === 'Open';
  return <section className="follow-up-outcome">
    <div className="follow-up-heading"><strong>{outcome.reference}</strong><span className="follow-up-pill">{outcome.status}</span></div>
    <p>{outcome.description}</p>
    <p><strong>Reported quantity:</strong> {formatQuantity(outcome.reportedQuantity)}{outcome.submittedAt && <> · <strong>Submitted:</strong> {formatDate(outcome.submittedAt)}</>}</p>
    <h5>Evidence</h5>
    {!outcome.evidence?.length ? <p className="follow-up-help">No evidence attachment metadata is available for this report.</p> : <ul className="follow-up-evidence">{outcome.evidence.map(file => <li key={file.id}><a href={`/api/v1/attachments/${file.id}/content`} target="_blank" rel="noreferrer">{file.originalName}</a><span>{file.mediaType} · {formatBytes(file.sizeBytes)} · uploaded {formatDate(file.uploadedAt)}</span></li>)}</ul>}
    {canReview && <form className="follow-up-review" onSubmit={event => event.preventDefault()}>
      <label>Review note (required for clarification or unresolved)<textarea name="reason" maxLength="2000" rows="2" /></label>
      <div className="follow-up-actions"><button type="button" disabled={busy} onClick={event => void onReview(outcome, event.currentTarget.form, 'Accepted')}>Accept reported quantity</button><button type="button" className="follow-up-secondary" disabled={busy} onClick={event => void onReview(outcome, event.currentTarget.form, 'Clarification')}>Request clarification</button><button type="button" className="follow-up-danger" disabled={busy} onClick={event => void onReview(outcome, event.currentTarget.form, 'Unresolved')}>Mark unresolved</button></div>
    </form>}
  </section>;
}

function closureReadiness(progress, requestStatus) {
  const blockers = [];
  if (requestStatus !== 'Open' && requestStatus !== 'Completed') blockers.push(`Request status is ${requestStatus || 'unknown'}; the API only allows closure from Open.`);
  if (!progress?.needs?.length) blockers.push('No required needs are recorded.');
  for (const need of progress?.needs ?? []) {
    const label = need.service || 'Assistance need';
    if (need.needId != null && need.status !== 'Accepted') blockers.push(`${label}: need status is ${need.status || 'unknown'}, not accepted.`);
    if (need.remainingQuantity == null || need.remainingQuantity > 0) blockers.push(`${label}: ${formatQuantity(need.remainingQuantity)} quantity remains unfulfilled.`);
  }
  return { ready: blockers.length === 0, blockers };
}

function formatQuantity(value) { return value == null ? 'Not available' : new Intl.NumberFormat(undefined, { maximumFractionDigits: 3 }).format(value); }
function formatBytes(value) { return value == null ? 'size unavailable' : `${(value / 1024 / 1024).toFixed(value >= 1024 * 1024 ? 1 : 2)} MiB`; }
function formatDate(value) { const date = new Date(value); return Number.isNaN(date.valueOf()) ? 'Time not provided' : date.toLocaleString(); }

async function explainError(error, fallback) {
  if (error instanceof ApiError) {
    if (error.status === 401) return 'Your session ended. Sign in again to continue.';
    if (error.status === 403) return 'Coordinator or Administrator access is required.';
    if (error.status === 409) return 'The request or outcome changed, or the API closure rules are not satisfied. Refresh and review the latest progress.';
    if (error.status === 400 && error.response) {
      const problem = await error.response.clone().json().catch(() => null);
      if (problem?.title) return problem.title;
    }
    if (error.kind === 'network' || error.kind === 'unavailable') return 'The request service could not be reached.';
  }
  return error instanceof Error ? error.message : fallback;
}
