import assert from 'node:assert/strict';
import { test } from 'node:test';
import { createRequire } from 'node:module';
const { apiFetch } = createRequire(import.meta.url)('../../src/session/api.ts');

test('401, 403 and unavailable API responses remain distinguishable', async () => {
  for (const [status, kind] of [[401, 'unauthorized'], [403, 'forbidden'], [503, 'unavailable']]) {
    globalThis.fetch = async () => Response.json({ title: 'Request failed' }, { status });
    await assert.rejects(apiFetch('/api/v1/auth/me'), (error) => error.status === status && error.kind === kind);
  }
});

test('network failure is reported and an unsafe request is never retried', async () => {
  let calls = 0;
  globalThis.fetch = async (url) => {
    calls += 1;
    if (url.endsWith('/antiforgery')) {
      return Response.json({ requestToken: 'token', headerName: 'X-CSRF-TOKEN' });
    }
    throw new TypeError('Network error');
  };
  await assert.rejects(apiFetch('/api/v1/auth/logout', { method: 'POST' }), (error) => error.kind === 'network');
  assert.equal(calls, 2);
});

test('failed antiforgery acquisition prevents the unsafe request', async () => {
  let calls = 0;
  globalThis.fetch = async () => { calls += 1; return new Response(null, { status: 503 }); };
  await assert.rejects(apiFetch('/api/v1/auth/logout', { method: 'POST' }));
  assert.equal(calls, 1);
});
