const assert = require('node:assert/strict');
const React = require('react');
const { create } = require('react-test-renderer');
const { test } = require('node:test');
const { CharityReferralWorkspace } = require('../../src/charity-referrals/CharityReferralWorkspace.jsx');
const { CharityReferralPage } = require('../../src/charity-referrals/CharityReferralPage.jsx');
const { SessionContext } = require('../../src/session/SessionContext.js');
const { ApiError } = require('../../src/session/ApiError.js');

globalThis.IS_REACT_ACT_ENVIRONMENT = true;
const referral = { id: 'referral-1', requestId: 'request-1', reference: 'REF-00000001', assignedQuantity: 4, scopeDescription: 'Fictional food parcel', status: 'Assigned' };
const representative = { displayName: 'Fictional charity representative' };

test('representative saves a report, uploads private evidence, then submits it', async () => {
  const calls = [];
  const request = async (path, init = {}) => {
    calls.push({ path, init });
    if (path === '/api/v1/referrals') return Response.json([referral]);
    if (path === '/api/v1/outcomes') return Response.json([]);
    if (path === `/api/v1/referrals/${referral.id}/outcomes`) return Response.json({ id: 'outcome-1', referralId: referral.id, reference: 'OUT-00000001', reportedQuantity: 2, description: 'Fictional delivery report', status: 'Draft', version: 'version-1' });
    if (path === '/api/v1/outcomes/outcome-1/attachments') return Response.json({ id: 'attachment-1', originalName: 'fictional.pdf', mediaType: 'application/pdf', sizeBytes: 128 });
    if (path === '/api/v1/outcomes/outcome-1/submit') return Response.json({ id: 'outcome-1', referralId: referral.id, reference: 'OUT-00000001', reportedQuantity: 2, description: 'Fictional delivery report', status: 'Submitted', version: 'version-2' });
    throw new Error(`Unexpected request: ${path}`);
  };
  const view = await mount(request);
  assert.match(rendered(view), /REF-00000001/);
  await fill(view, 'quantity', '2'); await fill(view, 'description', 'Fictional delivery report');
  await submit(view.root.findAllByType('form')[0]);
  assert.match(rendered(view), /saved as a draft/);
  await submit(view.root.findAllByType('form')[1]);
  assert.match(rendered(view), /Private evidence uploaded/);
  const submitButton = view.root.findAllByType('button').find(item => item.children.join('') === 'Submit report to coordinator');
  await React.act(async () => submitButton.props.onClick());
  assert.match(rendered(view), /submitted to the coordinator for review/);
  const createCall = calls.find(item => item.path.endsWith('/outcomes') && item.init.method === 'POST');
  assert.deepEqual(JSON.parse(createCall.init.body), { reportedQuantity: 2, description: 'Fictional delivery report' });
  const upload = calls.find(item => item.path.endsWith('/attachments'));
  assert.equal(upload.init.body.get('file').name, 'fictional.pdf');
  assert.equal(calls.some(item => item.path.endsWith('/submit') && item.init.method === 'POST'), true);
  await React.act(async () => view.unmount());
});

test('list failures show an alert instead of assigned referral details', async () => {
  const denied = await mount(async path => {
    if (path === '/api/v1/referrals') throw new ApiError('response', 403, Response.json({}, { status: 403 }));
    if (path === '/api/v1/outcomes') return Response.json([]);
    throw new Error(`Unexpected request: ${path}`);
  });
  assert.match(denied.root.findByProps({ role: 'alert' }).children.join(''), /Active charity representative access is required/);
  assert.doesNotMatch(rendered(denied), /Fictional food parcel/);
  await React.act(async () => denied.unmount());
});

test('clarification shows the coordinator reason and reopens the draft for editing', async () => {
  const outcome = { id: 'outcome-2', referralId: referral.id, reference: 'OUT-00000002', reportedQuantity: 1, description: 'Fictional report', status: 'Clarification', version: 'version-3' };
  const view = await mount(async path => {
    if (path === '/api/v1/referrals') return Response.json([referral]);
    if (path === '/api/v1/outcomes') return Response.json([outcome]);
    if (path === '/api/v1/requests/request-1/progress') return Response.json({ history: [{ entityId: outcome.id, action: 'Clarification', reason: 'Add a fictional delivery date.' }] });
    throw new Error(`Unexpected request: ${path}`);
  });
  await React.act(async () => new Promise(resolve => setImmediate(resolve)));
  assert.match(rendered(view), /Add a fictional delivery date/);
  assert.match(rendered(view), /Respond to clarification/);
  await React.act(async () => view.unmount());
});

test('unresolved reports stay visible and prevent a duplicate submission', async () => {
  const outcome = { id: 'outcome-3', referralId: referral.id, reference: 'OUT-00000003', reportedQuantity: 1, description: 'Fictional unresolved report', status: 'Unresolved', version: 'version-4' };
  const view = await mount(async path => {
    if (path === '/api/v1/referrals') return Response.json([referral]);
    if (path === '/api/v1/outcomes') return Response.json([outcome]);
    throw new Error(`Unexpected request: ${path}`);
  });
  assert.match(rendered(view), /This report is unresolved/);
  assert.match(rendered(view), /Contact the coordinator before creating another report/);
  assert.doesNotMatch(rendered(view), /Report delivery/);
  await React.act(async () => view.unmount());
});

test('non-charity accounts see the role gate without loading referral data', async () => {
  let view;
  const session = { state: { status: 'authenticated', user: { displayName: 'Fictional beneficiary', roles: ['Beneficiary'] } }, request: async () => { throw new Error('Private referral data must not load.'); } };
  await React.act(async () => { view = create(React.createElement(SessionContext.Provider, { value: session }, React.createElement(CharityReferralPage))); });
  assert.match(rendered(view), /Charity access required/);
  assert.doesNotMatch(rendered(view), /Assigned referrals/);
  await React.act(async () => view.unmount());
});

async function mount(request) {
  let view;
  await React.act(async () => { view = create(React.createElement(CharityReferralWorkspace, { request, user: representative })); await new Promise(resolve => setImmediate(resolve)); });
  return view;
}

async function fill(view, name, value) {
  const input = view.root.findAll(node => node.props.name === name)[0];
  await React.act(async () => input.props.onChange({ target: { value } }));
}

async function submit(form) {
  const file = new File(['%PDF fictional'], 'fictional.pdf', { type: 'application/pdf' });
  const event = { preventDefault() {}, currentTarget: { elements: { evidence: { files: [file] } }, reset() {} } };
  await React.act(async () => form.props.onSubmit(event));
}

function rendered(view) {
  const flatten = node => typeof node === 'string' ? node : Array.isArray(node) ? node.map(flatten).join(' ') : node?.children ? flatten(node.children) : '';
  return flatten(view.toJSON());
}
