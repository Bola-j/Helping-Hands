import { Text, View } from 'react-native';
import { BeneficiarySignup, DemoRequest } from '../session/demoTypes';
import { styles } from './styles';
import { ActionButton, ConsentRow, Field, ModeButton, RequestList } from './DemoControls';

export { ActionButton, ConsentRow, RequestList };

export function AuthPanel(props: { mode: 'register' | 'login'; setMode: (mode: 'register' | 'login') => void; name: string; setName: (value: string) => void; email: string; setEmail: (value: string) => void; password: string; setPassword: (value: string) => void; onSubmit: () => void; busy: boolean; signup: BeneficiarySignup | null }) {
  return <View style={styles.demoCard}>
    <View style={styles.modeRow}><ModeButton label="Create account" active={props.mode === 'register'} onPress={() => props.setMode('register')} /><ModeButton label="Sign in" active={props.mode === 'login'} onPress={() => props.setMode('login')} /></View>
    {props.signup ? <Text style={styles.noticeBox}>Account created with reference {props.signup.reference}. Sign in to continue.</Text> : null}
    {props.mode === 'register' ? <Field label="Display name" value={props.name} setValue={props.setName} autoComplete="name" /> : null}
    <Field label="Email" value={props.email} setValue={props.setEmail} autoComplete="email" />
    <Field label="Password" value={props.password} setValue={props.setPassword} secureTextEntry />
    <ActionButton label={props.mode === 'register' ? 'Create beneficiary account' : 'Sign in'} onPress={props.onSubmit} busy={props.busy} disabled={!props.email.trim() || !props.password || (props.mode === 'register' && !props.name.trim())} />
    <Text style={styles.scopeNote}>Use fictional details for this demonstration. Registration and sign-in are separate steps.</Text>
  </View>;
}
