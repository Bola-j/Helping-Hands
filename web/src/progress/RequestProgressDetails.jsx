function formatQuantity(value) {
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(Number(value ?? 0));
}

function NeedProgressCard({ need, index }) {
  const required = Number(need.requiredQuantity ?? 0);
  const assigned = Number(need.assignedQuantity ?? 0);
  const accepted = Number(need.acceptedQuantity ?? 0);
  const ratio = required > 0 ? Math.min(100, Math.round((accepted / required) * 100)) : 0;
  const portions = need.assignments ?? [];
  return <article className="request-progress-need"><h3>Need {index + 1}: {need.service ?? need.standard ?? 'Requested support'}</h3><p className="request-progress-standard">{need.standard ?? 'Service standard'}</p><div className="request-progress-metrics"><Metric label="Requested" value={formatQuantity(required)} /><Metric label="Assigned" value={formatQuantity(assigned)} /><Metric label="Accepted" value={formatQuantity(accepted)} /><Metric label="Still to fulfill" value={formatQuantity(need.remainingQuantity ?? Math.max(0, required - accepted))} /></div><div className="request-progress-bar" role="progressbar" aria-label={`Need ${index + 1} accepted progress`} aria-valuemin="0" aria-valuemax="100" aria-valuenow={ratio}><span style={{ width: `${ratio}%` }} /></div><p className="request-progress-bar-label">{ratio}% accepted</p>{portions.length > 0 ? <div><h4>Assigned portions</h4><ul className="request-progress-portions">{portions.map((portion, portionIndex) => <li key={portion.id ?? portionIndex}><strong>{portion.scopeDescription || portion.reference || `Portion ${portionIndex + 1}`}</strong><span>{formatQuantity(portion.acceptedQuantity)} accepted of {formatQuantity(portion.assignedQuantity)} assigned</span><small>{portion.status} · {portion.reference}</small>{portion.outcomes?.map(outcome => <small key={outcome.id}>Outcome {outcome.reference}: {formatQuantity(outcome.reportedQuantity)} reported · {outcome.status}</small>)}</li>)}</ul></div> : <p className="request-progress-muted">No portions have been assigned yet.</p>}</article>;
}

function Metric({ label, value }) {
  return <div><span>{label}</span><strong>{value}</strong></div>;
}

export function RequestProgressDetails({ progress }) {
  if (!progress) return <p className="request-progress-card">Choose a request to see its progress.</p>;
  return <article className="request-progress-card"><div className="request-progress-detail-heading"><div><p className="request-progress-eyebrow">Request reference</p><h2>{progress.reference}</h2></div><span className="request-progress-status">{progress.status}</span></div>{progress.description && <p className="request-progress-description">{progress.description}</p>}<p className="request-progress-privacy">This view shows progress only. Private evidence and uploaded documents are not displayed.</p><section aria-label="Need progress" className="request-progress-needs">{(progress.needs ?? []).map((need, index) => <NeedProgressCard key={need.id ?? index} need={need} index={index} />)}</section>{!progress.needs?.length && <p className="request-progress-muted">No individual needs are listed for this request.</p>}</article>;
}
