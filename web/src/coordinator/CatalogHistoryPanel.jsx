import { useCallback, useEffect, useState } from 'react';
import { catalogPage, catalogRequest } from './catalogApi';

export function CatalogHistoryPanel({ request, standard, selected, onSelect }) {
  const [items, setItems] = useState([]); const [effectiveAt, setEffectiveAt] = useState('');
  const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => setItems(await catalogPage(request, `/api/v1/catalog/standards/${standard.id}/versions`)), [request, standard.id]);
  useEffect(() => { load().catch(error => setError(error.message)); }, [load]);
  async function createVersion(event) {
    event.preventDefault(); setError(''); setBusy(true);
    try {
      const result = await catalogRequest(request, `/api/v1/catalog/standards/${standard.id}/versions`, { method: 'POST', body: { effectiveAt: new Date(effectiveAt).toISOString() } });
      await load(); onSelect(result.data); setEffectiveAt('');
    } catch (failure) { setError(failure.message); } finally { setBusy(false); }
  }
  return (
    <section className="catalog-card" aria-labelledby="history-heading">
      <h2 id="history-heading">4. Version history</h2><p className="catalog-help">{standard.name} · older versions remain visible after a new one is published.</p>
      {error && <p className="catalog-error" role="alert">{error}</p>}
      <ol className="catalog-history">{items.map(item => <li key={item.id}>
        <button type="button" className={selected?.id === item.id ? 'catalog-version-choice is-selected' : 'catalog-version-choice'} onClick={() => onSelect(item)}>
          <strong>Version {item.versionNumber}</strong><span>{new Date(item.effectiveAt).toLocaleString()}</span><span>{item.isPublished ? 'Published' : 'Draft'} · {item.components?.length ?? 0} components</span>
        </button>
      </li>)}</ol>
      <form className="catalog-form" onSubmit={createVersion}>
        <label htmlFor="effective-at">Create a new version · effective date</label><input id="effective-at" type="datetime-local" value={effectiveAt} onChange={event => setEffectiveAt(event.target.value)} required />
        <button type="submit" disabled={busy}>{busy ? 'Creating…' : 'Create draft version'}</button>
      </form>
    </section>
  );
}
