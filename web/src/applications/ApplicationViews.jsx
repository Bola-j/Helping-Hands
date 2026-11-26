export function ApplicationStatus({ status }) {
  const label = status || 'Unknown';
  return <span className={`application-status status-${label.toLowerCase()}`}>{label}</span>;
}

export function ApplicationAttachments({ application }) {
  if (!application.attachments?.length) return <p className="application-muted">No supporting documents uploaded.</p>;
  return <ul className="application-files">{application.attachments.map(file => <li key={file.id}>
    <a href={`/api/v1/attachments/${file.id}/content`} target="_blank" rel="noreferrer">{file.originalName}</a>
    <span>{Math.ceil(file.sizeBytes / 1024)} KB</span>
  </li>)}</ul>;
}

export function ApplicationHistory({ application }) {
  if (!application.decisions?.length) return <p className="application-muted">No coordinator decision has been recorded yet.</p>;
  return <ol className="application-history">{application.decisions.map((item, index) => <li key={`${item.decidedAt}-${index}`}>
    <strong>{item.decision}</strong> <time dateTime={item.decidedAt}>{new Date(item.decidedAt).toLocaleString()}</time>
    {item.reason && <p>{item.reason}</p>}
  </li>)}</ol>;
}
