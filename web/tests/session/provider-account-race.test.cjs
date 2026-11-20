const assert = require('node:assert/strict');
const { test } = require('node:test');
const { act, mount, token, user } = require('./provider-fixture.cjs');

test('a write queued before logout cannot run with a changed session', async () => {
  let finishLogout;
  const calls = [];
  globalThis.fetch = async (url) => {
    calls.push(url);
    if (url.endsWith('/me')) return Response.json(user);
    if (url.endsWith('/antiforgery')) return token();
    if (url.endsWith('/logout')) return new Promise((resolve) => { finishLogout = resolve; });
    return new Response(null, { status: 204 });
  };
  const view = await mount();
  let loggingOut, oldWrite;
  await act(async () => {
    loggingOut = view.session.logout();
    oldWrite = view.session.request('/api/v1/catalog/categories', { method: 'POST' });
    oldWrite.catch(() => {});
    while (!finishLogout) await new Promise((resolve) => setTimeout(resolve, 0));
  });
  await act(async () => {
    finishLogout(new Response(null, { status: 204 }));
    await loggingOut;
    await assert.rejects(oldWrite);
  });
  assert.equal(view.session.state.status, 'anonymous');
  assert.equal(calls.includes('/api/v1/catalog/categories'), false);
  await view.close();
});
