import { useState } from 'react';
import { ApplicationDetails } from './ApplicationDetails';
import { ApplicationShell } from './ApplicationGate';
import { uploadApplicationFile } from './applicationApi';
import { applicationError } from './applicationError';
import { useRecipientApplications } from './useRecipientApplications';

export function CharityApplicationWorkspace({ request, user }) {
  const data = useRecipientApplications(request);
  const [busy, setBusy] = useState(false);
  const [notice, setNotice] = useState('');
  async function submitFile(event) {
    event.preventDefault(); const form = event.currentTarget; const file = form.elements.document.files[0];
    if (!file || file.size > 10 * 1024 * 1024 || !['application/pdf', 'image/jpeg', 'image/png'].includes(file.type)) { data.setError('Choose one PDF, JPEG or PNG file up to 10 MiB.'); return; }
    setBusy(true); data.setError(''); setNotice('');
    try { await uploadApplicationFile(request, data.application.id, file); setNotice('Document uploaded. Your application is still pending review.'); form.reset(); await data.refresh(); }
    catch (error) { data.setError(await applicationError(error, 'The document could not be uploaded. Check the file and try again.')); }
    finally { setBusy(false); }
  }
  return <ApplicationShell><p className="application-eyebrow">Charity account</p><h1>Your application</h1><p className="application-lead">Signed in as {user.displayName}. Documents are visible only to your organization and authorized staff.</p>
    {data.loading && <p role="status">Loading your application…</p>}{data.error && <p className="application-error" role="alert">{data.error}</p>}{notice && <p className="application-notice" role="status">{notice}</p>}
    {data.application && <><ApplicationDetails application={data.application} />{data.application.approvalStatus === 'Pending' && <form className="application-card application-form" onSubmit={submitFile}>
      <h2>Add a supporting document</h2><p>Accepted: PDF, JPEG or PNG, up to 10 MiB.</p><label htmlFor="application-document">Choose a document</label><input id="application-document" name="document" type="file" accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png" />
      <button disabled={busy}>{busy ? 'Uploading…' : 'Upload document'}</button></form>}</>}
    {!data.loading && !data.application && !data.error && <p>No charity application is linked to this account.</p>}
  </ApplicationShell>;
}
