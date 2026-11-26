const assert = require('node:assert/strict');
const React = require('react');
const { create } = require('react-test-renderer');
const { test } = require('node:test');
const { ApplicationGate } = require('../../src/applications/ApplicationGate.jsx');
const { ApplicationAttachments } = require('../../src/applications/ApplicationViews.jsx');
const { CharityApplicationWorkspace } = require('../../src/applications/CharityApplicationWorkspace.jsx');
const { CoordinatorApplicationsWorkspace } = require('../../src/applications/CoordinatorApplicationsWorkspace.jsx');
const { ApiError } = require('../../src/session/ApiError.js');

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

const pendingApplication = {
  id: 'recipient-1', version: 'version-1', reference: 'REC-00000001', displayName: 'Fictional charity',
  approvalStatus: 'Pending', active: false, contactDetails: 'Fictional contact',
  registrationInformation: null, attachments: [], decisions: [],
};

async function mount(element) {
  let view;
  await React.act(async () => { view = create(element); });
  return view;
}

function button(view, label) {
  return view.root.findAllByType('button').find(item => item.children.join('').trim() === label);
}

async function fill(view, id, value) {
  await React.act(async () => view.root.findByProps({ id }).props.onChange({ target: { value } }));
}

async function getRoleText(view, role) {
  return view.root.findByProps({ role }).children.join('');
}

function fakeFile({ type, size, name }) {
  return new File([new Uint8Array(size)], name, { type, lastModified: 0 });
}

function charityRequest(upload) {
  return async (path, options) => {
    if (path === '/api/v1/recipients') return Response.json([{ id: pendingApplication.id }]);
    if (path === `/api/v1/recipients/${pendingApplication.id}/attachments` && options?.method === 'POST') {
      return upload(options.body);
    }
    if (path === `/api/v1/recipients/${pendingApplication.id}`) return Response.json(pendingApplication);
    throw new Error(`Unexpected request: ${path}`);
  };
}

function coordinatorRequest(decide) {
  const application = structuredClone(pendingApplication);
  return async (path, options) => {
    if (path === '/api/v1/recipients') return Response.json([{ id: application.id, displayName: application.displayName, reference: application.reference, approvalStatus: application.approvalStatus }]);
    if (path === `/api/v1/recipients/${application.id}/decisions` && options?.method === 'POST') return decide(JSON.parse(options.body), application);
    if (path === `/api/v1/recipients/${application.id}`) return Response.json(application);
    throw new Error(`Unexpected request: ${path}`);
  };
}

async function submitUpload(view, file) {
  const form = view.root.findByType('form');
  let reset = false;
  await React.act(async () => form.props.onSubmit({
    preventDefault() {},
    currentTarget: { elements: { document: { files: [file] } }, reset() { reset = true; } },
  }));
  return reset;
}

test('representative uploads an allowed document as multipart and remains pending', async () => {
  let uploaded;
  const request = charityRequest(async body => {
    uploaded = body;
    return new Response(null, { status: 201 });
  });
  const view = await mount(React.createElement(CharityApplicationWorkspace, { request, user: { displayName: 'Fictional representative' } }));
  const reset = await submitUpload(view, fakeFile({ type: 'application/pdf', size: 256, name: 'fictional.pdf' }));

  assert.equal(uploaded instanceof FormData, true);
  assert.equal(uploaded.get('file').name, 'fictional.pdf');
  assert.equal(reset, true);
  assert.match(await getRoleText(view, 'status'), /Document uploaded.*still pending review/);
  assert.match(view.root.findByProps({ className: 'application-status status-pending' }).children.join(''), /Pending/);
  await React.act(async () => view.unmount());
});

test('representative blocks unsupported or oversized files before sending them', async () => {
  let uploads = 0;
  const request = charityRequest(async () => { uploads += 1; return new Response(null, { status: 201 }); });
  const view = await mount(React.createElement(CharityApplicationWorkspace, { request, user: { displayName: 'Fictional representative' } }));

  await submitUpload(view, fakeFile({ type: 'text/plain', size: 20, name: 'notes.txt' }));
  assert.match(await getRoleText(view, 'alert'), /PDF, JPEG or PNG.*10 MiB/);
  await submitUpload(view, fakeFile({ type: 'application/pdf', size: 10 * 1024 * 1024 + 1, name: 'large.pdf' }));
  assert.match(await getRoleText(view, 'alert'), /PDF, JPEG or PNG.*10 MiB/);
  assert.equal(uploads, 0);
  await React.act(async () => view.unmount());
});

