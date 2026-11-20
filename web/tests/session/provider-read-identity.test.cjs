const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('protected reads carry the expected account ID', async () => {
  let expected;
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/me')) return Response.json(user);
    expected = new Headers(init.headers).get('X-Expected-User-Id');
    return Response.json({ items: [] });
  };
  const view = await mount();
  await act(async () => view.session.request('/api/v1/catalog/categories'));
  assert.equal(expected, user.id);
  await view.close();
});

test('a cross-account read returns no data and prevents subsequent writes', async () => {
  const calls = [];
  let finishRead;
  globalThis.fetch = async (url) => {
    calls.push(url);
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    if (url.endsWith('/categories')) return new Promise((resolve) => { finishRead = resolve; });
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  let read;
  await act(async () => { read = view.session.request('/api/v1/catalog/categories'); read.catch(() => {}); });
  await act(async () => {
    finishRead(new Response(null, { status: 409, headers: { 'X-Session-Changed': 'true' } }));
    await assert.rejects(read);
  });
  assert.equal(view.session.state.status, 'unavailable');
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories', { method: 'POST' })));
  assert.equal(calls.includes('/api/v1/auth/antiforgery'), false);
  await view.close();
});

test('a changed-session read cancels a write waiting for its antiforgery token', async () => {
  let finishToken;
  const calls = [];
  globalThis.fetch = async (url) => {
    calls.push(url);
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return new Promise((resolve) => { finishToken = resolve; });
    if (url.endsWith('/categories')) return new Response(null, { status: 409, headers: { 'X-Session-Changed': 'true' } });
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  let write;
  await act(async () => {
    write = view.session.request('/api/v1/example', { method: 'POST' }); write.catch(() => {});
    while (!finishToken) await new Promise((resolve) => setTimeout(resolve, 0));
  });
  await act(async () => assert.rejects(view.session.request('/api/v1/catalog/categories')));
  await act(async () => { finishToken(token()); await assert.rejects(write); });
  assert.equal(calls.includes('/api/v1/example'), false);
  await view.close();
});
