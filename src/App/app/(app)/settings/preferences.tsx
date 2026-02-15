import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { getUserPreferences, upsertUserPreferences } from '@/services/user-preferences';

const WEIGHT_UNITS = ['Lbs', 'Kg'] as const;
const DISTANCE_UNITS = ['Miles', 'Kilometers', 'Meters', 'Yards'] as const;

export default function PreferencesScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [weightUnit, setWeightUnit] = useState<string>('Lbs');
  const [distanceUnit, setDistanceUnit] = useState<string>('Miles');
  const [bodyweight, setBodyweight] = useState<string>('');

  const loadPreferences = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await getUserPreferences(api);
      setWeightUnit(data.weightUnit);
      setDistanceUnit(data.distanceUnit);
      setBodyweight(data.bodyweight ? data.bodyweight.toString() : '');
    } catch (err) {
      console.error('Failed to load preferences:', err);
      Alert.alert('Error', 'Failed to load preferences');
    } finally {
      setIsLoading(false);
    }
  }, [api]);

  useFocusEffect(
    useCallback(() => {
      loadPreferences();
    }, [loadPreferences]),
  );

  const handleSave = async () => {
    try {
      setIsSaving(true);

      const bodyweightValue = bodyweight.trim() === '' ? undefined : parseFloat(bodyweight);

      if (bodyweightValue !== undefined && (isNaN(bodyweightValue) || bodyweightValue <= 0)) {
        Alert.alert('Error', 'Bodyweight must be a positive number');
        return;
      }

      await upsertUserPreferences(api, {
        weightUnit,
        distanceUnit,
        bodyweight: bodyweightValue,
      });

      Alert.alert('Success', 'Preferences saved successfully', [
        { text: 'OK', onPress: () => router.back() },
      ]);
    } catch (err) {
      console.error('Failed to save preferences:', err);
      const message = err instanceof Error ? err.message : 'Failed to save preferences';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <View style={[styles.loadingContainer, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.tint} />
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => router.back()} style={styles.backButton}>
          <Text style={[styles.backButtonText, { color: colors.tint }]}>Back</Text>
        </TouchableOpacity>
        <Text style={[styles.headerTitle, { color: colors.text }]}>Preferences</Text>
        <View style={styles.backButton} />
      </View>

      <ScrollView style={styles.content} contentContainerStyle={styles.contentContainer}>
        <View style={styles.section}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>Weight Unit</Text>
          <View style={styles.segmentedControl}>
            {WEIGHT_UNITS.map((unit) => (
              <TouchableOpacity
                key={unit}
                style={[
                  styles.segmentButton,
                  {
                    backgroundColor:
                      weightUnit === unit ? colors.tint : colors.background,
                    borderColor: colors.icon + '40',
                  },
                ]}
                onPress={() => setWeightUnit(unit)}
              >
                <Text
                  style={[
                    styles.segmentText,
                    {
                      color: weightUnit === unit ? '#fff' : colors.text,
                    },
                  ]}
                >
                  {unit}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        <View style={styles.section}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>Distance Unit</Text>
          <View style={styles.segmentedControl}>
            {DISTANCE_UNITS.map((unit) => (
              <TouchableOpacity
                key={unit}
                style={[
                  styles.segmentButton,
                  {
                    backgroundColor:
                      distanceUnit === unit ? colors.tint : colors.background,
                    borderColor: colors.icon + '40',
                  },
                ]}
                onPress={() => setDistanceUnit(unit)}
              >
                <Text
                  style={[
                    styles.segmentText,
                    {
                      color: distanceUnit === unit ? '#fff' : colors.text,
                    },
                  ]}
                >
                  {unit}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        <View style={styles.section}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>
            Bodyweight ({weightUnit})
          </Text>
          <TextInput
            style={[
              styles.input,
              {
                backgroundColor: colors.background,
                borderColor: colors.icon + '40',
                color: colors.text,
              },
            ]}
            value={bodyweight}
            onChangeText={setBodyweight}
            placeholder={`Enter bodyweight in ${weightUnit}`}
            placeholderTextColor={colors.icon + '80'}
            keyboardType="decimal-pad"
          />
        </View>

        <TouchableOpacity
          style={[
            styles.saveButton,
            { backgroundColor: colors.tint },
            isSaving && styles.saveButtonDisabled,
          ]}
          onPress={handleSave}
          disabled={isSaving}
        >
          {isSaving ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <Text style={styles.saveButtonText}>Save</Text>
          )}
        </TouchableOpacity>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
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
    marginBottom: 32,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 12,
  },
  segmentedControl: {
    flexDirection: 'row',
    gap: 8,
    flexWrap: 'wrap',
  },
  segmentButton: {
    flex: 1,
    minWidth: 80,
    paddingVertical: 12,
    paddingHorizontal: 16,
    borderRadius: 8,
    borderWidth: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  segmentText: {
    fontSize: 14,
    fontWeight: '600',
  },
  input: {
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 16,
    paddingVertical: 12,
    fontSize: 16,
  },
  saveButton: {
    paddingVertical: 16,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 8,
  },
  saveButtonDisabled: {
    opacity: 0.6,
  },
  saveButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
