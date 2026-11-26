import { useSession } from '../session/SessionProvider';
import { ApplicationGate } from './ApplicationGate';
import { CharityApplicationWorkspace } from './CharityApplicationWorkspace';

export function CharityApplicationPage() {
  const { state, request } = useSession();
  return <ApplicationGate state={state} allowedRoles={['RecipientRepresentative']} prompt="Sign in with your charity representative account to view your application.">
    <CharityApplicationWorkspace request={request} user={state.user} />
  </ApplicationGate>;
}
