export function MatchingQueue({ items, selectedId, loading, onSelect, onRefresh }) {
  return <aside className="matching-card matching-queue" aria-label="Request queue">
    <div className="matching-panel-title"><div><p className="matching-eyebrow">Queue</p><h2>Requests</h2></div><button type="button" onClick={onRefresh} disabled={loading}>Refresh</button></div>
    {loading && <p role="status">Loading requests…</p>}
    {!loading && items.length === 0 && <p>No requests are available.</p>}
    <ul>{items.map(item => <li key={item.id}><button type="button" aria-current={item.id === selectedId ? 'true' : undefined} onClick={() => onSelect(item.id)}><strong>{item.reference}</strong><span>{item.status}</span><small>{item.needs?.length ?? 0} needs</small></button></li>)}</ul>
  </aside>;
}

export function NeedQueue({ needs, progress, selectedId, onSelect }) {
  return <nav className="matching-needs" aria-label="Request needs">
    <h3>Needs in this request</h3>
    {needs.map(need => {
      const assigned = progress.needs?.find(item => item.needId === need.id)?.assignedQuantity ?? 0;
      const left = Math.max(0, Number(need.quantity) - Number(assigned));
      return <button key={need.id} type="button" aria-current={need.id === selectedId ? 'true' : undefined} onClick={() => onSelect(need.id)}>
        <strong>{need.serviceName} · {need.standardName}</strong><span>{need.status} · {left} unassigned</span>
      </button>;
    })}
  </nav>;
}
