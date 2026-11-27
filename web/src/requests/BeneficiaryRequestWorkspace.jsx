import { useEffect, useState } from 'react';
import { ApiError } from '../session/ApiError';
import { readDemoBeneficiary } from '../session/readDemoBeneficiary';
import { loadCurrentCatalog, submitMultiNeedRequest } from './beneficiaryRequestApi.js';

const emptyNeed = () => ({ standardVersionId: '', quantity: '1' });

export function BeneficiaryRequestWorkspace({ request, user }) {
  const [profile, setProfile] = useState(null);
  const [catalog, setCatalog] = useState([]);
  const [needs, setNeeds] = useState([emptyNeed()]);
  const [description, setDescription] = useState('');
  const [confirmedAuthority, setConfirmedAuthority] = useState(false);
  const [agreedSharing, setAgreedSharing] = useState(false);
  const [created, setCreated] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    let active = true;
    Promise.all([readDemoBeneficiary(request, user.displayName), loadCurrentCatalog(request)])
      .then(([beneficiary, choices]) => { if (active) { setProfile(beneficiary); setCatalog(choices); } })
      .catch(failure => { if (active) setError(loadError(failure)); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [request, user.displayName]);

  function updateNeed(index, field, value) {
    setNeeds(current => current.map((need, i) => i === index ? { ...need, [field]: value } : need));
  }

  function validate() {
    if (!profile) return 'Your beneficiary profile could not be found. Sign in again or contact a coordinator.';
    if (!description.trim()) return 'Describe the support you are asking for.';
    if (needs.length < 1 || needs.length > 50) return 'Add between 1 and 50 needs.';
    if (needs.some(need => !need.standardVersionId || !Number.isFinite(Number(need.quantity)) || Number(need.quantity) <= 0)) return 'Choose a service standard and enter a quantity greater than zero for every need.';
    if (new Set(needs.map(need => need.standardVersionId)).size !== needs.length) return 'Choose a different standard for each need.';
    if (!confirmedAuthority || !agreedSharing) return 'Confirm that you have permission to submit and agree to share this request with coordinators and charities.';
    return '';
  }

  async function onSubmit(event) {
    event.preventDefault();
    const problem = validate();
    if (problem) { setError(problem); return; }
    setBusy(true); setError(''); setCreated(null);
    try {
      const payload = needs.map(need => ({ standardVersionId: need.standardVersionId, quantity: Number(need.quantity) }));
      setCreated(await submitMultiNeedRequest(request, profile.id, description, payload));
    } catch (failure) { setError(submitError(failure)); }
    finally { setBusy(false); }
  }

  if (loading) return <p role="status">Loading your profile and available services…</p>;
  return <>
    <p className="beneficiary-request-eyebrow">Beneficiary workspace</p>
    <h1>Request support</h1>
    <p className="beneficiary-request-lead">Choose the services and quantities you need. The coordinator will review your request.</p>
    <p className="beneficiary-request-signed">Signed in as {user.displayName}{profile ? ` · beneficiary ${profile.reference}` : ''}</p>
    {error && <p role="alert" className="beneficiary-request-error">{error}</p>}
    {created && <section className="beneficiary-request-success" role="status"><h2>Request submitted</h2><p>Your reference is <strong>{created.reference}</strong>. Keep it to check with your coordinator.</p><p>Status: {created.status}</p><a href="/requests/progress">View request progress</a></section>}
    <RequestForm catalog={catalog} needs={needs} description={description} authority={confirmedAuthority} sharing={agreedSharing} busy={busy} onNeed={updateNeed} onAdd={() => setNeeds(current => current.length < 50 ? [...current, emptyNeed()] : current)} onRemove={index => setNeeds(current => current.filter((_, i) => i !== index))} onDescription={setDescription} onAuthority={setConfirmedAuthority} onSharing={setAgreedSharing} onSubmit={onSubmit} />
  </>;
}

function RequestForm({ catalog, needs, description, authority, sharing, busy, onNeed, onAdd, onRemove, onDescription, onAuthority, onSharing, onSubmit }) {
  return <form className="beneficiary-request-form" onSubmit={onSubmit}>
    <label className="beneficiary-request-label">What support do you need?
      <textarea value={description} maxLength="4000" rows="4" onChange={event => onDescription(event.target.value)} />
    </label>
    <div className="beneficiary-request-heading"><h2>Services and quantities</h2><button type="button" disabled={busy || needs.length >= 50} onClick={onAdd}>Add another need</button></div>
    {needs.map((need, index) => <NeedFields key={index} index={index} need={need} catalog={catalog} needs={needs} busy={busy} onNeed={onNeed} onRemove={onRemove} />)}
    <label className="beneficiary-request-check"><input type="checkbox" checked={authority} onChange={event => onAuthority(event.target.checked)} /> I have permission to submit this request for the beneficiary.</label>
    <label className="beneficiary-request-check"><input type="checkbox" checked={sharing} onChange={event => onSharing(event.target.checked)} /> I agree that the request details can be shared with coordinators and suitable charities.</label>
    <button className="beneficiary-request-submit" type="submit" disabled={busy || catalog.length === 0}>{busy ? 'Submitting…' : 'Submit request'}</button>
    {catalog.length === 0 && <p className="beneficiary-request-help">There are no current published services to choose from.</p>}
  </form>;
}

function NeedFields({ index, need, catalog, needs, busy, onNeed, onRemove }) {
  const selected = catalog.find(choice => choice.standardVersionId === need.standardVersionId);
  return <fieldset className="beneficiary-request-need"><legend>Need {index + 1}</legend>
    <label className="beneficiary-request-label">Service and unit standard
      <select value={need.standardVersionId} disabled={busy} onChange={event => onNeed(index, 'standardVersionId', event.target.value)}>
        <option value="">Choose a current service</option>{catalog.map(choice => <option key={choice.standardVersionId} value={choice.standardVersionId} disabled={needs.some((other, i) => i !== index && other.standardVersionId === choice.standardVersionId)}>{choice.categoryCode} · {choice.serviceName} · {choice.standardName} (v{choice.standardVersionNumber})</option>)}
      </select>
    </label>
    {selected && <div className="beneficiary-request-standard"><p><strong>{selected.standardName}</strong> · {selected.serviceName}</p><p>Current standard v{selected.standardVersionNumber}; service v{selected.serviceVersionNumber}</p>{selected.components?.length > 0 && <><p>Included items:</p><ul>{selected.components.map((component, i) => <li key={`${component.name}-${i}`}>{component.name}: {component.quantity} {component.unit}</li>)}</ul></>}</div>}
    <div className="beneficiary-request-need-row"><label className="beneficiary-request-label">Quantity<input type="number" min="0.01" step="0.01" value={need.quantity} disabled={busy} onChange={event => onNeed(index, 'quantity', event.target.value)} /></label>{needs.length > 1 && <button type="button" className="beneficiary-request-remove" disabled={busy} onClick={() => onRemove(index)}>Remove need {index + 1}</button>}</div>
  </fieldset>;
}

function loadError(error) {
  if (error instanceof ApiError && error.status === 403) return 'This account cannot access beneficiary request services.';
  if (error instanceof ApiError && error.status === 401) return 'Your sign-in ended. Sign in again to continue.';
  if (error instanceof ApiError && (error.kind === 'network' || error.kind === 'unavailable')) return 'Could not load your profile or services. Check that the local services are running, then refresh.';
  return 'Your beneficiary profile or current service standards could not be loaded.';
}

function submitError(error) {
  if (error instanceof ApiError && error.status === 409) return 'A selected standard changed while you were preparing the request. Refresh the available services and choose again.';
  if (error instanceof ApiError && error.status === 400) return 'The request was not accepted. Check the description, confirmations, selected standards and quantities.';
  if (error instanceof ApiError && error.status === 404) return 'Your beneficiary profile is not linked to this account. Please sign in again or contact a coordinator.';
  return loadError(error);
}
