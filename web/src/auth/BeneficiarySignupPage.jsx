import { AuthShell } from './AuthShell';
import { CheckField, Field, FormAlert, PasswordField, SubmitButton } from './fields';
import { ReferenceSlip } from './ReferenceSlip';
import { useSignupForm } from './useSignupForm';
import { PASSWORD_MIN, required, validEmail, validPassword } from './validation';
import { registerBeneficiary } from '../session/registerBeneficiary';

const INITIAL = { displayName: '', email: '', password: '', contactDetails: '', isFamily: false };

function validate(values) {
  return {
    displayName: required(values.displayName, 'Enter your name.'),
    email: validEmail(values.email),
    password: validPassword(values.password),
  };
}

function submit(values) {
  return registerBeneficiary({
    displayName: values.displayName.trim(),
    email: values.email.trim(),
    password: values.password,
    contactDetails: values.contactDetails,
    isFamily: values.isFamily,
  });
}

export function BeneficiarySignupPage() {
  const { values, errors, formError, busy, result, setValue, onSubmit } =
    useSignupForm({ initial: INITIAL, validate, submit });

  return (
    <AuthShell active="beneficiary">
      {result ? (
        <ReferenceSlip
          tone="ready"
          heading="Your account is created"
          reference={result.reference}
          action={<a className="auth-button" href="/login">Sign in</a>}
        >
          <p>Save this reference. It identifies your beneficiary profile.</p>
          <p>Creating your account did not sign you in. Sign in to send a request.</p>
        </ReferenceSlip>
      ) : (
        <form onSubmit={onSubmit} noValidate>
          <h2>Create your account</h2>
          <FormAlert>{formError}</FormAlert>
          <Field id="displayName" label="Your name" autoComplete="name"
            value={values.displayName} error={errors.displayName} onChange={v => setValue('displayName', v)} />
          <Field id="email" type="email" label="Email" autoComplete="email" required
            value={values.email} error={errors.email} onChange={v => setValue('email', v)} />
          <PasswordField id="password" label="Password" autoComplete="new-password"
            hint={`At least ${PASSWORD_MIN} characters.`}
            value={values.password} error={errors.password} onChange={v => setValue('password', v)} />
          <Field id="contactDetails" label="Contact details" optional multiline
            hint="A phone number or another way to reach you."
            value={values.contactDetails} error={errors.contactDetails} onChange={v => setValue('contactDetails', v)} />
          <CheckField id="isFamily" label="This account is for a family or household"
            checked={values.isFamily} onChange={v => setValue('isFamily', v)} />
          <SubmitButton busy={busy} label="Create account" busyLabel="Creating account…" />
        </form>
      )}
    </AuthShell>
  );
}
