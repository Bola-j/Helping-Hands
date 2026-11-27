import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { loadCoordinatorRequests, loadRequestForMatching } from './matchingApi.requests.js';
import { loadEligibleCharities, submitSplitAllocation } from './matchingApi.allocations.js';
import { CandidateAllocationPanel } from './CandidateAllocationPanel.jsx';
import { MatchingQueue, NeedQueue } from './MatchingQueue.jsx';

export function CoordinatorMatchingWorkspace({ request, user }) {
  const [items, setItems] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [selectedNeedId, setSelectedNeedId] = useState('');
  const [details, setDetails] = useState(null);
  const [progress, setProgress] = useState({ needs: [] });
  const [matchData, setMatchData] = useState(null);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const selectionGeneration = useRef(0);

  const refreshList = useCallback(async preferredId => {
    setLoading(true); setError('');
    try {
      const requests = await loadCoordinatorRequests(request);
      setItems(requests);
      const target = requests.some(item => item.id === preferredId) ? preferredId : requests[0]?.id ?? '';
      if (target) await selectRequest(target);
      else { setSelectedId(''); setDetails(null); setProgress({ needs: [] }); setMatchData(null); }
    } catch (failure) { setError(explainMatchingError(failure, 'Requests could not be loaded.')); }
    finally { setLoading(false); }
  }, [request]);

  const selectRequest = useCallback(async id => {
    const generation = ++selectionGeneration.current;
    setSelectedId(id); setSelectedNeedId(''); setDetails(null); setProgress({ needs: [] }); setMatchData(null); setError(''); setNotice('');
    try {
      const result = await loadRequestForMatching(request, id);
      if (generation !== selectionGeneration.current) return;
      setDetails(result.details); setProgress(result.progress);
      setSelectedNeedId(result.details.needs?.find(need => need.status === 'Accepted')?.id ?? result.details.needs?.[0]?.id ?? '');
    } catch (failure) { if (generation === selectionGeneration.current) setError(explainMatchingError(failure, 'Request details and allocation progress could not be loaded.')); }
  }, [request]);

  useEffect(() => { void refreshList(''); }, [refreshList]);

  const selectedNeed = useMemo(() => details?.needs?.find(need => need.id === selectedNeedId) ?? null, [details, selectedNeedId]);
  const assigned = progress.needs?.find(item => item.needId === selectedNeedId)?.assignedQuantity ?? 0;
  const remaining = selectedNeed ? Math.max(0, Number(selectedNeed.quantity) - Number(assigned)) : 0;

  useEffect(() => {
    let current = true;
    if (!details || !selectedNeed || selectedNeed.status !== 'Accepted' || remaining <= 0) { setMatchData(null); return () => { current = false; }; }
    setMatchData(null); setError('');
    loadEligibleCharities(request, details.id, selectedNeed)
      .then(data => { if (current) setMatchData(data); })
      .catch(failure => { if (current) { setMatchData({ candidates: [], unavailable: true }); setError(explainMatchingError(failure, 'Eligible charities could not be loaded.')); } });
    return () => { current = false; };
  }, [request, details, selectedNeed, remaining]);

  async function allocate(legs, scope) {
    setBusy(true); setError(''); setNotice('');
    try {
      const allocations = legs.map(({ candidate, quantity }) => ({
        recipientId: candidate.recipientId,
        serviceVersionId: selectedNeed.serviceVersionId,
        standardVersionId: selectedNeed.standardVersionId,
        quantity,
        scopeDescription: scope,
      }));
      const result = await submitSplitAllocation(request, details.id, selectedNeed.id, details.version, allocations);
      const summary = (result.legs ?? []).map(leg => `${leg.allocatedQuantity} units · ${leg.referralReference}`).join('; ');
      setNotice(summary ? `Allocation created: ${summary}` : 'Allocation created.');
      await refreshList(details.id);
    } catch (failure) {
      if (failure?.status === 409) {
        await refreshList(details?.id);
        setError(explainMatchingError(failure, 'Allocation could not be saved. Refresh and try again.'));
      } else setError(explainMatchingError(failure, 'Allocation could not be saved. Refresh the request and try again.'));
    } finally { setBusy(false); }
  }

  return <>
    <header className="matching-page-heading"><div><p className="matching-eyebrow">Coordinator workspace</p><h1>Match &amp; refer</h1><p>Choose an accepted need, compare charities using its exact recorded versions, then allocate all or part of its remaining quantity.</p></div><span className="matching-user">Signed in as {user.displayName}</span></header>
    <nav className="matching-workspace-nav" aria-label="Coordinator workspaces"><a href="/coordinator/requests">Review requests</a><a href="/coordinator/standards">Service standards</a><a href="/coordinator/applications">Charity applications</a></nav>
    {error && <p role="alert" className="matching-error">{error}</p>}
    {notice && <p role="status" className="matching-success">{notice}</p>}
    <div className="matching-layout">
      <MatchingQueue items={items} selectedId={selectedId} loading={loading} onSelect={selectRequest} onRefresh={() => void refreshList(selectedId)} />
      <section className="matching-detail" aria-live="polite">
        {!details && !loading && <article className="matching-card"><h2>Select a request</h2><p>Choose a request from the list to inspect its needs.</p></article>}
        {details && <>
          <article className="matching-card matching-request-summary"><p className="matching-eyebrow">{details.reference} · {details.status}</p><h2>{details.description || 'Request needs'}</h2><p>Request quantity and assignment progress are refreshed after each successful allocation.</p></article>
          <article className="matching-card"><NeedQueue needs={details.needs ?? []} progress={progress} selectedId={selectedNeedId} onSelect={setSelectedNeedId} /></article>
          <CandidateAllocationPanel key={`${details.id}-${selectedNeedId}-${details.version}`} need={selectedNeed} matchData={matchData} remaining={remaining} busy={busy} onSubmit={allocate} />
        </>}
      </section>
    </div>
  </>;
}

function explainMatchingError(error, fallback) {
  if (error?.status === 409) return 'The request or capacity changed while you were matching. The request has been refreshed; check the remaining quantity and current matches before trying again.';
  if (error?.status === 401) return 'Your session ended. Sign in again to continue.';
  if (error?.status === 403) return 'Coordinator or Administrator access is required.';
  if (error?.kind === 'network' || error?.kind === 'unavailable') return 'The matching service could not be reached. Check that the local API is running.';
  return error instanceof Error ? error.message : fallback;
}
