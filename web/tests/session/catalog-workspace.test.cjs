const assert = require('node:assert/strict');
const React = require('react');
const { create } = require('react-test-renderer');
const { test } = require('node:test');
const { CatalogCategoryPanel } = require('../../src/coordinator/CatalogCategoryPanel.jsx');
const { CatalogServicePanel } = require('../../src/coordinator/CatalogServicePanel.jsx');
const { CatalogStandardsPanel } = require('../../src/coordinator/CatalogStandardsPanel.jsx');
const { CatalogComponentsPanel } = require('../../src/coordinator/CatalogComponentsPanel.jsx');
const { CatalogHistoryPanel } = require('../../src/coordinator/CatalogHistoryPanel.jsx');
const { CatalogVersionPanel } = require('../../src/coordinator/CatalogVersionPanel.jsx');
const { ApiError } = require('../../src/session/ApiError.js');

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

function json(data, status = 200, headers = {}) {
  return Response.json(data, { status, headers });
}

async function mount(element) {
  let view;
  await React.act(async () => { view = create(element); });
  return view;
}

function button(view, label) {
  return view.root.findAllByType('button').find(item => item.children.join('') === label);
}

async function fill(view, id, value) {
  await React.act(async () => view.root.findByProps({ id }).props.onChange({ target: { value } }));
}

async function submit(view) {
  await React.act(async () => view.root.findByType('form').props.onSubmit({ preventDefault() {} }));
}

test('category creation posts a trimmed code and reports success', async () => {
  const calls = [];
  let listReads = 0;
  const request = async (path, options) => {
    calls.push({ path, options });
    if (options?.method === 'POST') return json({ id: 'category-1' }, 201);
    return json({ items: ++listReads === 1 ? [] : [{ id: 'category-1', code: 'FOOD' }] });
  };
  const view = await mount(React.createElement(CatalogCategoryPanel, { request, selected: null, onSelect() {} }));
  await fill(view, 'category-code', ' FOOD ');
  await submit(view);

  assert.deepEqual(calls.find(call => call.options?.method === 'POST'), {
    path: '/api/v1/catalog/categories',
    options: { method: 'POST', headers: new Headers({ Accept: 'application/json', 'Content-Type': 'application/json' }), body: JSON.stringify({ code: 'FOOD' }) },
  });
  assert.match(view.root.findByProps({ role: 'status' }).children.join(''), /Category saved/);
  assert.match(view.root.findByType('strong').children.join(''), /FOOD/);
  await React.act(async () => view.unmount());
});

test('service publishing uses the current ETag and reloads published status', async () => {
  const calls = [];
  let listReads = 0;
  const request = async (path, options) => {
    calls.push({ path, options });
    if (path.endsWith('/service-versions/service-1') && options?.method === 'GET') {
      return json({ id: 'service-1' }, 200, { ETag: '"service-v1"' });
    }
    if (path.endsWith('/publish')) return new Response(null, { status: 204 });
    return json({ items: [{ id: 'service-1', name: 'Food', versionNumber: 1, isPublished: ++listReads > 1 }] });
  };
  const view = await mount(React.createElement(CatalogServicePanel, {
    request, category: { id: 'category-1', code: 'FOOD' }, selected: null, onSelect() {},
  }));
  await React.act(async () => button(view, 'Publish').props.onClick());

  const publish = calls.find(call => call.path.endsWith('/publish'));
  assert.equal(publish.options.method, 'POST');
  assert.equal(publish.options.headers.get('If-Match'), '"service-v1"');
  assert.match(view.root.findByProps({ role: 'status' }).children.join(''), /published/i);
  await React.act(async () => view.unmount());
});

test('unit standard creation sends the selected service and trimmed name', async () => {
  const calls = [];
  const request = async (path, options) => {
    calls.push({ path, options });
    if (options?.method === 'POST') return json({ id: 'standard-1' }, 201);
    return json({ items: [] });
  };
  const view = await mount(React.createElement(CatalogStandardsPanel, {
    request, service: { id: 'service-1', name: 'Food', versionNumber: 1, isPublished: false }, selected: null, onSelect() {},
  }));
  await fill(view, 'standard-name', ' Household parcel ');
  await submit(view);

  const post = calls.find(call => call.options?.method === 'POST');
  assert.equal(post.path, '/api/v1/catalog/service-versions/service-1/standards');
  assert.equal(post.options.body, JSON.stringify({ name: 'Household parcel' }));
  assert.match(view.root.findByProps({ role: 'status' }).children.join(''), /Unit standard saved/);
  await React.act(async () => view.unmount());
});

