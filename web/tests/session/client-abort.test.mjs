import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { test } from 'node:test';
const { apiFetch } = createRequire(import.meta.url)('../../src/session/api.ts');

test('an aborted queued action does not fetch a token or send a write', async () => {
  const calls = [];
  const abort = new AbortController();
  let finishFirst;
  globalThis.fetch = async (url) => {
    calls.push(url);
    if (url.endsWith('/antiforgery')) return Response.json({ requestToken: 'token', headerName: 'X-CSRF-TOKEN' });
    if (url.endsWith('/first')) return new Promise((resolve) => { finishFirst = resolve; });
    return new Response(null, { status: 204 });
  };
  const first = apiFetch('/api/v1/first', { method: 'POST' });
  const second = apiFetch('/api/v1/second', { method: 'POST', signal: abort.signal });
  second.catch(() => {});
  while (!finishFirst) await new Promise((resolve) => setTimeout(resolve, 0));
  abort.abort();
  finishFirst(new Response(null, { status: 204 }));
  await first;
  await assert.rejects(second);
  assert.deepEqual(calls, ['/api/v1/auth/antiforgery', '/api/v1/first']);
});

test('aborting a stalled token request releases the next queued action', async () => {
  const abort = new AbortController();
  let tokenStarted;
  const started = new Promise((resolve) => { tokenStarted = resolve; });
  let first = true;
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/antiforgery') && first) {
      first = false;
      tokenStarted();
      return new Promise((resolve, reject) => init.signal?.addEventListener('abort', () => reject(new DOMException('Aborted', 'AbortError'))));
    }
    if (url.endsWith('/antiforgery')) return Response.json({ requestToken: 'token', headerName: 'X-CSRF-TOKEN' });
    return new Response(null, { status: 204 });
  };
  const stalled = apiFetch('/api/v1/first', { method: 'POST', signal: abort.signal });
  stalled.catch(() => {});
  const next = apiFetch('/api/v1/second', { method: 'POST' });
  await started;
  abort.abort();
  await assert.rejects(stalled);
  assert.equal((await next).status, 204);
});
