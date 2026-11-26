import { ApplicationAttachments, ApplicationHistory, ApplicationStatus } from './ApplicationViews';

export function ApplicationDetails({ application }) {
  return <section className="application-card" aria-labelledby="application-detail-heading">
    <div className="application-heading"><div><p className="application-eyebrow">Reference {application.reference}</p>
      <h2 id="application-detail-heading">{application.displayName}</h2></div><ApplicationStatus status={application.approvalStatus} /></div>
    <dl className="application-facts"><div><dt>Contact details</dt><dd>{application.contactDetails || 'Not provided'}</dd></div>
      <div><dt>Registration information</dt><dd>{application.registrationInformation || 'Not provided'}</dd></div></dl>
    <h3>Supporting documents</h3><ApplicationAttachments application={application} />
    <h3>Review history</h3><ApplicationHistory application={application} />
  </section>;
}
