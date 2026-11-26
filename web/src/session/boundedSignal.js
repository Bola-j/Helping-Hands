export const REQUEST_TIMEOUT_MS = 45_000;

export function boundedSignal(caller) {
  const controller = new AbortController();
  const abort = () => controller.abort();
  const timer = setTimeout(abort, REQUEST_TIMEOUT_MS);
  if (caller?.aborted) abort();
  else caller?.addEventListener('abort', abort, { once: true });
  return { signal: controller.signal, cleanup: () => { clearTimeout(timer); caller?.removeEventListener('abort', abort); } };
}