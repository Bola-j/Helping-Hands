const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('an expired cookie on a protected write signs out the stale session', async () => {
  let expected;
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    expected = new Headers(init.headers).get('X-Expected-User-Id');
    return new Response(null, { status: 401 });
  };
  const view = await mount();
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories', { method: 'POST' })));
  assert.equal(expected, user.id);
  assert.equal(view.session.state.status, 'anonymous');
  await view.close();
});
