import { Link } from 'expo-router';
import { Text, View } from 'react-native';
import { PageShell } from './PageShell';
import { styles } from './styles';

export function HowItWorksPage() {
  return (
    <PageShell>
      <Text style={styles.eyebrow}>From request to follow-up</Text>
      <Text accessibilityRole="header" style={styles.pageTitle}>How it works</Text>
      <Text style={styles.lead}>The platform keeps a record of coordination. People and charities arrange assistance outside it.</Text>
      <View style={styles.steps}>
        <Step number="01" title="A person makes a request" text="A beneficiary describes the support they need." />
        <Step number="02" title="A coordinator reviews it" text="The coordinator reviews the request and refers eligible portions to approved charities." />
        <Step number="03" title="The outcome is reported" text="The charity reports what it delivered. A coordinator reviews the report and follows up on anything unresolved." />
      </View>
      <Link href="/" style={styles.backLink}>← Back to home</Link>
    </PageShell>
  );
}

function Step({ number, title, text }: { number: string; title: string; text: string }) {
  return <View style={styles.step}><Text style={styles.stepNumber}>{number}</Text><View style={styles.stepBody}><Text accessibilityRole="header" style={styles.stepTitle}>{title}</Text><Text style={styles.body}>{text}</Text></View></View>;
}