test('component creation sends a numeric quantity and unit', async () => {
  let posted;
  const request = async (path, options) => {
    if (options?.method === 'POST') posted = { path, body: JSON.parse(options.body) };
    return json({ id: 'component-1' }, options?.method === 'POST' ? 201 : 200);
  };
  let changed = false;
  const view = await mount(React.createElement(CatalogComponentsPanel, {
    request, version: { id: 'standard-version-1', isPublished: false, components: [] },
    onChanged: async () => { changed = true; }, onError() {},
  }));
  await fill(view, 'component-name', 'Rice');
  await fill(view, 'component-quantity', '1.25');
  await fill(view, 'component-unit', 'kg');
  await submit(view);

  assert.deepEqual(posted, {
    path: '/api/v1/catalog/standard-versions/standard-version-1/components',
    body: { name: 'Rice', quantity: 1.25, unit: 'kg' },
  });
  assert.equal(changed, true);
  await React.act(async () => view.unmount());
});

test('published standard versions show component quantities and disable edits', async () => {
  const request = async () => json({
    id: 'version-1', versionNumber: 1, effectiveAt: '2026-01-01T00:00:00Z', isPublished: true,
    components: [{ id: 'component-1', name: 'Rice', quantity: 1.25, unit: 'kg' }],
  });
  const view = await mount(React.createElement(CatalogVersionPanel, { request, versionId: 'version-1' }));

  assert.match(view.root.findAllByType('small')[0].children.join(''), /1\.25 kg/);
  assert.equal(view.root.findByProps({ id: 'version-effective' }).props.disabled, true);
  assert.equal(view.root.findAllByType('button').some(item => /Add component|Edit|Remove/.test(item.children.join(''))), false);
  assert.equal(view.root.findAllByType('button').some(item => item.children.join('') === 'Publish version'), false);
  await React.act(async () => view.unmount());
});

test('standard history lists prior versions and creates a new draft version', async () => {
  const calls = [];
  let reads = 0;
  const nextVersion = { id: 'version-2', versionNumber: 2, effectiveAt: '2026-02-01T00:00:00Z', isPublished: false, components: [] };
  const request = async (path, options) => {
    calls.push({ path, options });
    if (options?.method === 'POST') return json(nextVersion, 201);
    return json({ items: ++reads === 1 ? [{ id: 'version-1', versionNumber: 1, effectiveAt: '2026-01-01T00:00:00Z', isPublished: true, components: [{ id: 'c1' }] }] : [nextVersion] });
  };
  let selected;
  const view = await mount(React.createElement(CatalogHistoryPanel, {
    request, standard: { id: 'standard-1', name: 'Household parcel' }, selected: null, onSelect: item => { selected = item; },
  }));
  assert.match(view.root.findAllByType('strong')[0].children.join(''), /Version 1/);
  await fill(view, 'effective-at', '2026-02-01T00:00');
  await submit(view);

  const post = calls.find(call => call.options?.method === 'POST');
  assert.equal(post.path, '/api/v1/catalog/standards/standard-1/versions');
  assert.equal(JSON.parse(post.options.body).effectiveAt, new Date('2026-02-01T00:00').toISOString());
  assert.equal(selected.id, 'version-2');
  await React.act(async () => view.unmount());
});

test('catalog validation and authorization errors are shown to the coordinator', async () => {
  const deniedRequest = async () => {
    throw new ApiError('forbidden', 403, Response.json({ title: 'Forbidden' }, { status: 403 }));
  };
  const view = await mount(React.createElement(CatalogCategoryPanel, {
    request: deniedRequest, selected: null, onSelect() {},
  }));
  assert.match(view.root.findByProps({ role: 'alert' }).children.join(''), /Coordinator or Administrator access is required/);
  await React.act(async () => view.unmount());

  let deniedSaveView;
  const invalidRequest = async (_path, options) => {
    if (options.method === 'POST') {
      throw new ApiError('invalid', 400, Response.json({ title: 'Category code is invalid.' }, { status: 400 }));
    }
    return json({ items: [] });
  };
  deniedSaveView = await mount(React.createElement(CatalogCategoryPanel, {
    request: invalidRequest, selected: null, onSelect() {},
  }));
  await fill(deniedSaveView, 'category-code', 'INVALID');
  await submit(deniedSaveView);
  assert.match(deniedSaveView.root.findByProps({ role: 'alert' }).children.join(''), /Category code is invalid/);
  await React.act(async () => deniedSaveView.unmount());
});

test('stale service publication shows the API conflict and does not claim success', async () => {
  let reads = 0;
  const request = async (path, options) => {
    if (path.endsWith('/service-versions/service-1') && options.method === 'GET') {
      return json({ id: 'service-1' }, 200, { ETag: '"service-v1"' });
    }
    if (path.endsWith('/publish')) {
      throw new ApiError('conflict', 409, Response.json({ title: 'This service version changed.' }, { status: 409 }));
    }
    return json({ items: [{ id: 'service-1', name: 'Food', versionNumber: 1, isPublished: false, revision: ++reads }] });
  };
  const view = await mount(React.createElement(CatalogServicePanel, {
    request, category: { id: 'category-1', code: 'FOOD' }, selected: null, onSelect() {},
  }));
  await React.act(async () => button(view, 'Publish').props.onClick());

  assert.match(view.root.findByProps({ role: 'alert' }).children.join(''), /This service version changed/);
  assert.equal(view.root.findAllByProps({ role: 'status' }).length, 0);
  await React.act(async () => view.unmount());
});
