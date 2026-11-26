import { useState } from 'react';

function describedBy(id, hint, error) {
  return [hint ? `${id}-hint` : '', error ? `${id}-error` : ''].filter(Boolean).join(' ') || undefined;
}

export function Field({ id, label, hint, error, optional, multiline, type = 'text', value, onChange, ...rest }) {
  const Control = multiline ? 'textarea' : 'input';
  return (
    <div className="auth-field">
      <label htmlFor={id}>
        {label}
        {optional && <span className="auth-optional"> (optional)</span>}
      </label>
      {hint && <p className="auth-hint" id={`${id}-hint`}>{hint}</p>}
      <Control
        id={id}
        name={id}
        type={multiline ? undefined : type}
        value={value}
        rows={multiline ? 3 : undefined}
        onChange={event => onChange(event.target.value)}
        aria-invalid={error ? 'true' : undefined}
        aria-describedby={describedBy(id, hint, error)}
        {...rest}
      />
      {error && <p className="auth-error" id={`${id}-error`}>{error}</p>}
    </div>
  );
}

export function PasswordField({ id, label, hint, error, value, onChange, autoComplete }) {
  const [shown, setShown] = useState(false);
  return (
    <div className="auth-field">
      <label htmlFor={id}>{label}</label>
      {hint && <p className="auth-hint" id={`${id}-hint`}>{hint}</p>}
      <div className="auth-password">
        <input
          id={id}
          name={id}
          type={shown ? 'text' : 'password'}
          value={value}
          autoComplete={autoComplete}
          onChange={event => onChange(event.target.value)}
          aria-invalid={error ? 'true' : undefined}
          aria-describedby={describedBy(id, hint, error)}
        />
        <button
          type="button"
          className="auth-reveal"
          onClick={() => setShown(current => !current)}
          aria-label={shown ? 'Hide password' : 'Show password'}
        >
          {shown ? 'Hide' : 'Show'}
        </button>
      </div>
      {error && <p className="auth-error" id={`${id}-error`}>{error}</p>}
    </div>
  );
}

export function CheckField({ id, label, hint, checked, onChange }) {
  return (
    <div className="auth-check">
      <input
        id={id}
        name={id}
        type="checkbox"
        checked={checked}
        onChange={event => onChange(event.target.checked)}
        aria-describedby={hint ? `${id}-hint` : undefined}
      />
      <label htmlFor={id}>
        {label}
        {hint && <span className="auth-hint" id={`${id}-hint`}>{hint}</span>}
      </label>
    </div>
  );
}

export function FormAlert({ children }) {
  if (!children) return null;
  return <div className="auth-alert" role="alert">{children}</div>;
}

export function SubmitButton({ busy, label, busyLabel }) {
  return (
    <button className="auth-button" type="submit" disabled={busy}>
      {busy ? busyLabel : label}
    </button>
  );
}