const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('forbidden request preserves session; expired request makes it anonymous', async () => {
  let status = 403;
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) return Response.json(user);
    assert.equal(new Headers(init.headers).get('X-Expected-User-Id'), user.id);
    return new Response(null, { status });
  };
  const view = await mount();
  assert.equal(view.session.state.status, 'authenticated');
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories')));
  assert.equal(view.session.state.status, 'authenticated');
  status = 401;
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories')));
  assert.equal(view.session.state.status, 'anonymous');
  await view.close();
});

test('failed logout does not falsely mark a user signed out', async () => {
  globalThis.fetch = async (url) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    throw new TypeError('Network failure');
  };
  const view = await mount();
  await act(async () => assert.rejects(view.session.logout()));
  assert.equal(view.session.state.status, 'authenticated');
  await view.close();
});

test('successful login refreshes the user; logout clears the session', async () => {
  let signedIn = false;
  globalThis.fetch = async (url) => {
    if (url.endsWith('/antiforgery')) return token();
    if (url.endsWith('/me')) return signedIn ? Response.json(user) : new Response(null, { status: 401 });
    signedIn = url.endsWith('/login');
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  assert.equal(view.session.state.status, 'anonymous');
  await act(async () => view.session.login('example@example.test', 'fictional'));
  assert.equal(view.session.state.status, 'authenticated');
  await act(async () => view.session.logout());
  assert.equal(view.session.state.status, 'anonymous');
  await view.close();
});
