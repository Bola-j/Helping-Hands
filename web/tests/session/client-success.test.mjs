import assert from 'node:assert/strict';
import { test } from 'node:test';
import { createRequire } from 'node:module';
const { apiFetch } = createRequire(import.meta.url)('../../src/session/api.ts');

test('safe requests use same-origin cookies without requesting antiforgery', async () => {
  const calls = [];
  globalThis.fetch = async (url, init) => {
    calls.push({ url, init });
    return new Response('{"ok":true}', { status: 200 });
  };
  const response = await apiFetch('/api/v1/auth/me');
  assert.equal(response.status, 200);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, '/api/v1/auth/me');
  assert.equal(calls[0].init.credentials, 'same-origin');
  assert.equal(new Headers(calls[0].init.headers).has('X-CSRF-TOKEN'), false);
});

test('each write obtains a fresh token and sends it with same-origin cookies', async () => {
  const calls = [];
  let token = 0;
  globalThis.fetch = async (url, init) => {
    calls.push({ url, init });
    if (url.endsWith('/antiforgery')) {
      token += 1;
      return Response.json({ requestToken: `token-${token}`, headerName: 'X-CSRF-TOKEN' });
    }
    return new Response(null, { status: 204 });
  };
  await apiFetch('/api/v1/auth/login', { method: 'POST', body: '{}' });
  await apiFetch('/api/v1/auth/logout', { method: 'POST' });
  assert.deepEqual(calls.map((call) => call.url), [
    '/api/v1/auth/antiforgery', '/api/v1/auth/login',
    '/api/v1/auth/antiforgery', '/api/v1/auth/logout',
  ]);
  assert.ok(calls.every((call) => call.init.credentials === 'same-origin'));
  assert.equal(new Headers(calls[1].init.headers).get('X-CSRF-TOKEN'), 'token-1');
  assert.equal(new Headers(calls[3].init.headers).get('X-CSRF-TOKEN'), 'token-2');
});
