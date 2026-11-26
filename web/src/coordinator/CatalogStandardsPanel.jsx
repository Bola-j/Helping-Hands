import { useCallback, useEffect, useState } from 'react';
import { catalogCommand, catalogPage, catalogRequest } from './catalogApi';

export function CatalogStandardsPanel({ request, service, selected, onSelect }) {
  const [items, setItems] = useState([]); const [name, setName] = useState('');
  const [editing, setEditing] = useState(null); const [editName, setEditName] = useState('');
  const [error, setError] = useState(''); const [notice, setNotice] = useState(''); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => setItems(await catalogPage(request, `/api/v1/catalog/service-versions/${service.id}/standards`)), [request, service.id]);
  useEffect(() => { load().catch(error => setError(error.message)); }, [load]);
  async function save(event) {
    event.preventDefault(); setError(''); setNotice(''); setBusy(true);
    try {
      if (editing) await catalogCommand(request, `/api/v1/catalog/standards/${editing}`, 'PUT', { name: editName.trim() });
      else await catalogRequest(request, `/api/v1/catalog/service-versions/${service.id}/standards`, { method: 'POST', body: { name: name.trim() } });
      await load(); setName(''); setEditing(null); setNotice('Unit standard saved.');
    } catch (failure) { setError(failure.message); } finally { setBusy(false); }
  }
  return (
    <section className="catalog-card" aria-labelledby="standards-heading">
      <h2 id="standards-heading">3. Unit standards</h2><p className="catalog-help">{service.name} · service v{service.versionNumber} ({service.isPublished ? 'published' : 'draft'})</p>
      {error && <p className="catalog-error" role="alert">{error}</p>}{notice && <p className="catalog-notice" role="status">{notice}</p>}
      <ul className="catalog-list">{items.map(item => <li key={item.id} className={selected === item.id ? 'is-selected' : ''}>
        <button type="button" className="catalog-select" onClick={() => onSelect(item)}><strong>{item.name}</strong><span>Open version history</span></button>
        <button type="button" className="catalog-small-button" onClick={() => { setEditing(item.id); setEditName(item.name); setName(''); }}>Edit</button>
      </li>)}</ul>
      <form className="catalog-form" onSubmit={save}>
        <label htmlFor="standard-name">{editing ? 'Edit standard name' : 'New unit standard'}</label>
        <input id="standard-name" value={editing ? editName : name} onChange={event => editing ? setEditName(event.target.value) : setName(event.target.value)} maxLength={200} required />
        <div className="catalog-actions"><button type="submit" disabled={busy}>{busy ? 'Saving…' : editing ? 'Save standard' : 'Add standard'}</button>{editing && <button type="button" className="catalog-secondary" onClick={() => setEditing(null)}>Cancel edit</button>}</div>
      </form>
    </section>
  );
}
