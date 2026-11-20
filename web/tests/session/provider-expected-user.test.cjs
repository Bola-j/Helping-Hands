const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('protected writes and logout carry the user ID captured by the provider', async () => {
  const seen = [];
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    seen.push(new Headers(init.headers).get('X-Expected-User-Id'));
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  await act(async () => view.session.request('/api/v1/catalog/categories', { method: 'POST' }));
  await act(async () => view.session.logout());
  assert.deepEqual(seen, [user.id, user.id]);
  await view.close();
});

test('a different browser session rejects an old user write and clears stale state', async () => {
  let mutations = 0;
  const otherId = '22222222-2222-4222-8222-222222222222';
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    if (new Headers(init.headers).get('X-Expected-User-Id') !== otherId) return new Response(null, { status: 409, headers: { 'X-Session-Changed': 'true' } });
    mutations += 1;
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories', { method: 'POST' })));
  assert.equal(mutations, 0);
  assert.equal(view.session.state.status, 'unavailable');
  await view.close();
});

test('anonymous users cannot initiate a protected write or logout', async () => {
  const calls = [];
  globalThis.fetch = async (url) => { calls.push(url); return new Response(null, { status: 401 }); };
  const view = await mount();
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories', { method: 'POST' })));
  await act(async () => assert.rejects(view.session.logout()));
  assert.deepEqual(calls, ['/api/v1/auth/me']);
  await view.close();
});
