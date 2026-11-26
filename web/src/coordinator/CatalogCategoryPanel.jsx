import { useCallback, useEffect, useState } from 'react';
import { catalogCommand, catalogPage, catalogRequest } from './catalogApi';

export function CatalogCategoryPanel({ request, selected, onSelect }) {
  const [items, setItems] = useState([]); const [code, setCode] = useState('');
  const [editing, setEditing] = useState(null); const [editCode, setEditCode] = useState('');
  const [error, setError] = useState(''); const [notice, setNotice] = useState(''); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => setItems(await catalogPage(request, '/api/v1/catalog/categories')), [request]);
  useEffect(() => { load().catch(error => setError(error.message)); }, [load]);
  async function save(event) {
    event.preventDefault(); setError(''); setNotice(''); setBusy(true);
    try {
      if (editing) await catalogCommand(request, `/api/v1/catalog/categories/${editing}`, 'PUT', { code: editCode.trim() });
      else await catalogRequest(request, '/api/v1/catalog/categories', { method: 'POST', body: { code: code.trim() } });
      await load(); setCode(''); setEditing(null); setNotice('Category saved.');
    } catch (failure) { setError(failure.message); } finally { setBusy(false); }
  }
  return (
    <section className="catalog-card" aria-labelledby="categories-heading">
      <h2 id="categories-heading">1. Categories</h2><p className="catalog-help">Group services under a short category code.</p>
      {error && <p className="catalog-error" role="alert">{error}</p>}{notice && <p className="catalog-notice" role="status">{notice}</p>}
      <ul className="catalog-list">{items.map(item => <li key={item.id} className={selected === item.id ? 'is-selected' : ''}>
        <button type="button" className="catalog-select" onClick={() => onSelect(item)}><strong>{item.code}</strong><span>Open services</span></button>
        <button type="button" className="catalog-small-button" onClick={() => { setEditing(item.id); setEditCode(item.code); setCode(''); }}>Edit</button>
      </li>)}</ul>
      <form className="catalog-form" onSubmit={save}>
        <label htmlFor="category-code">{editing ? 'Edit category code' : 'New category code'}</label>
        <input id="category-code" value={editing ? editCode : code} onChange={event => editing ? setEditCode(event.target.value) : setCode(event.target.value)} maxLength={50} required />
        <div className="catalog-actions"><button type="submit" disabled={busy}>{busy ? 'Saving…' : editing ? 'Save category' : 'Add category'}</button>{editing && <button type="button" className="catalog-secondary" onClick={() => setEditing(null)}>Cancel edit</button>}</div>
      </form>
    </section>
  );
}
