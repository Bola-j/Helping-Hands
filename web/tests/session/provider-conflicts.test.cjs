const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('a stale logout clears session state and is never retried', async () => {
  let calls = 0;
  globalThis.fetch = async (url) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    calls += 1;
    return new Response(null, { status: 409, headers: { 'X-Session-Changed': 'true' } });
  };
  const view = await mount();
  await act(async () => assert.rejects(view.session.logout()));
  assert.equal(view.session.state.status, 'unavailable');
  assert.equal(calls, 1);
  await view.close();
});

test('an ordinary domain conflict preserves the authenticated session', async () => {
  globalThis.fetch = async (url) => {
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    return new Response(null, { status: 409 });
  };
  const view = await mount();
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories', { method: 'POST' })));
  assert.equal(view.session.state.status, 'authenticated');
  await view.close();
});

test('a session mismatch rejects already queued writes before they dispatch', async () => {
  let finishFirst;
  const calls = [];
  globalThis.fetch = async (url) => {
    calls.push(url);
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    if (url.endsWith('/first')) return new Promise((resolve) => { finishFirst = resolve; });
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  let first, second;
  await act(async () => {
    first = view.session.request('/api/v1/first', { method: 'POST' });
    second = view.session.request('/api/v1/second', { method: 'POST' });
    first.catch(() => {}); second.catch(() => {});
    while (!finishFirst) await new Promise((resolve) => setTimeout(resolve, 0));
  });
  await act(async () => {
    finishFirst(new Response(null, { status: 409, headers: { 'X-Session-Changed': 'true' } }));
    await assert.rejects(first); await assert.rejects(second);
  });
  assert.equal(calls.includes('/api/v1/second'), false);
  assert.equal(view.session.state.status, 'unavailable');
  await view.close();
});
