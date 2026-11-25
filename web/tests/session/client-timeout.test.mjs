import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { test } from 'node:test';
const require = createRequire(import.meta.url);
const { apiFetch } = require('../../src/session/api.js');
const { REQUEST_TIMEOUT_MS } = require('../../src/session/boundedSignal.js');

test('a timed-out request releases the next queued action', async (context) => {
  context.mock.timers.enable({ apis: ['setTimeout'] });
  let first = true;
  let notifyStarted;
  const started = new Promise((resolve) => { notifyStarted = resolve; });
  globalThis.fetch = async (url, init) => {
    if (url.endsWith('/antiforgery') && first) {
      first = false;
      notifyStarted();
      return new Promise((resolve, reject) => init.signal.addEventListener('abort', () => reject(new DOMException('Aborted', 'AbortError'))));
    }
    if (url.endsWith('/antiforgery')) return Response.json({ requestToken: 'token', headerName: 'X-CSRF-TOKEN' });
    return new Response(null, { status: 204 });
  };
  const timedOut = apiFetch('/api/v1/first', { method: 'POST' });
  timedOut.catch(() => {});
  const next = apiFetch('/api/v1/second', { method: 'POST' });
  await started;
  context.mock.timers.tick(REQUEST_TIMEOUT_MS);
  await assert.rejects(timedOut);
  assert.equal((await next).status, 204);
});
