export function RequestProgressList({ requests, selectedId, onSelect }) {
  if (!requests.length) return <div className="request-progress-empty"><h3>No requests yet</h3><p>When you submit a request, you can follow its progress here.</p><a className="request-progress-action" href="/requests/new">Make a request</a></div>;
  return <ul className="request-progress-list">{requests.map(item => <li key={item.id}><button type="button" className={selectedId === item.id ? 'is-selected' : ''} aria-current={selectedId === item.id ? 'true' : undefined} onClick={() => onSelect(item.id)}><strong>{item.reference}</strong><span>{item.status}</span><small>{item.needs?.length ?? 0} {item.needs?.length === 1 ? 'need' : 'needs'}</small></button></li>)}</ul>;
}
