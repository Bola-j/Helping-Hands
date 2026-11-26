import { AuthShell } from './AuthShell';
import { Field, FormAlert, PasswordField, SubmitButton } from './fields';
import { ReferenceSlip } from './ReferenceSlip';
import { useSignupForm } from './useSignupForm';
import { PASSWORD_MIN, required, validEmail, validPassword } from './validation';
import { registerCharity } from '../session/registerCharity';

const INITIAL = {
  representativeDisplayName: '', email: '', password: '',
  organizationName: '', contactDetails: '', registrationInformation: '',
};

function validate(values) {
  return {
    representativeDisplayName: required(values.representativeDisplayName, 'Enter your name.'),
    email: validEmail(values.email),
    password: validPassword(values.password),
    organizationName: required(values.organizationName, 'Enter the name of your organization.'),
    contactDetails: required(values.contactDetails, 'Enter contact details for the organization.'),
  };
}

function submit(values) {
  return registerCharity({
    representativeDisplayName: values.representativeDisplayName.trim(),
    email: values.email.trim(),
    password: values.password,
    organizationName: values.organizationName.trim(),
    contactDetails: values.contactDetails.trim(),
    registrationInformation: values.registrationInformation,
  });
}

export function CharitySignupPage() {
  const { values, errors, formError, busy, result, setValue, onSubmit } =
    useSignupForm({ initial: INITIAL, validate, submit });

  return (
    <AuthShell active="charity">
      {result ? (
        <ReferenceSlip
          tone="pending"
          heading="Application received"
          reference={result.reference}
          status="Pending review"
          action={<a className="auth-button" href="/login">Sign in</a>}
        >
          <p>A coordinator has to approve this application. Your charity stays inactive until then.</p>
          <p>Creating this account did not sign you in.</p>
        </ReferenceSlip>
      ) : (
        <form onSubmit={onSubmit} noValidate>
          <h2>Representative and organization</h2>
          <FormAlert>{formError}</FormAlert>
          <Field id="representativeDisplayName" label="Your name" autoComplete="name"
            value={values.representativeDisplayName} error={errors.representativeDisplayName}
            onChange={v => setValue('representativeDisplayName', v)} />
          <Field id="email" type="email" label="Email" autoComplete="email"
            value={values.email} error={errors.email} onChange={v => setValue('email', v)} />
          <PasswordField id="password" label="Password" autoComplete="new-password"
            hint={`At least ${PASSWORD_MIN} characters.`}
            value={values.password} error={errors.password} onChange={v => setValue('password', v)} />
          <Field id="organizationName" label="Organization name" autoComplete="organization"
            value={values.organizationName} error={errors.organizationName}
            onChange={v => setValue('organizationName', v)} />
          <Field id="contactDetails" label="Organization contact details" multiline
            hint="A phone number, address or another way to reach the organization."
            value={values.contactDetails} error={errors.contactDetails}
            onChange={v => setValue('contactDetails', v)} />
          <Field id="registrationInformation" label="Registration information" optional multiline
            hint="A charity registration number or other details that help a coordinator review the application."
            value={values.registrationInformation} error={errors.registrationInformation}
            onChange={v => setValue('registrationInformation', v)} />
          <SubmitButton busy={busy} label="Submit application" busyLabel="Submitting application…" />
        </form>
      )}
    </AuthShell>
  );
}