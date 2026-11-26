import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../session/ApiError';

const categories = ['Goods', 'Money', 'OrganizationalReferral'];

export function CoordinatorRequestReviewWorkspace({ request, user }) {
  const [items, setItems] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [details, setDetails] = useState(null);
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

  const refresh = useCallback(async (preferredId = '') => {
    setLoading(true); setError('');
    try {
      const requests = await loadRequests();
      setItems(requests);
      const target = requests.some(item => item.id === preferredId) ? preferredId : requests[0]?.id ?? '';
      setSelectedId(target);
      if (target) {
        const [detailResponse, historyResponse] = await Promise.all([
          request(`/api/v1/requests/${target}`), request(`/api/v1/requests/${target}/history`),
        ]);
        setDetails(await detailResponse.json());
        setHistory(await historyResponse.json());
      } else { setDetails(null); setHistory([]); }
    } catch (failure) { setError(await explainError(failure, 'Requests could not be loaded.')); }
    finally { setLoading(false); }
  }, [loadRequests, request]);

  useEffect(() => { void refresh(''); }, []);

  async function selectRequest(id) {
    setSelectedId(id); setDetails(null); setHistory([]); setError('');
    try {
      const [detailResponse, historyResponse] = await Promise.all([
        request(`/api/v1/requests/${id}`), request(`/api/v1/requests/${id}/history`),
      ]);
      setDetails(await detailResponse.json()); setHistory(await historyResponse.json());
    } catch (failure) { setError(await explainError(failure, 'Request details could not be loaded.')); }
  }

  async function decide(need, decision, form) {
    const values = new FormData(form);
    const reason = String(values.get('reason') ?? '').trim();
    const category = String(values.get('category') ?? '');
    if ((decision === 'Rejected' || decision === 'Clarification') && !reason) {
      setError(decision === 'Rejected' ? 'Enter a reason before rejecting this need.' : 'Enter what information is needed before requesting clarification.');
      return;
    }
    if ((decision === 'Accepted' || decision === 'Rejected') && !categories.includes(category)) {
      setError('Choose a classification for this need.'); return;
    }
    setBusy(true); setError(''); setNotice('');
    try {
      const response = await request(`/api/v1/requests/${details.id}/needs/${need.id}/decisions`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ decision, category: category || null, serviceSubtype: String(values.get('subtype') ?? '').trim() || null, reason: reason || null, version: details.version }),
      });
      await response.json();
      setNotice(`${need.serviceName}: ${decision === 'Clarification' ? 'clarification requested' : decision.toLowerCase()}.`);
      await refresh(details.id);
    } catch (failure) { setError(await explainError(failure, 'The decision could not be saved. Refresh and try again.')); }
    finally { setBusy(false); }
  }

  return <>
    <p className="request-review-eyebrow">Coordinator workspace</p>
    <h1>Assistance requests</h1>
    <p className="request-review-lead">Review each need against the standard selected when the request was submitted. Use fictional records for demos.</p>
    <p className="request-review-signed">Signed in as {user.displayName}</p>
    {error && <p role="alert" className="request-review-error">{error}</p>}
    {notice && <p role="status" className="request-review-notice">{notice}</p>}
    <div className="request-review-layout">
      <aside className="request-review-card" aria-label="Request list">
        <div className="request-review-heading"><h2>Requests</h2><button type="button" onClick={() => void refresh(selectedId)} disabled={loading}>Refresh</button></div>
        {loading && <p role="status">Loading requests…</p>}
        {!loading && items.length === 0 && <p>No requests are available.</p>}
        <ul className="request-review-list">{items.map(item => <li key={item.id}>
          <button type="button" aria-current={item.id === selectedId ? 'true' : undefined} onClick={() => void selectRequest(item.id)}>
            <strong>{item.reference}</strong><span>{item.status}</span><small>{item.needs?.length ?? 0} needs{!item.isMultiNeed ? ' · legacy request' : ''}</small>
          </button>
        </li>)}</ul>
      </aside>
      <section className="request-review-detail" aria-live="polite">
        {!details && !loading && <div className="request-review-card"><h2>Select a request</h2><p>Choose a request from the list to see its needs and history.</p></div>}
        {details && <>
          <article className="request-review-card">
            <div className="request-review-heading"><div><p className="request-review-eyebrow">{details.reference}</p><h2>{details.status}</h2></div><span className="request-review-pill">{details.isMultiNeed ? 'Multi-need' : 'Legacy single-need'}</span></div>
            <p className="request-review-description">{details.description}</p>
            {details.needs?.map(need => <NeedCard key={`${need.id}-${need.status}-${details.version}`} need={need} requestId={details.id} request={request} requestStatus={details.status} busy={busy} onDecision={decide} />)}
            {!details.isMultiNeed && <p className="request-review-help">This older single-need record uses a separate review contract and cannot be decided from this multi-need workspace.</p>}
          </article>
          <article className="request-review-card"><h2>Request history</h2>
            {history.length === 0 ? <p>No request history is available.</p> : <ol className="request-review-history">{history.map((entry, index) => <li key={`${entry.action}-${entry.occurredAt}-${index}`}><strong>{entry.action}</strong>{entry.reason && <p>{entry.reason}</p>}<time>{formatDate(entry.occurredAt)}</time></li>)}</ol>}
          </article>
        </>}
      </section>
    </div>
  </>;
}

