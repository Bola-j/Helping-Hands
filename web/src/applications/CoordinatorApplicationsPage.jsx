import { useSession } from '../session/SessionProvider';
import { ApplicationGate } from './ApplicationGate';
import { CoordinatorApplicationsWorkspace } from './CoordinatorApplicationsWorkspace';

export function CoordinatorApplicationsPage() {
  const { state, request } = useSession();
  return <ApplicationGate state={state} allowedRoles={['Coordinator', 'Administrator']} prompt="Sign in with a coordinator or administrator account to review applications.">
    <CoordinatorApplicationsWorkspace request={request} user={state.user} />
  </ApplicationGate>;
}
