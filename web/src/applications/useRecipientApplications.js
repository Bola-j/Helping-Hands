import { useCallback, useEffect, useRef, useState } from 'react';
import { loadRecipient } from './applicationApi';
import { applicationError } from './applicationError';

export function useRecipientApplications(request) {
  const [items, setItems] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const selectedIdRef = useRef(selectedId);
  selectedIdRef.current = selectedId;
  const [application, setApplication] = useState(null);
  const [error, setError] = useState('');
  const [listLoading, setListLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(true);
  const refresh = useCallback(async () => {
    setListLoading(true); setError('');
    try {
      const response = await request('/api/v1/recipients');
      const list = await response.json();
      setItems(list);
      const currentId = selectedIdRef.current;
      const target = list.some(item => item.id === currentId) ? currentId : list[0]?.id ?? '';
      setSelectedId(target);
      if (target && target !== currentId) { setDetailLoading(true); setApplication(null); }
      if (target && target === currentId) {
        setDetailLoading(true);
        try { setApplication(await loadRecipient(request, target)); }
        finally { setDetailLoading(false); }
      }
      else if (!target) { setApplication(null); setDetailLoading(false); }
    } catch (failure) { setError(await applicationError(failure, 'Applications could not be loaded.')); }
    finally { setListLoading(false); }
  }, [request]);

  useEffect(() => { void refresh(); }, [refresh]);
  useEffect(() => {
    if (!selectedId) { setApplication(null); setDetailLoading(false); return; }
    let current = true;
    setApplication(null); setDetailLoading(true);
    loadRecipient(request, selectedId).then(value => { if (current) setApplication(value); })
      .catch(async failure => { if (current) setError(await applicationError(failure, 'Application details could not be loaded.')); })
      .finally(() => { if (current) setDetailLoading(false); });
    return () => { current = false; };
  }, [request, selectedId]);
  return { items, selectedId, setSelectedId, application, setApplication, error, setError, loading: listLoading || detailLoading, refresh };
}
