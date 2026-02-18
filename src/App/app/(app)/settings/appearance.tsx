import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { useThemePreference } from '@/hooks/useThemePreference';

type ThemePreference = 'light' | 'dark' | 'system';

const OPTIONS: { value: ThemePreference; label: string; description: string }[] = [
  { value: 'system', label: 'System', description: 'Follow device setting' },
  { value: 'light', label: 'Light', description: 'Always use light mode' },
  { value: 'dark', label: 'Dark', description: 'Always use dark mode' },
];

export default function AppearanceScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const { preference, setPreference } = useThemePreference();

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => router.back()} style={styles.backButton}>
          <Text style={[styles.backButtonText, { color: colors.tint }]}>Back</Text>
        </TouchableOpacity>
        <Text style={[styles.headerTitle, { color: colors.text }]}>Appearance</Text>
        <View style={styles.backButton} />
      </View>

      <ScrollView style={styles.content} contentContainerStyle={styles.contentContainer}>
        <View style={styles.section}>
          {OPTIONS.map((option) => (
            <TouchableOpacity
              key={option.value}
              style={[styles.optionRow, { borderColor: colors.icon + '20' }]}
              onPress={() => setPreference(option.value)}
            >
              <View style={styles.optionText}>
                <Text style={[styles.optionLabel, { color: colors.text }]}>
                  {option.label}
                </Text>
                <Text style={[styles.optionDescription, { color: colors.icon }]}>
                  {option.description}
                </Text>
              </View>
              {preference === option.value && (
                <Text style={[styles.checkmark, { color: colors.tint }]}>✓</Text>
              )}
            </TouchableOpacity>
          ))}
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingTop: 60,
    paddingBottom: 16,
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: '600',
  },
  backButton: {
    width: 60,
  },
  backButtonText: {
    fontSize: 16,
    fontWeight: '500',
  },
  content: {
    flex: 1,
  },
  contentContainer: {
    paddingHorizontal: 20,
    paddingBottom: 40,
  },
  section: {
    marginTop: 8,
  },
  optionRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 16,
    borderBottomWidth: 1,
  },
  optionText: {
    flex: 1,
  },
  optionLabel: {
    fontSize: 16,
    fontWeight: '500',
  },
  optionDescription: {
    fontSize: 13,
    marginTop: 2,
  },
  checkmark: {
    fontSize: 20,
    fontWeight: '600',
    marginLeft: 12,
  },
});
