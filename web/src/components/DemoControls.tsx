import { Pressable, Text, TextInput, View } from 'react-native';
import { styles } from './styles';
import { DemoRequest } from '../session/demoTypes';

export function Field(props: { label: string; value: string; setValue: (value: string) => void; autoComplete?: 'name' | 'email'; secureTextEntry?: boolean }) {
  return <View style={styles.field}><Text style={styles.fieldLabel}>{props.label}</Text><TextInput accessibilityLabel={props.label} autoComplete={props.autoComplete} keyboardType={props.label === 'Email' ? 'email-address' : 'default'} autoCapitalize={props.label === 'Email' ? 'none' : 'words'} secureTextEntry={props.secureTextEntry} value={props.value} onChangeText={props.setValue} style={styles.input} /></View>;
}

export function ActionButton(props: { label: string; onPress: () => void; busy: boolean; disabled?: boolean }) {
  return <Pressable accessibilityRole="button" disabled={props.busy || props.disabled} onPress={props.onPress} style={[styles.actionButton, (props.busy || props.disabled) && styles.disabledButton]}><Text style={styles.actionButtonText}>{props.busy ? 'Please wait…' : props.label}</Text></Pressable>;
}

export function ModeButton(props: { label: string; active: boolean; onPress: () => void }) {
  return <Pressable accessibilityRole="button" accessibilityState={{ selected: props.active }} onPress={props.onPress} style={[styles.modeButton, props.active && styles.activeModeButton]}><Text style={[styles.modeButtonText, props.active && styles.activeModeText]}>{props.label}</Text></Pressable>;
}

export function ConsentRow(props: { label: string; value: boolean; onToggle: () => void }) {
  return <Pressable accessibilityRole="checkbox" accessibilityState={{ checked: props.value }} onPress={props.onToggle} style={styles.consentRow}><Text style={styles.checkbox}>{props.value ? '✓' : ' '}</Text><Text style={styles.body}>{props.label}</Text></Pressable>;
}

export function RequestList({ requests }: { requests: DemoRequest[] }) {
  return <View style={styles.requestList}><Text style={styles.sectionTitle}>Your requests</Text>{requests.length ? requests.map(item => <View key={item.id} style={styles.requestItem}><Text style={styles.requestReference}>{item.reference} · {item.status}</Text><Text style={styles.body}>{item.description}</Text></View>) : <Text style={styles.body}>No requests yet. Submit a single-need request to see it here.</Text>}</View>;
}
