export function ServiceSelection({ options, selectedPairs, busy, onToggle, onSubmit }) {
  return <form className="charity-services-card" onSubmit={onSubmit}>
    <div className="charity-services-heading"><div><h2>Supported services</h2><p>Select each service and platform unit your charity can provide.</p></div><button disabled={busy}>{busy ? 'Saving…' : 'Save services'}</button></div>
    {!options.length && <p>No published service standards are available yet.</p>}
    <div className="charity-services-options">{options.map(option => <ServiceOption key={option.standardVersionId} option={option} checked={selectedPairs.includes(`${option.serviceVersionId}|${option.standardVersionId}`)} disabled={busy} onToggle={onToggle} />)}</div>
  </form>;
}

function ServiceOption({ option, checked, disabled, onToggle }) {
  return <article className="charity-services-option">
    <label className="charity-services-option-title"><input type="checkbox" checked={checked} disabled={disabled} onChange={() => onToggle(option)} /><span><strong>{option.serviceName}</strong><small>{option.categoryCode} · service v{option.serviceVersionNumber}</small></span></label>
    <p><strong>{option.unitStandardName}</strong> · standard v{option.standardVersionNumber} · effective {formatDate(option.effectiveAt)}</p>
    <details><summary>View standard contents</summary>
      {option.components?.length ? <ul>{option.components.map((component, index) => <li key={`${component.name}-${index}`}>{component.name}: {component.quantity} {component.unit}</li>)}</ul> : <p>This standard has no component details.</p>}
    </details>
  </article>;
}

export function CapacityList({ capacities, values, disabled, onChange, onSubmit }) {
  return <section className="charity-services-card" aria-labelledby="capacity-heading">
    <div className="charity-services-heading"><div><h2 id="capacity-heading">Available today</h2><p>Enter the total units available from now. A new number replaces today’s balance.</p></div></div>
    {!capacities.length && <p>Select and save at least one service to manage capacity.</p>}
    <div className="charity-capacity-list">{capacities.map(item => <CapacityRow key={item.charityServiceId} item={item} value={values[item.charityServiceId] ?? ''} disabled={disabled} onChange={onChange} onSubmit={onSubmit} />)}</div>
  </section>;
}

function CapacityRow({ item, value, disabled, onChange, onSubmit }) {
  const stale = item.isStale || item.status === 'Stale';
  const statusClass = stale ? 'capacity-stale' : item.status === 'Zero' ? 'capacity-zero' : 'capacity-current';
  return <article id={`capacity-${item.charityServiceId}`} className="charity-capacity-row">
    <div className="charity-services-heading"><h3>{item.serviceName} · {item.unitStandardName}</h3><span className={`charity-capacity-status ${statusClass}`}>{stale ? 'Stale — update needed' : item.status === 'Zero' ? 'Zero available' : 'Updated today'}</span></div>
    <p>Service v{item.serviceVersionNumber} · standard v{item.standardVersionNumber} · Cairo date {item.cairoDate}</p>
    <p><strong>Remaining units available now:</strong> {stale ? 'Not updated today' : `${item.available} units`}</p>
    <p><strong>Last update:</strong> {item.updatedAt ? formatDate(item.updatedAt, true) : 'No update for today'}{item.lastUpdatedCairoDate ? ` · Cairo date ${item.lastUpdatedCairoDate}` : ''}</p>
    {stale && item.lastKnownAvailable !== null && item.lastKnownAvailable !== undefined && <p>Previous known balance: {item.lastKnownAvailable} units from {item.lastUpdatedCairoDate} (stale; not available for new assignments).</p>}
    <form className="charity-capacity-form" onSubmit={event => onSubmit(event, item)}>
      <label>Available units from now<input type="number" min="0" max="999999999999999" step="0.001" required value={value} disabled={disabled} onChange={event => onChange(item.charityServiceId, event.target.value)} /></label>
      <button disabled={disabled}>{disabled ? 'Saving…' : stale ? 'Update today’s capacity' : 'Save capacity'}</button>
    </form>
  </article>;
}

function formatDate(value, dateTime = false) {
  if (!value) return 'Not provided';
  const parsed = new Date(value);
  return Number.isNaN(parsed.valueOf()) ? 'Not provided' : parsed.toLocaleString(undefined, dateTime ? { dateStyle: 'medium', timeStyle: 'short' } : { dateStyle: 'medium' });
}
