const assert = require('node:assert/strict');
const React = require('react');
const { create } = require('react-test-renderer');
const { test } = require('node:test');
const { SessionProvider, useSession } = require('../../src/session/SessionProvider.jsx');
const { act, token, user } = require('./provider-fixture.cjs');

test('failed login does not discard an earlier pending refresh', async () => {
  let resolveUser;
  let session;
  globalThis.fetch = async (url) => {
    if (url.endsWith('/me')) return new Promise((resolve) => { resolveUser = resolve; });
    if (url.endsWith('/antiforgery')) return token();
    return new Response(null, { status: 401 });
  };
  function Observer() { session = useSession(); return null; }
  let view;
  await act(async () => { view = create(React.createElement(SessionProvider, null, React.createElement(Observer))); });
  assert.equal(session.state.status, 'loading');
  await act(async () => assert.rejects(session.login('example@example.test', 'bad')));
  await act(async () => { resolveUser(Response.json(user)); });
  assert.equal(session.state.status, 'authenticated');
  await act(async () => view.unmount());
});

test('an old unauthorized response cannot clear a newer login', async () => {
  let resolveOld;
  let session;
  let userReads = 0;
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/antiforgery')) return token();
    if (url.endsWith('/login')) return new Response(null, { status: 204 });
    if (url.endsWith('/me')) return Response.json({ ...user, displayName: ++userReads === 1 ? 'First' : 'Second' });
    assert.equal(new Headers(init.headers).get('X-Expected-User-Id'), user.id);
    return new Promise((resolve) => { resolveOld = resolve; });
  };
  function Observer() { session = useSession(); return null; }
  let view;
  await act(async () => { view = create(React.createElement(SessionProvider, null, React.createElement(Observer))); });
  let oldRequest;
  await act(async () => { oldRequest = session.request('/api/v1/catalog/categories'); oldRequest.catch(() => {}); });
  await act(async () => session.login('second@example.test', 'fictional'));
  await act(async () => { resolveOld(new Response(null, { status: 401 })); await assert.rejects(oldRequest); });
  assert.equal(session.state.status, 'authenticated');
  assert.equal(session.state.user.displayName, 'Second');
  await act(async () => view.unmount());
});
