// Client-side checks. The server stays the source of truth: if it rejects
// something these checks allowed, the form shows the server's message instead.
export const PASSWORD_MIN = 8;

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const required = (value, message) => (value.trim() ? '' : message);

export function validEmail(value) {
  if (!value.trim()) return 'Enter your email address.';
  return EMAIL.test(value.trim()) ? '' : 'Enter an email address like name@example.org.';
}

export function validPassword(value) {
  if (!value) return 'Create a password.';
  return value.length >= PASSWORD_MIN ? '' : `Use at least ${PASSWORD_MIN} characters.`;
}