function NeedCard({ need, requestId, request, requestStatus, busy, onDecision }) {
  const canReview = need.status === 'Submitted' && ['Submitted', 'InReview', 'Clarification'].includes(requestStatus);
  const [showHistory, setShowHistory] = useState(false);
  const [history, setHistory] = useState(null);
  const [historyError, setHistoryError] = useState(false);
  return <section className="request-review-need">
    <div className="request-review-heading"><h3>{need.serviceName} · {need.standardName}</h3><span className="request-review-pill">{need.status}</span></div>
    <p>{need.categoryCode} · service v{need.serviceVersionNumber} · standard v{need.standardVersionNumber}</p>
    <p><strong>Requested quantity:</strong> {need.quantity}</p>
    {need.category && <p><strong>Classified as:</strong> {need.category}{need.serviceSubtype ? ` — ${need.serviceSubtype}` : ''}</p>}
    <details><summary>Selected standard contents</summary>
      {need.components?.length ? <ul>{need.components.map((component, index) => <li key={`${component.name}-${index}`}>{component.name}: {component.quantity} {component.unit}</li>)}</ul> : <p>No component details were recorded for this standard.</p>}
    </details>
    {canReview && <form className="request-review-form" onSubmit={event => event.preventDefault()}>
      <label>Classification<select name="category" defaultValue={need.category ?? ''}><option value="">Choose classification</option>{categories.map(item => <option key={item}>{item}</option>)}</select></label>
      <label>Service subtype (optional)<input name="subtype" defaultValue={need.serviceSubtype ?? ''} maxLength="100" /></label>
      <label>Reason or clarification request<textarea name="reason" maxLength="2000" rows="3" placeholder="Required for rejection or clarification" /></label>
      <div className="request-review-actions">
        <button type="button" disabled={busy} onClick={event => void onDecision(need, 'Accepted', event.currentTarget.form)}>Accept</button>
        <button type="button" className="request-review-secondary" disabled={busy} onClick={event => void onDecision(need, 'Clarification', event.currentTarget.form)}>Request clarification</button>
        <button type="button" className="request-review-danger" disabled={busy} onClick={event => void onDecision(need, 'Rejected', event.currentTarget.form)}>Reject</button>
      </div>
    </form>}
    {!canReview && <p className="request-review-help">This need is {need.status.toLowerCase()} and has no available coordinator decision.</p>}
    <button type="button" className="request-review-history-toggle" onClick={async () => {
      if (!showHistory && history === null) {
        try { const response = await request(`/api/v1/requests/${requestId}/needs/${need.id}/history`); setHistory(await response.json()); }
        catch { setHistory([]); setHistoryError(true); }
      }
      setShowHistory(value => !value);
    }}>{showHistory ? 'Hide need history' : 'View need history'}</button>
    {showHistory && (historyError ? <p className="request-review-help">Need history could not be loaded.</p> : <NeedHistoryEntries entries={history ?? []} />)}
  </section>;
}

function NeedHistoryEntries({ entries }) {
  if (!entries.length) return <p className="request-review-help">No need history is available.</p>;
  return <ol className="request-review-history">{entries.map((entry, index) => <li key={`${entry.action}-${entry.occurredAt}-${index}`}><strong>{entry.action}</strong>{entry.category && <span> · {entry.category}</span>}{entry.reason && <p>{entry.reason}</p>}<time>{formatDate(entry.occurredAt)}</time></li>)}</ol>;
}

async function explainError(error, fallback) {
  if (error instanceof ApiError) {
    if (error.status === 401) return 'Your session ended. Sign in again to continue.';
    if (error.status === 403) return 'Coordinator or Administrator access is required.';
    if (error.status === 409) return 'The request changed while you were reviewing it. Refresh the request and try again.';
    if (error.status === 400 && error.response) {
      const problem = await error.response.clone().json().catch(() => null);
      if (problem?.title) return problem.title;
    }
    if (error.kind === 'network' || error.kind === 'unavailable') return 'The request service could not be reached.';
  }
  return error instanceof Error ? error.message : fallback;
}

function formatDate(value) {
  const date = new Date(value);
  return Number.isNaN(date.valueOf()) ? 'Time not provided' : date.toLocaleString();
}
