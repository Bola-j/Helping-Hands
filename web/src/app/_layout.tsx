import { Stack } from 'expo-router';
import { SessionProvider } from '../session/SessionProvider';

export default function RootLayout() {
  return <SessionProvider><Stack screenOptions={{ headerShown: false }} /></SessionProvider>;
}
