export function DailyCapacityTasks({ capacities, hasSelectedServices, refreshing, onRefresh }) {
  const tasks = capacities.filter(item => item.isStale || item.status === 'Stale');
  return <section className="charity-services-card charity-capacity-tasks" aria-labelledby="capacity-tasks-heading">
    <div className="charity-services-heading"><div><p className="charity-services-eyebrow">Today’s task</p><h2 id="capacity-tasks-heading">Daily capacity updates</h2></div><button type="button" disabled={refreshing} onClick={onRefresh}>{refreshing ? 'Refreshing…' : 'Refresh tasks'}</button></div>
    {!hasSelectedServices && <p>Select and save supported services to see their daily capacity tasks.</p>}
    {hasSelectedServices && tasks.length === 0 && <p role="status">All selected services have today’s capacity update.</p>}
    {tasks.length > 0 && <ul className="charity-capacity-task-list">{tasks.map(item => <li key={item.charityServiceId}>
      <div><strong>{item.serviceName} · {item.unitStandardName}</strong><span>Service v{item.serviceVersionNumber} · standard v{item.standardVersionNumber}</span><span>Needs an update for Cairo date {item.cairoDate}.</span></div>
      <a href={`#capacity-${item.charityServiceId}`}>Update today’s capacity</a>
    </li>)}</ul>}
  </section>;
}
