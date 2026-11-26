const assert = require('node:assert/strict');
const React = require('react');
const { create } = require('react-test-renderer');
const { test } = require('node:test');
const { ApiError } = require('../../src/session/ApiError.js');
const { useSignupForm } = require('../../src/auth/useSignupForm.js');
const { validEmail, validPassword, required } = require('../../src/auth/validation.js');

globalThis.IS_REACT_ACT_ENVIRONMENT = true;
globalThis.requestAnimationFrame ??= callback => callback();

const initial = { displayName: '', email: '', password: '' };
const validate = values => ({
  displayName: required(values.displayName, 'Enter your name.'),
  email: validEmail(values.email),
  password: validPassword(values.password),
});
const submitEvent = () => ({ preventDefault() {}, currentTarget: { querySelector: () => ({ focus() {} }) } });

async function mount(submit) {
  let form;
  function FormProbe() {
    form = useSignupForm({ initial, validate, submit });
    return null;
  }
  let view;
  await React.act(async () => { view = create(React.createElement(FormProbe)); });
  return { get form() { return form; }, view };
}

async function enterValidValues(form) {
  await React.act(async () => {
    form.setValue('displayName', 'Fictional person');
    form.setValue('email', 'person@example.test');
    form.setValue('password', 'FictionalPassword123!');
  });
}

test('duplicate signup shows a useful message and does not report success', async () => {
  let submitted;
  const mounted = await mount(async values => {
    submitted = values;
    throw new ApiError('conflict', 409, Response.json({ title: 'Conflict' }, { status: 409 }));
  });
  await enterValidValues(mounted.form);
  await React.act(async () => mounted.form.onSubmit(submitEvent()));

  assert.equal(submitted.email, 'person@example.test');
  assert.match(mounted.form.formError, /account with this email may already exist/i);
  assert.equal(mounted.form.result, null);
  await React.act(async () => mounted.view.unmount());
});

test('server weak-password validation is attached to the password field', async () => {
  const mounted = await mount(async () => {
    const response = Response.json({ errors: { Password: ['Password is too weak.'] } }, { status: 400 });
    throw new ApiError('invalid', 400, response);
  });
  await enterValidValues(mounted.form);
  await React.act(async () => mounted.form.onSubmit(submitEvent()));

  assert.equal(mounted.form.errors.password, 'Password is too weak.');
  assert.equal(mounted.form.formError, 'Check the fields below and try again.');
  assert.equal(mounted.form.result, null);
  await React.act(async () => mounted.view.unmount());
});
