import { Link } from 'expo-router';
import { ReactNode } from 'react';
import { ScrollView, Text, View } from 'react-native';
import { styles } from './styles';

export function PageShell({ children }: { children: ReactNode }) {
  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.pageContent}>
      <View style={styles.header}>
        <Link href="/" style={styles.brand} accessibilityLabel="Helping Hands home">Helping Hands</Link>
        <Link href="/how-it-works" style={styles.navLink}>How it works</Link>
      </View>
      <View style={styles.content}>{children}</View>
      <View style={styles.footer}><Text style={styles.footerText}>Helping Hands · Assistance happens in the community</Text></View>
    </ScrollView>
  );
}
