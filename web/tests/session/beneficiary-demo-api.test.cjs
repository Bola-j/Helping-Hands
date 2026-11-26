const test = require('node:test');
const assert = require('node:assert/strict');
const { createDemoRequest } = require('../../src/session/createDemoRequest.js');
const { readDemoBeneficiary } = require('../../src/session/readDemoBeneficiary.js');
const { readDemoRequests } = require('../../src/session/readDemoRequests.js');

test('single-need helper sends both required confirmations and returns generated reference and status', async () => {
  let sent;
  const request = async (path, init) => {
    sent = { path, init };
    return { json: async () => ({ id: 'request-id', reference: 'REQ-0001', status: 'Submitted' }) };
  };
  const result = await createDemoRequest(request, 'beneficiary-id', 'A fictional need description');
  assert.equal(sent.path, '/api/v1/requests/single-need');
  assert.deepEqual(JSON.parse(sent.init.body), { beneficiaryId: 'beneficiary-id', description: 'A fictional need description', authorityConfirmed: true, sharingAgreed: true });
  assert.equal(result.reference, 'REQ-0001');
  assert.equal(result.status, 'Submitted');
});

test('beneficiary profile lookup requires exactly one matching visible profile', async () => {
  const request = async () => ({ json: async () => [{ id: 'own-id', reference: 'BEN-1', displayName: 'Demo User', isFamily: false }] });
  assert.equal((await readDemoBeneficiary(request, 'Demo User')).id, 'own-id');
  await assert.rejects(readDemoBeneficiary(async () => ({ json: async () => [
    { id: 'one', reference: 'BEN-1', displayName: 'Demo User', isFamily: false },
    { id: 'two', reference: 'BEN-2', displayName: 'Demo User', isFamily: false },
  ] }), 'Demo User'));
});

test('request history rejects an invalid API response shape', async () => {
  await assert.rejects(readDemoRequests(async () => ({ json: async () => ({ items: [] }) })), /Invalid request list response/);
});
