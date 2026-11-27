import { useCallback, useEffect, useRef, useState } from 'react';
import { loadOwnRequests, loadRequestProgress } from './requestProgressApi.js';
import { requestListError, requestProgressError } from './progressError.js';
import { RequestProgressList } from './RequestProgressList.jsx';
import { RequestProgressDetails } from './RequestProgressDetails.jsx';

export function BeneficiaryRequestProgressWorkspace({ request, user }) {
  const [requests, setRequests] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [progress, setProgress] = useState(null);
  const [listError, setListError] = useState('');
  const [progressError, setProgressError] = useState('');
  const [listLoading, setListLoading] = useState(true);
  const [progressLoading, setProgressLoading] = useState(false);
  const selectionGeneration = useRef(0);

  const selectRequest = useCallback(async id => {
    const generation = ++selectionGeneration.current;
    setSelectedId(id); setProgress(null); setProgressError(''); setProgressLoading(true);
    try {
      const result = await loadRequestProgress(request, id);
      if (generation === selectionGeneration.current) setProgress(result);
    } catch (failure) {
      if (generation === selectionGeneration.current) setProgressError(requestProgressError(failure));
    } finally { if (generation === selectionGeneration.current) setProgressLoading(false); }
  }, [request]);

  const refreshRequests = useCallback(async preferredId => {
    setListLoading(true); setListError('');
    try {
      const items = await loadOwnRequests(request);
      setRequests(items);
      const nextId = items.some(item => item.id === preferredId) ? preferredId : items[0]?.id ?? '';
      if (nextId) await selectRequest(nextId);
      else { selectionGeneration.current++; setSelectedId(''); setProgress(null); setProgressError(''); }
    } catch (failure) { setListError(requestListError(failure)); }
    finally { setListLoading(false); }
  }, [request, selectRequest]);

  useEffect(() => { void refreshRequests(''); }, [refreshRequests]);

  return <>
    <div className="request-progress-heading"><div><p className="request-progress-eyebrow">Beneficiary workspace</p><h1>Your request progress</h1><p>Follow each request and see how its needs are being fulfilled.</p></div><span>Signed in as {user.displayName}</span></div>
    {listError && <p role="alert" className="request-progress-error">{listError}</p>}
    <div className="request-progress-layout">
      <aside className="request-progress-sidebar">
        <div className="request-progress-list-heading"><h2>Your requests</h2><button type="button" onClick={() => void refreshRequests(selectedId)} disabled={listLoading}>{listLoading ? 'Refreshing…' : 'Refresh'}</button></div>
        {listLoading && !requests.length && <p role="status">Loading your requests…</p>}
        {!listLoading && !listError && !requests.length && <RequestProgressList requests={[]} selectedId={selectedId} onSelect={selectRequest} />}
        {!!requests.length && <RequestProgressList requests={requests} selectedId={selectedId} onSelect={selectRequest} />}
      </aside>
      <section className="request-progress-main" aria-live="polite">
        {progressLoading && <p role="status" className="request-progress-card">Loading request progress…</p>}
        {progressError && <div className="request-progress-error-box"><p role="alert">{progressError}</p><button type="button" onClick={() => void selectRequest(selectedId)}>Try again</button></div>}
        {progress && <RequestProgressDetails progress={progress} />}
        {!listLoading && !listError && !requests.length && <p className="request-progress-empty-hint">Submitted requests will appear here.</p>}
        {!progress && !progressLoading && !progressError && !!requests.length && <RequestProgressDetails progress={null} />}
      </section>
    </div>
  </>;
}
