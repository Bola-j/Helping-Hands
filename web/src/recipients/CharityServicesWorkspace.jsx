import { useEffect, useState } from 'react';
import { loadCapacity, loadCharities, loadServiceChoices } from './charityServicesReadApi.js';
import { saveDailyCapacity, saveServiceChoices } from './charityServicesWriteApi.js';
import { charityServicesError } from './charityServicesMessages.js';
import { CapacityList, ServiceSelection } from './CharityServicesSections.jsx';
import { DailyCapacityTasks } from './DailyCapacityTasks.jsx';

export function CharityServicesWorkspace({ request, user }) {
  const [charities, setCharities] = useState([]);
  const [recipientId, setRecipientId] = useState('');
  const [services, setServices] = useState(null);
  const [capacities, setCapacities] = useState([]);
  const [selectedPairs, setSelectedPairs] = useState([]);
  const [capacityValues, setCapacityValues] = useState({});
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [loadingCharities, setLoadingCharities] = useState(true);
  const [loadingWorkspace, setLoadingWorkspace] = useState(false);
  const [savingServices, setSavingServices] = useState(false);
  const [savingCapacity, setSavingCapacity] = useState(false);
  const [refreshingTasks, setRefreshingTasks] = useState(false);

  useEffect(() => {
    let active = true;
    loadCharities(request).then(items => {
      if (!active) return;
      setCharities(items);
      if (items.length === 1) setRecipientId(items[0].id);
    }).catch(failure => { if (active) setError(charityServicesError(failure, 'load your charities')); })
      .finally(() => { if (active) setLoadingCharities(false); });
    return () => { active = false; };
  }, [request]);

  useEffect(() => {
    if (!recipientId) {
      setServices(null); setCapacities([]); setSelectedPairs([]); setCapacityValues({});
      return undefined;
    }
    let active = true;
    setLoadingWorkspace(true); setServices(null); setCapacities([]); setError('');
    Promise.all([loadServiceChoices(request, recipientId), loadCapacity(request, recipientId)])
      .then(([serviceData, capacityData]) => {
        if (!active) return;
        setServices(serviceData);
        setCapacities(capacityData.capacities ?? []);
        setSelectedPairs((serviceData.selectedServices ?? []).map(pairKey));
        setCapacityValues(Object.fromEntries((capacityData.capacities ?? []).map(item => [item.charityServiceId, item.available ?? ''])));
      }).catch(failure => { if (active) setError(charityServicesError(failure, 'load service and capacity details')); })
      .finally(() => { if (active) setLoadingWorkspace(false); });
    return () => { active = false; };
  }, [request, recipientId]);

  function toggleService(option) {
    const key = pairKey(option);
    setSelectedPairs(current => current.includes(key) ? current.filter(item => item !== key) : [...current, key]);
  }

  async function submitServices(event) {
    event.preventDefault();
    setSavingServices(true); setError(''); setNotice('');
    try {
      const servicesToSave = selectedPairs.map(key => {
        const [serviceVersionId, standardVersionId] = key.split('|');
        return { serviceVersionId, standardVersionId };
      });
      const result = await saveServiceChoices(request, recipientId, servicesToSave);
      setServices(current => ({ ...current, selectedServices: result.selectedServices }));
      setSelectedPairs((result.selectedServices ?? []).map(pairKey));
      setNotice('Supported services saved. Update today’s available units for each service below.');
      try {
        const latest = await loadCapacity(request, recipientId);
        setCapacities(latest.capacities ?? []);
        setCapacityValues(Object.fromEntries((latest.capacities ?? []).map(item => [item.charityServiceId, item.available ?? ''])));
      } catch (failure) {
        setError(`Services were saved, but today’s capacity could not be refreshed. ${charityServicesError(failure, 'load capacity')}`);
      }
    } catch (failure) { setError(charityServicesError(failure, 'save the selected services')); }
    finally { setSavingServices(false); }
  }

  async function submitCapacity(event, capacity) {
    event.preventDefault();
    const amount = capacityValues[capacity.charityServiceId];
    if (amount === '' || !Number.isFinite(Number(amount)) || Number(amount) < 0 || Number(amount) > 999999999999999) {
      setError('Enter an available quantity of zero or more.'); return;
    }
    setSavingCapacity(true); setError(''); setNotice('');
    let saved = false;
    try {
      await saveDailyCapacity(request, recipientId, capacity.charityServiceId, amount);
      saved = true;
      const result = await loadCapacity(request, recipientId);
      setCapacities(result.capacities ?? []);
      const updated = result.capacities?.find(item => item.charityServiceId === capacity.charityServiceId);
      setCapacityValues(current => ({ ...current, [capacity.charityServiceId]: updated?.available ?? '' }));
      setNotice(`${capacity.serviceName} capacity updated for ${result.cairoDate}.`);
    } catch (failure) {
      setError(saved ? 'Capacity was saved, but the task list could not refresh. Use Refresh tasks and check the latest status.' : charityServicesError(failure, 'save capacity'));
    }
    finally { setSavingCapacity(false); }
  }

  async function refreshTasks() {
    setRefreshingTasks(true); setError('');
    try {
      const latest = await loadCapacity(request, recipientId);
      setCapacities(latest.capacities ?? []);
      setCapacityValues(Object.fromEntries((latest.capacities ?? []).map(item => [item.charityServiceId, item.available ?? ''])));
    } catch (failure) { setError(charityServicesError(failure, 'refresh today’s capacity tasks')); }
    finally { setRefreshingTasks(false); }
  }

  return <>
    <p className="charity-services-eyebrow">Charity workspace</p>
    <h1>Services and capacity</h1>
    <p className="charity-services-lead">Choose the services and unit standards your organization supports, then update how many units are available today.</p>
    <p className="charity-services-signed">Signed in as {user.displayName}</p>
    {loadingCharities && <p role="status">Loading your charity accounts…</p>}
    {!loadingCharities && charities.length === 0 && <p>No active charity membership is linked to this account.</p>}
    {charities.length > 1 && <label className="charity-services-org">Organization<select value={recipientId} onChange={event => setRecipientId(event.target.value)}><option value="">Choose an organization</option>{charities.map(item => <option key={item.id} value={item.id}>{item.displayName} · {item.reference}</option>)}</select></label>}
    {error && <p role="alert" className="charity-services-error">{error}</p>}
    {notice && <p role="status" className="charity-services-notice">{notice}</p>}
    {loadingWorkspace && <p role="status">Loading service standards and today’s capacity…</p>}
    {services && !loadingWorkspace && <>
      <DailyCapacityTasks capacities={capacities} hasSelectedServices={(services.selectedServices ?? []).length > 0} refreshing={refreshingTasks} onRefresh={refreshTasks} />
      <ServiceSelection options={services.applicableOptions ?? []} selectedPairs={selectedPairs} busy={savingServices} onToggle={toggleService} onSubmit={submitServices} />
      <CapacityList capacities={capacities} values={capacityValues} disabled={savingServices || savingCapacity} onChange={(id, value) => setCapacityValues(current => ({ ...current, [id]: value }))} onSubmit={submitCapacity} />
    </>}
  </>;
}

function pairKey(pair) { return `${pair.serviceVersionId}|${pair.standardVersionId}`; }
