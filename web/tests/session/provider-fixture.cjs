const React = require('react');
const { create, act } = require('react-test-renderer');
const { SessionProvider, useSession } = require('../../src/session/SessionProvider.tsx');

globalThis.IS_REACT_ACT_ENVIRONMENT = true;
const user = { id: '11111111-1111-4111-8111-111111111111', displayName: 'Example User', email: 'example@example.test', roles: ['Coordinator'] };

async function mount() {
  let session;
  function Observer() { session = useSession(); return null; }
  let renderer;
  await act(async () => { renderer = create(React.createElement(SessionProvider, null, React.createElement(Observer))); });
  return { get session() { return session; }, close: async () => act(async () => renderer.unmount()) };
}

function token() { return Response.json({ requestToken: 'test-token', headerName: 'X-CSRF-TOKEN' }); }
module.exports = { act, mount, token, user };
