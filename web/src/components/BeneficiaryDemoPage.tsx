
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

  const requests = demo.requests;
  const totalRequests = requests.length;
  const submittedRequests = requests.filter(item => item.status === 'Submitted').length;
  const clarificationRequests = requests.filter(item => item.status === 'Clarification').length;
  const acceptedRequests = requests.filter(item => item.status === 'Accepted').length;
  const rejectedRequests = requests.filter(item => item.status === 'Rejected').length;
  const completedRequests = requests.filter(item => item.status === 'Completed').length;

  const submitAuth = () => {
    if (!email.trim() || !password) return;
    if (mode === 'register') {
      if (!name.trim()) return;
      void demo.register(name.trim(), email.trim(), password).then(created => {
        if (created) setMode('login');
      });
      return;
    }
    void demo.login(email.trim(), password);
  };

  const submitRequest = () => {
    if (description.trim().length < 10 || !authorityConfirmed || !sharingAgreed) return;
    void demo.submitRequest(description.trim()).then(saved => {
      if (saved) setDescription('');
    });
  };

  return (
    <PageShell>
      <View style={styles.demoIntro}>
        <Text style={styles.eyebrow}>FICTIONAL DATA DEMO</Text>
        <Text accessibilityRole="header" style={styles.pageTitle}>
          Beneficiary request demo
        </Text>
        <Text style={styles.body}>
          Create a demo account, sign in, submit one request for a single need,
          and see its reference and status.
        </Text>
      </View>

      {demo.error ? <Text accessibilityRole="alert" style={styles.errorBox}>{demo.error}</Text> : null}
      {demo.notice ? <Text accessibilityRole="status" style={styles.noticeBox}>{demo.notice}</Text> : null}

      {demo.session.status === 'loading' ? (
        <Text style={styles.body}>Checking your session…</Text>
      ) : null}

      {demo.session.status === 'unavailable' ? (
        <Text style={styles.errorBox}>
          The session service is unavailable. Check that the API is running, then reload this page.
        </Text>
      ) : null}

      {demo.session.status === 'anonymous' ? (
        <AuthPanel
          mode={mode}
          setMode={setMode}
          name={name}
          setName={setName}
          email={email}
          setEmail={setEmail}
          password={password}
          setPassword={setPassword}
          onSubmit={submitAuth}
          busy={demo.busy}
          signup={demo.signup}
        />
      ) : null}

      {demo.session.status === 'authenticated' &&
      !demo.session.user.roles.includes('Beneficiary') ? (
        <View style={styles.demoCard}>
          <Text style={styles.cardTitle}>This demo is for beneficiary accounts</Text>
          <Text style={styles.body}>
            Sign out and use a fictional beneficiary account to continue.
          </Text>
          <ActionButton label="Sign out" onPress={() => void demo.logout()} busy={demo.busy} />
        </View>
      ) : null}

      {demo.session.status === 'authenticated' &&
      demo.session.user.roles.includes('Beneficiary') ? (
        <View style={styles.demoCard}>
          <View style={styles.accountRow}>
            <View>
              <Text style={styles.cardTitle}>
                Welcome, {demo.session.user.displayName}
              </Text>
              <Text style={styles.body}>
                Beneficiary reference: {demo.profile?.reference ?? 'Loading…'}
              </Text>
            </View>
            <ActionButton label="Sign out" onPress={() => void demo.logout()} busy={demo.busy} />
          </View>

          <Text style={styles.sectionTitle}>Your dashboard</Text>
          <Text style={styles.body}>
            A summary of your assistance requests.
          </Text>

          <View style={styles.statsGrid}>
            <View style={styles.statCard}>
              <Text style={styles.statValue}>{totalRequests}</Text>
              <Text style={styles.statLabel}>Total requests</Text>
            </View>

            <View style={styles.statCard}>
              <Text style={styles.statValue}>{submittedRequests}</Text>
              <Text style={styles.statLabel}>Awaiting review</Text>
            </View>

            <View style={styles.statCard}>
              <Text style={styles.statValue}>{clarificationRequests}</Text>
              <Text style={styles.statLabel}>Action needed</Text>
            </View>

            <View style={styles.statCard}>
              <Text style={styles.statValue}>{acceptedRequests}</Text>
              <Text style={styles.statLabel}>Accepted</Text>
            </View>

            <View style={styles.statCard}>
              <Text style={styles.statValue}>{rejectedRequests}</Text>
              <Text style={styles.statLabel}>Rejected</Text>
            </View>

            <View style={styles.statCard}>
              <Text style={styles.statValue}>{completedRequests}</Text>
              <Text style={styles.statLabel}>Completed</Text>
            </View>
          </View>

          <Text style={styles.sectionTitle}>Submit a single-need request</Text>

          <TextInput
            accessibilityLabel="Describe the one need"
            value={description}
            onChangeText={setDescription}
            placeholder="Describe the support needed"
            multiline
            style={styles.textArea}
          />

          <ConsentRow
            label="I am authorized to make this request."
            value={authorityConfirmed}
            onToggle={() => setAuthorityConfirmed(!authorityConfirmed)}
          />

          <ConsentRow
            label="I agree that these details can be shared with relevant support organizations."
            value={sharingAgreed}
            onToggle={() => setSharingAgreed(!sharingAgreed)}
          />

          <ActionButton
            label="Submit request"
            onPress={submitRequest}
            busy={demo.busy}
            disabled={
              description.trim().length < 10 ||
              !authorityConfirmed ||
              !sharingAgreed
            }
          />

          <RequestList requests={requests} />

          <Text style={styles.scopeNote}>
            This screen covers one need per request. Multi-need requests and charity
            or coordinator workflows are outside this demo.
          </Text>
        </View>
      ) : null}
    </PageShell>
  );
}
