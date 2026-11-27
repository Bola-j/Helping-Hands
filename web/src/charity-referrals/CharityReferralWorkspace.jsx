import { useCallback, useEffect, useMemo, useState } from 'react';
import { loadAssignedReferrals, loadOutcomes, loadReferralProgress } from './referralApi.js';
import { createOutcome, submitOutcome, updateOutcome, uploadOutcomeEvidence } from './outcomeApi.js';
import { referralError, validateEvidence, validateOutcomeReport } from './referralMessages.js';
import { ReferralQueue, ReferralReportPanel } from './ReferralViews.jsx';

export function CharityReferralWorkspace({ request, user }) {
  const [referrals, setReferrals] = useState([]); const [outcomes, setOutcomes] = useState([]); const [selectedId, setSelectedId] = useState('');
  const [quantity, setQuantity] = useState(''); const [description, setDescription] = useState(''); const [error, setError] = useState(''); const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(true); const [busy, setBusy] = useState(false); const [evidenceName, setEvidenceName] = useState(''); const [clarificationReason, setClarificationReason] = useState('');

  const refresh = useCallback(async preferredId => {
    setLoading(true); setError('');
    try {
      const [assigned, reports] = await Promise.all([loadAssignedReferrals(request), loadOutcomes(request)]);
      setReferrals(assigned); setOutcomes(reports);
      const selected = assigned.find(item => item.id === preferredId) ?? assigned[0];
      setSelectedId(selected?.id ?? '');
      const draft = reports.find(item => item.referralId === selected?.id && ['Draft', 'Clarification'].includes(item.status));
      setQuantity(draft?.reportedQuantity?.toString() ?? ''); setDescription(draft?.description ?? ''); setEvidenceName('');
    } catch (failure) { setError(referralError(failure)); }
    finally { setLoading(false); }
  }, [request]);

  useEffect(() => { void refresh(''); }, [refresh]);

  const selected = referrals.find(item => item.id === selectedId) ?? null;
  const report = useMemo(() => outcomes.find(item => item.referralId === selectedId && ['Draft', 'Clarification'].includes(item.status)) ?? null, [outcomes, selectedId]);

  useEffect(() => {
    let active = true;
    if (!selected?.requestId || report?.status !== 'Clarification') { setClarificationReason(''); return () => { active = false; }; }
    loadReferralProgress(request, selected.requestId).then(data => {
      const event = data.history?.find(item => item.entityId === report.id && item.action === 'Clarification');
      if (active) setClarificationReason(event?.reason ?? '');
    }).catch(() => { if (active) setClarificationReason(''); });
    return () => { active = false; };
  }, [request, selected?.requestId, report?.id, report?.status]);

  const choose = useCallback(referral => {
    setSelectedId(referral.id); setError(''); setNotice(''); setEvidenceName('');
    const draft = outcomes.find(item => item.referralId === referral.id && ['Draft', 'Clarification'].includes(item.status));
    setQuantity(draft?.reportedQuantity?.toString() ?? ''); setDescription(draft?.description ?? '');
  }, [outcomes]);

  async function saveDraft(event) {
    event.preventDefault();
    const accepted = outcomes.filter(item => item.referralId === selected.id && item.status === 'Accepted').reduce((sum, item) => sum + Number(item.reportedQuantity), 0);
    const problem = validateOutcomeReport(quantity, description, Math.max(0, Number(selected.assignedQuantity) - accepted));
    if (problem) { setError(problem); return; }
    setBusy(true); setError(''); setNotice('');
    try {
      const saved = report ? await updateOutcome(request, report, quantity, description) : await createOutcome(request, selected.id, quantity, description);
      setOutcomes(current => [...current.filter(item => item.id !== saved.id), saved]); setNotice(`Report ${saved.reference} saved as a draft.`);
    } catch (failure) { setError(referralError(failure)); }
    finally { setBusy(false); }
  }

  async function uploadEvidence(event) {
    event.preventDefault(); const file = event.currentTarget.elements.evidence.files[0];
    if (!report) { setError('Save your report before uploading evidence.'); return; }
    const problem = validateEvidence(file); if (problem) { setError(problem); return; }
    setBusy(true); setError(''); setNotice('');
    try { const result = await uploadOutcomeEvidence(request, report.id, file); setEvidenceName(result.originalName); setNotice(`Private evidence uploaded: ${result.originalName}.`); event.currentTarget.reset(); }
    catch (failure) { setError(referralError(failure)); }
    finally { setBusy(false); }
  }

  async function sendReport() {
    if (!report) { setError('Save your report and upload evidence before submitting it.'); return; }
    setBusy(true); setError(''); setNotice('');
    try { const submitted = await submitOutcome(request, report); setOutcomes(current => current.map(item => item.id === submitted.id ? submitted : item)); setNotice(`Report ${submitted.reference} submitted to the coordinator for review.`); }
    catch (failure) { setError(referralError(failure)); }
    finally { setBusy(false); }
  }

  return <><header className="charity-referrals-heading"><div><p className="charity-referrals-eyebrow">Charity workspace</p><h1>Assigned referrals</h1><p>Record delivery and send a report with private evidence for coordinator review.</p></div><span>Signed in as {user.displayName}</span></header>
    <nav className="charity-referrals-nav" aria-label="Charity workspaces"><a href="/charity/services">Services and capacity</a><a href="/charity/referrals">Assigned referrals</a></nav>
    {error && <p role="alert" className="charity-referrals-alert">{error}</p>}{notice && <p role="status" className="charity-referrals-notice">{notice}</p>}
    <div className="charity-referrals-layout"><ReferralQueue referrals={referrals} selectedId={selectedId} loading={loading} onSelect={choose} onRefresh={() => void refresh(selectedId)} />
      <section className="charity-referrals-detail" aria-live="polite">{loading && !referrals.length && <p role="status">Loading your assigned referrals…</p>}
        {!loading && !error && !referrals.length && <article className="charity-referrals-card"><h2>No assigned referrals</h2><p>Referrals assigned to your active charity memberships will appear here.</p></article>}
        {selected && <ReferralReportPanel referral={selected} outcomes={outcomes.filter(item => item.referralId === selected.id)} report={report} clarificationReason={clarificationReason} quantity={quantity} description={description} evidenceName={evidenceName} busy={busy} onQuantity={setQuantity} onDescription={setDescription} onSave={saveDraft} onUpload={uploadEvidence} onSubmit={sendReport} />}
      </section>
    </div>
  </>;
}
