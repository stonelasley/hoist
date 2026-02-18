import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

export default function SettingsScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  return (
    <ScrollView
      style={[styles.container, { backgroundColor: colors.background }]}
      contentContainerStyle={styles.scrollContent}
    >
      <View style={styles.header}>
        <Text style={[styles.headerTitle, { color: colors.text }]}>Settings</Text>
      </View>

      <View style={styles.section}>
        <TouchableOpacity
          style={[styles.settingItem, { borderColor: colors.icon + '20' }]}
          onPress={() => router.push('/(app)/settings/locations')}
        >
          <Text style={[styles.settingItemText, { color: colors.text }]}>My Locations</Text>
          <Text style={[styles.chevron, { color: colors.icon }]}>›</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.settingItem, { borderColor: colors.icon + '20' }]}
          onPress={() => router.push('/(app)/settings/preferences')}
        >
          <Text style={[styles.settingItemText, { color: colors.text }]}>Unit Preferences</Text>
          <Text style={[styles.chevron, { color: colors.icon }]}>›</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.settingItem, { borderColor: colors.icon + '20' }]}
          onPress={() => router.push('/(app)/settings/appearance')}
        >
          <Text style={[styles.settingItemText, { color: colors.text }]}>Appearance</Text>
          <Text style={[styles.chevron, { color: colors.icon }]}>›</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    paddingBottom: 32,
  },
  header: {
    paddingHorizontal: 20,
    paddingTop: 60,
    paddingBottom: 16,
  },
  headerTitle: {
    fontSize: 32,
    fontWeight: 'bold',
  },
  section: {
    paddingHorizontal: 20,
  },
  settingItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 16,
    borderBottomWidth: 1,
  },
  settingItemText: {
    fontSize: 16,
    fontWeight: '500',
  },
  chevron: {
    fontSize: 24,
    fontWeight: '300',
  },
});
