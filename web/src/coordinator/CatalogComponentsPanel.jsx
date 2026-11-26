import { useState } from 'react';
import { catalogCommand, catalogRequest } from './catalogApi';

const EMPTY_COMPONENT = { name: '', quantity: '', unit: '' };

export function CatalogComponentsPanel({ request, version, onChanged, onError }) {
  const [editing, setEditing] = useState(null); const [values, setValues] = useState(EMPTY_COMPONENT); const [busy, setBusy] = useState(false);
  async function save(event) {
    event.preventDefault(); onError(''); setBusy(true);
    const body = { name: values.name.trim(), quantity: Number(values.quantity), unit: values.unit.trim() };
    try {
      if (editing) await catalogCommand(request, `/api/v1/catalog/components/${editing}`, 'PUT', body);
      else await catalogRequest(request, `/api/v1/catalog/standard-versions/${version.id}/components`, { method: 'POST', body });
      await onChanged(); setValues(EMPTY_COMPONENT); setEditing(null);
    } catch (error) { onError(error.message); } finally { setBusy(false); }
  }
  async function remove(id) {
    onError(''); setBusy(true);
    try { await catalogCommand(request, `/api/v1/catalog/components/${id}`, 'DELETE'); await onChanged(); }
    catch (error) { onError(error.message); } finally { setBusy(false); }
  }
  return (
    <div className="catalog-components">
      <h3>Components and quantities</h3>
      {version.components?.length ? <ul className="catalog-components-list">{version.components.map(item => <li key={item.id}>
        <span><strong>{item.name}</strong><small>{item.quantity} {item.unit}</small></span>
        {!version.isPublished && <div className="catalog-row-actions"><button type="button" className="catalog-small-button" onClick={() => { setEditing(item.id); setValues({ name: item.name, quantity: String(item.quantity), unit: item.unit }); }}>Edit</button><button type="button" className="catalog-small-button" disabled={busy} onClick={() => remove(item.id)}>Remove</button></div>}
      </li>)}</ul> : <p className="catalog-empty">Add at least one component to publish this standard version.</p>}
      {!version.isPublished && <form className="catalog-form" onSubmit={save}>
        <h4>{editing ? 'Edit component' : 'Add component'}</h4>
        <label htmlFor="component-name">Component name</label><input id="component-name" value={values.name} maxLength={200} required onChange={event => setValues({ ...values, name: event.target.value })} />
        <div className="catalog-fields"><label htmlFor="component-quantity">Quantity<input id="component-quantity" type="number" min="0.001" max="999999999999999" step="0.001" value={values.quantity} required onChange={event => setValues({ ...values, quantity: event.target.value })} /></label><label htmlFor="component-unit">Unit<input id="component-unit" value={values.unit} maxLength={50} required onChange={event => setValues({ ...values, unit: event.target.value })} /></label></div>
        <div className="catalog-actions"><button type="submit" disabled={busy}>{busy ? 'Saving…' : editing ? 'Save component' : 'Add component'}</button>{editing && <button type="button" className="catalog-secondary" onClick={() => { setEditing(null); setValues(EMPTY_COMPONENT); }}>Cancel edit</button>}</div>
        <p className="catalog-help">Use a positive quantity with up to three decimal places. A version supports up to 100 components.</p>
      </form>}
    </div>
  );
}
