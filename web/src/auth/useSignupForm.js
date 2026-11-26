import { useCallback, useState } from 'react';
import { ApiError } from '../session/ApiError';
import { authErrorMessage } from '../session/authErrorMessage';
import { readServerFieldErrors } from './serverErrors';

const hasErrors = errors => Object.values(errors).some(Boolean);

function focusFirstInvalid(form) {
  requestAnimationFrame(() => form.querySelector('[aria-invalid="true"]')?.focus());
}

// Shared state for both signup screens. `initial`, `validate` and `submit`
// must be stable (module-level) references.
export function useSignupForm({ initial, validate, submit }) {
  const [values, setValues] = useState(initial);
  const [errors, setErrors] = useState({});
  const [formError, setFormError] = useState('');
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState(null);

  const setValue = useCallback((name, value) => {
    setValues(current => ({ ...current, [name]: value }));
    setErrors(current => (current[name] ? { ...current, [name]: '' } : current));
  }, []);

  const onSubmit = useCallback(async event => {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const found = validate(values);
    setErrors(found);
    setFormError('');
    if (hasErrors(found)) {
      setFormError('Check the highlighted fields and try again.');
      focusFirstInvalid(form);
      return;
    }
    setBusy(true);
    try {
      setResult(await submit(values));
      setValues(initial); // do not keep the password in memory after success
    } catch (error) {
      setFormError(authErrorMessage(error, 'signup'));
      if (error instanceof ApiError && error.kind === 'invalid') {
        setErrors(await readServerFieldErrors(error));
        focusFirstInvalid(form);
      }
    } finally {
      setBusy(false);
    }
  }, [busy, initial, submit, validate, values]);

  return { values, errors, formError, busy, result, setValue, onSubmit };
}