import { useState } from 'react';
import { Text, TextInput, View } from 'react-native';
import { PageShell } from './PageShell';
import { useBeneficiaryDemo } from '../session/useBeneficiaryDemo';
import { styles } from './styles';
import { ActionButton, AuthPanel, ConsentRow, RequestList } from './BeneficiaryDemoSections';

export function BeneficiaryDemoPage() {
  const demo = useBeneficiaryDemo();
  const [mode, setMode] = useState<'register' | 'login'>('register');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [description, setDescription] = useState('');
  const [authorityConfirmed, setAuthorityConfirmed] = useState(false);
  const [sharingAgreed, setSharingAgreed] = useState(false);
  const submitAuth = () => {
    if (!email.trim() || !password) return;
    if (mode === 'register') {
      if (!name.trim()) return;
      void demo.register(name.trim(), email.trim(), password).then(created => { if (created) setMode('login'); });
      return;
    }
    void demo.login(email.trim(), password);
  };
  const submitRequest = () => {
    if (description.trim().length < 10 || !authorityConfirmed || !sharingAgreed) return;
    void demo.submitRequest(description.trim()).then(saved => { if (saved) setDescription(''); });
  };
  return (
    <PageShell>
      <View style={styles.demoIntro}>
        <Text style={styles.eyebrow}>FICTIONAL DATA DEMO</Text>
        <Text accessibilityRole="header" style={styles.pageTitle}>Beneficiary request demo</Text>
        <Text style={styles.body}>Create a demo account, sign in, submit one request for a single need, and see its reference and status.</Text>
      </View>
      {demo.error ? <Text accessibilityRole="alert" style={styles.errorBox}>{demo.error}</Text> : null}
      {demo.notice ? <Text accessibilityRole="status" style={styles.noticeBox}>{demo.notice}</Text> : null}
      {demo.session.status === 'loading' ? <Text style={styles.body}>Checking your session…</Text> : null}
      {demo.session.status === 'unavailable' ? <Text style={styles.errorBox}>The session service is unavailable. Check that the API is running, then reload this page.</Text> : null}
      {demo.session.status === 'anonymous' ? <AuthPanel mode={mode} setMode={setMode} name={name} setName={setName} email={email} setEmail={setEmail} password={password} setPassword={setPassword} onSubmit={submitAuth} busy={demo.busy} signup={demo.signup} /> : null}
      {demo.session.status === 'authenticated' && !demo.session.user.roles.includes('Beneficiary') ? <View style={styles.demoCard}><Text style={styles.cardTitle}>This demo is for beneficiary accounts</Text><Text style={styles.body}>Sign out and use a fictional beneficiary account to continue.</Text><ActionButton label="Sign out" onPress={() => void demo.logout()} busy={demo.busy} /></View> : null}
      {demo.session.status === 'authenticated' && demo.session.user.roles.includes('Beneficiary') ? <View style={styles.demoCard}>
        <View style={styles.accountRow}><View><Text style={styles.cardTitle}>Welcome, {demo.session.user.displayName}</Text><Text style={styles.body}>Beneficiary reference: {demo.profile?.reference ?? 'Loading…'}</Text></View><ActionButton label="Sign out" onPress={() => void demo.logout()} busy={demo.busy} /></View>
        <Text style={styles.sectionTitle}>Submit a single-need request</Text>
        <TextInput accessibilityLabel="Describe the one need" value={description} onChangeText={setDescription} placeholder="Describe the support needed" multiline style={styles.textArea} />
        <ConsentRow label="I am authorized to make this request." value={authorityConfirmed} onToggle={() => setAuthorityConfirmed(!authorityConfirmed)} />
        <ConsentRow label="I agree that these details can be shared with relevant support organizations." value={sharingAgreed} onToggle={() => setSharingAgreed(!sharingAgreed)} />
        <ActionButton label="Submit request" onPress={submitRequest} busy={demo.busy} disabled={description.trim().length < 10 || !authorityConfirmed || !sharingAgreed} />
        <RequestList requests={demo.requests} />
        <Text style={styles.scopeNote}>This screen covers one need per request. Multi-need requests and charity or coordinator workflows are outside this demo.</Text>
      </View> : null}
    </PageShell>
  );
}

