const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('a stale logout 401 cannot clear a newer refreshed session', async () => {
  let finishLogout;
  let reads = 0;
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) {
      const first = ++reads === 1;
      return Response.json({ ...user, id: first ? user.id : '22222222-2222-4222-8222-222222222222', displayName: first ? 'First' : 'Second' });
    }
    if (url.endsWith('/antiforgery')) return token();
    assert.equal(new Headers(init.headers).get('X-Expected-User-Id'), user.id);
    return new Promise((resolve) => { finishLogout = resolve; });
  };
  const view = await mount();
  let logout;
  await act(async () => {
    logout = view.session.logout(); logout.catch(() => {});
    while (!finishLogout) await new Promise((resolve) => setTimeout(resolve, 0));
  });
  await act(async () => view.session.refresh());
  await act(async () => { finishLogout(new Response(null, { status: 401 })); await assert.rejects(logout); });
  assert.equal(view.session.state.status, 'authenticated');
  assert.equal(view.session.state.user.displayName, 'Second');
  await view.close();
});

test('a current logout 401 marks the session anonymous', async () => {
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    assert.equal(new Headers(init.headers).get('X-Expected-User-Id'), user.id);
    return new Response(null, { status: 401 });
  };
  const view = await mount();
  await act(async () => assert.rejects(view.session.logout()));
  assert.equal(view.session.state.status, 'anonymous');
  await view.close();
});
