import { Link } from 'expo-router';
import { Text, View } from 'react-native';
import { PageShell } from './PageShell';
import { styles } from './styles';

export function HomePage() {
  return (
    <PageShell>
      <View style={styles.hero}>
        <View style={styles.heroCopy}>
          <Text style={styles.eyebrow}>A place to coordinate support</Text>
          <Text accessibilityRole="header" style={styles.title}>Coordinate requests for assistance.</Text>
          <Text style={styles.lead}>Helping Hands helps beneficiaries, charities and coordinators follow a request from review to reported outcome.</Text>
          <Link href="/how-it-works" style={styles.primaryLink}>See how it works  →</Link>
        </View>
        <Journey />
      </View>
      <View style={styles.note}><Text style={styles.noteTitle}>Assistance takes place outside the platform.</Text><Text style={styles.body}>Helping Hands records the request, referral and reported outcome. It does not collect money or deliver goods.</Text></View>
    </PageShell>
  );
}

function Journey() {
  return (
    <View style={styles.journey}>
      <Text style={styles.journeyLabel}>THE PATH OF A REQUEST</Text>
      <Text style={styles.journeyStep}>A person asks for support</Text>
      <Text style={styles.journeyConnector}>│</Text>
      <Text style={styles.journeyStep}>A coordinator reviews the request</Text>
      <Text style={styles.journeyConnector}>│</Text>
      <Text style={styles.journeyStep}>An approved charity is referred</Text>
      <Text style={styles.journeyConnector}>│</Text>
      <Text style={styles.journeyStep}>The reported outcome is reviewed</Text>
    </View>
  );
}
