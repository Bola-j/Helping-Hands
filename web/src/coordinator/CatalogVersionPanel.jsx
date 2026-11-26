import { useCallback, useEffect, useState } from 'react';
import { catalogCommand, catalogRequest } from './catalogApi';
import { CatalogComponentsPanel } from './CatalogComponentsPanel';

export function CatalogVersionPanel({ request, versionId }) {
  const [detail, setDetail] = useState(null); const [effectiveAt, setEffectiveAt] = useState('');
  const [error, setError] = useState(''); const [notice, setNotice] = useState(''); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => {
    const result = await catalogRequest(request, `/api/v1/catalog/standard-versions/${versionId}`);
    setDetail(result.data);
    const date = new Date(result.data.effectiveAt); date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
    setEffectiveAt(date.toISOString().slice(0, 16));
  }, [request, versionId]);
  useEffect(() => { load().catch(error => setError(error.message)); }, [load]);
  async function save(event) {
    event.preventDefault(); setError(''); setNotice(''); setBusy(true);
    try {
      if (event.nativeEvent.submitter?.value === 'publish') {
        await catalogCommand(request, `/api/v1/catalog/standard-versions/${versionId}/publish`, 'POST', undefined, `/api/v1/catalog/standard-versions/${versionId}`);
        setNotice('Standard version published.');
      } else {
        await catalogCommand(request, `/api/v1/catalog/standard-versions/${versionId}`, 'PUT', { effectiveAt: new Date(effectiveAt).toISOString() });
        setNotice('Effective date saved.');
      }
      await load();
    } catch (failure) { setError(failure.message); } finally { setBusy(false); }
  }
  if (!detail) return <section className="catalog-card"><p role="status">Loading version…</p>{error && <p className="catalog-error" role="alert">{error}</p>}</section>;
  return (
    <section className="catalog-card catalog-version-detail" aria-labelledby="version-detail-heading">
      <h2 id="version-detail-heading">Version {detail.versionNumber}</h2><p className="catalog-help">{detail.isPublished ? 'Published · read only' : 'Draft · add at least one component before publishing'} · history stays attached to existing records.</p>
      {error && <p className="catalog-error" role="alert">{error}</p>}{notice && <p className="catalog-notice" role="status">{notice}</p>}
      <form className="catalog-form" onSubmit={save}>
        <label htmlFor="version-effective">Effective date</label><input id="version-effective" type="datetime-local" value={effectiveAt} disabled={detail.isPublished} onChange={event => setEffectiveAt(event.target.value)} required />
        <div className="catalog-actions">{!detail.isPublished && <><button type="submit" value="save" disabled={busy}>{busy ? 'Saving…' : 'Save draft'}</button><button type="submit" value="publish" disabled={busy || !detail.components?.length}>{busy ? 'Saving…' : 'Publish version'}</button></>}</div>
      </form>
      <CatalogComponentsPanel request={request} version={detail} onChanged={load} onError={setError} />
    </section>
  );
}