test('coordinator requires a rejection reason and records the reason with the current version', async () => {
  let submitted;
  const request = coordinatorRequest(async (body, application) => {
    submitted = body;
    application.approvalStatus = body.decision;
    application.decisions = [{ decision: body.decision, reason: body.reason, decidedAt: '2026-01-01T00:00:00Z' }];
    return new Response(null, { status: 204 });
  });
  const view = await mount(React.createElement(CoordinatorApplicationsWorkspace, { request, user: { displayName: 'Fictional coordinator' } }));

  assert.equal(button(view, 'Reject').props.disabled, true);
  await fill(view, 'decision-reason', '  ');
  assert.equal(button(view, 'Reject').props.disabled, true);
  await fill(view, 'decision-reason', 'Fictional missing registration evidence');
  assert.equal(button(view, 'Reject').props.disabled, false);
  await React.act(async () => button(view, 'Reject').props.onClick());

  assert.deepEqual(submitted, { decision: 'Rejected', reason: 'Fictional missing registration evidence', version: 'version-1' });
  assert.match(view.root.findByProps({ className: 'application-status status-rejected' }).children.join(''), /Rejected/);
  assert.match(JSON.stringify(view.toJSON()), /Fictional missing registration evidence/);
  await React.act(async () => view.unmount());
});

test('coordinator approval sends the current version and shows approved status', async () => {
  let submitted;
  const request = coordinatorRequest(async (body, application) => {
    submitted = body;
    application.approvalStatus = body.decision;
    application.active = true;
    return new Response(null, { status: 204 });
  });
  const view = await mount(React.createElement(CoordinatorApplicationsWorkspace, { request, user: { displayName: 'Fictional coordinator' } }));
  await React.act(async () => button(view, 'Approve').props.onClick());

  assert.deepEqual(submitted, { decision: 'Approved', reason: null, version: 'version-1' });
  assert.match(view.root.findByProps({ className: 'application-status status-approved' }).children.join(''), /Approved/);
  await React.act(async () => view.unmount());
});

test('stale coordinator decision shows the API conflict and leaves application pending', async () => {
  const request = coordinatorRequest(async () => {
    throw new ApiError('conflict', 409, Response.json({ detail: 'This application changed. Refresh before deciding.' }, { status: 409 }));
  });
  const view = await mount(React.createElement(CoordinatorApplicationsWorkspace, { request, user: { displayName: 'Fictional coordinator' } }));
  await React.act(async () => button(view, 'Approve').props.onClick());

  assert.match(await getRoleText(view, 'alert'), /application changed.*Refresh before deciding/);
  assert.match(view.root.findByProps({ className: 'application-status status-pending' }).children.join(''), /Pending/);
  await React.act(async () => view.unmount());
});

test('application attachments link only to the authorized content route', async () => {
  const view = await mount(React.createElement(ApplicationAttachments, { application: {
    attachments: [{ id: 'attachment-1', originalName: 'fictional.pdf', sizeBytes: 1024, storageKey: 'private-secret-key' }],
  } }));

  const link = view.root.findByType('a');
  assert.equal(link.props.href, '/api/v1/attachments/attachment-1/content');
  assert.match(link.children.join(''), /fictional.pdf/);
  assert.doesNotMatch(JSON.stringify(view.toJSON()), /private-secret-key/);
  await React.act(async () => view.unmount());
});

test('role gate denies other signed-in roles and shows unavailable service state', async () => {
  const denied = await mount(React.createElement(ApplicationGate, {
    state: { status: 'authenticated', user: { roles: ['Beneficiary'] } },
    allowedRoles: ['Coordinator', 'Administrator'], prompt: 'Sign in with a coordinator account.', children: React.createElement('p', null, 'Private application list'),
  }));
  assert.match(denied.root.findByType('h1').children.join(''), /Access not available/);
  await React.act(async () => denied.unmount());

  const unavailable = await mount(React.createElement(ApplicationGate, {
    state: { status: 'unavailable' }, allowedRoles: ['Coordinator'], prompt: 'Sign in.', children: null,
  }));
  assert.match(await getRoleText(unavailable, 'alert'), /session service is unavailable/);
  await React.act(async () => unavailable.unmount());
});
