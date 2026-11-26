import { useEffect, useState } from 'react';
import { catalogCommand, catalogPage, catalogRequest } from './catalogApi';

export function CatalogServicePanel({ request, category, selected, onSelect }) {
  const [items, setItems] = useState([]); const [name, setName] = useState('');
  const [editing, setEditing] = useState(null); const [editName, setEditName] = useState('');
  const [error, setError] = useState(''); const [notice, setNotice] = useState(''); const [busy, setBusy] = useState(false);
  useEffect(() => { catalogPage(request, `/api/v1/catalog/categories/${category.id}/versions`).then(setItems).catch(error => setError(error.message)); }, [request, category.id]);
  async function save(event) {
    event.preventDefault(); setError(''); setNotice(''); setBusy(true);
    try {
      if (editing) await catalogCommand(request, `/api/v1/catalog/service-versions/${editing}`, 'PUT', { name: editName.trim() });
      else await catalogRequest(request, `/api/v1/catalog/categories/${category.id}/versions`, { method: 'POST', body: { name: name.trim() } });
      setItems(await catalogPage(request, `/api/v1/catalog/categories/${category.id}/versions`)); setName(''); setEditing(null); setNotice('Service saved.');
    } catch (failure) { setError(failure.message); } finally { setBusy(false); }
  }
  async function publish(id) {
    setError(''); setNotice(''); setBusy(true);
    try { await catalogCommand(request, `/api/v1/catalog/service-versions/${id}/publish`, 'POST', undefined, `/api/v1/catalog/service-versions/${id}`); setItems(await catalogPage(request, `/api/v1/catalog/categories/${category.id}/versions`)); setNotice('Service version published.'); }
    catch (failure) { setError(failure.message); } finally { setBusy(false); }
  }
  return (
    <section className="catalog-card" aria-labelledby="services-heading">
      <h2 id="services-heading">2. Services</h2><p className="catalog-help">Category <strong>{category.code}</strong>. Create a service version, then publish it before publishing its standards.</p>
      {error && <p className="catalog-error" role="alert">{error}</p>}{notice && <p className="catalog-notice" role="status">{notice}</p>}
      <ul className="catalog-list">{items.map(item => <li key={item.id} className={selected === item.id ? 'is-selected' : ''}>
        <button type="button" className="catalog-select" onClick={() => onSelect(item)}><strong>{item.name} · v{item.versionNumber}</strong><span>{item.isPublished ? 'Published' : 'Draft'} · open standards</span></button>
        {!item.isPublished && <div className="catalog-row-actions"><button type="button" className="catalog-small-button" onClick={() => { setEditing(item.id); setEditName(item.name); setName(''); }}>Edit</button><button type="button" className="catalog-small-button" disabled={busy} onClick={() => publish(item.id)}>Publish</button></div>}
      </li>)}</ul>
      <form className="catalog-form" onSubmit={save}>
        <label htmlFor="service-name">{editing ? 'Edit service name' : 'New service version'}</label>
        <input id="service-name" value={editing ? editName : name} onChange={event => editing ? setEditName(event.target.value) : setName(event.target.value)} maxLength={200} required />
        <div className="catalog-actions"><button type="submit" disabled={busy}>{busy ? 'Saving…' : editing ? 'Save service' : 'Add service'}</button>{editing && <button type="button" className="catalog-secondary" onClick={() => setEditing(null)}>Cancel edit</button>}</div>
      </form>
    </section>
  );
}
