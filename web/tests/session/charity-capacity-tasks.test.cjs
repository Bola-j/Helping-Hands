const assert = require('node:assert/strict');
const React = require('react');
const { create } = require('react-test-renderer');
const { test } = require('node:test');
const { CharityServicesWorkspace } = require('../../src/recipients/CharityServicesWorkspace.jsx');
const { ApiError } = require('../../src/session/ApiError.js');

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

const recipientId = 'fictional-recipient';
const serviceId = 'fictional-service-version';
const standardId = 'fictional-standard-version';
const capacityId = 'fictional-charity-service';
const user = { displayName: 'Fictional Charity Representative' };

test('capacity task refreshes after a zero-unit update and shows the saved zero state', async () => {
  const calls = [];
  let updated = false;
  const request = async (path, init = {}) => {
    calls.push({ path, init });
    if (path === '/api/v1/recipients') return json([{ id: recipientId, reference: 'REC-1', displayName: 'Fictional Charity' }]);
    if (path.endsWith('/services')) return json({ selectedServices: [{ serviceVersionId: serviceId, standardVersionId: standardId }], applicableOptions: [] });
    if (path.endsWith('/capacity') && init.method === 'PUT') { updated = true; return json({ capacities: [] }); }
    if (path.endsWith('/capacity')) return json({ capacities: [capacity(updated ? 'Zero' : 'Stale', updated ? 0 : null)] });
    throw new Error(`Unexpected request ${path}`);
  };
  const view = await mountWorkspace(request);
  assert.match(renderedText(view), /Needs an update for Cairo date\s+2026-11-27/);

  const quantity = view.root.findAllByType('input').find(input => input.props.type === 'number');
  await React.act(async () => quantity.props.onChange({ target: { value: '0' } }));
  const capacityForm = view.root.findAllByType('form')[1];
  await React.act(async () => capacityForm.props.onSubmit({ preventDefault() {} }));

  const put = calls.find(call => call.path.endsWith('/capacity') && call.init.method === 'PUT');
  assert.equal(JSON.parse(put.init.body).capacities[0].available, 0);
  assert.ok(calls.filter(call => call.path.endsWith('/capacity') && !call.init.method).length >= 2);
  assert.doesNotMatch(renderedText(view), /Needs an update for Cairo date/);
  assert.match(renderedText(view), /Zero available/);
  assert.match(renderedText(view), /All selected services have today’s capacity update/);
  await React.act(async () => view.unmount());
});

test('capacity task page exposes an alert when capacity cannot be loaded', async () => {
  const request = async path => {
    if (path === '/api/v1/recipients') return json([{ id: recipientId, reference: 'REC-1', displayName: 'Fictional Charity' }]);
    if (path.endsWith('/services')) return json({ selectedServices: [], applicableOptions: [] });
    if (path.endsWith('/capacity')) throw new ApiError('network', null);
    throw new Error(`Unexpected request ${path}`);
  };
  const view = await mountWorkspace(request);
  assert.match(renderedText(view), /could not load service and capacity details/i);
  assert.equal(view.root.findAll(node => node.props.role === 'alert').length, 1);
  await React.act(async () => view.unmount());
});

async function mountWorkspace(request) {
  let view;
  await React.act(async () => {
    view = create(React.createElement(CharityServicesWorkspace, { request, user }));
    await new Promise(resolve => setImmediate(resolve));
  });
  return view;
}

function renderedText(view) {
  return textFromNode(view.toJSON());
}

function textFromNode(node) {
  if (typeof node === 'string') return node;
  if (Array.isArray(node)) return node.map(textFromNode).join(' ');
  return node?.children ? textFromNode(node.children) : '';
}

function json(value) { return { json: async () => value }; }

function capacity(status, available) {
  return { charityServiceId: capacityId, serviceName: 'Fictional food', unitStandardName: 'Parcel', serviceVersionNumber: 1, standardVersionNumber: 1, cairoDate: '2026-11-27', status, isStale: status === 'Stale', available, updatedAt: status === 'Stale' ? null : '2026-11-27T09:00:00Z', lastUpdatedCairoDate: status === 'Stale' ? '2026-11-26' : '2026-11-27', lastKnownAvailable: status === 'Stale' ? 2 : 0 };
}
