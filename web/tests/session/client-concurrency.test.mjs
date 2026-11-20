import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { test } from 'node:test';
const { apiFetch } = createRequire(import.meta.url)('../../src/session/api.ts');

test('concurrent first writes serialize token and write pairs', async () => {
  const calls = [];
  let releaseFirst;
  globalThis.fetch = async (url) => {
    calls.push(url);
    if (url.endsWith('/antiforgery')) return Response.json({ requestToken: 'token', headerName: 'X-CSRF-TOKEN' });
    if (url.endsWith('/first')) return new Promise((resolve) => { releaseFirst = resolve; });
    return new Response(null, { status: 204 });
  };
  const first = apiFetch('/api/v1/first', { method: 'POST' });
  const second = apiFetch('/api/v1/second', { method: 'POST' });
  while (!releaseFirst) await new Promise((resolve) => setTimeout(resolve, 0));
  assert.deepEqual(calls, ['/api/v1/auth/antiforgery', '/api/v1/first']);
  releaseFirst(new Response(null, { status: 204 }));
  await Promise.all([first, second]);
  assert.deepEqual(calls, ['/api/v1/auth/antiforgery', '/api/v1/first', '/api/v1/auth/antiforgery', '/api/v1/second']);
});

test('a failed unsafe request releases the next queued action', async () => {
  globalThis.fetch = async (url) => {
    if (url.endsWith('/antiforgery')) return Response.json({ requestToken: 'token', headerName: 'X-CSRF-TOKEN' });
    return new Response(null, { status: url.endsWith('/first') ? 403 : 204 });
  };
  const first = apiFetch('/api/v1/first', { method: 'POST' });
  const second = apiFetch('/api/v1/second', { method: 'POST' });
  await assert.rejects(first, (error) => error.kind === 'forbidden');
  assert.equal((await second).status, 204);
});
