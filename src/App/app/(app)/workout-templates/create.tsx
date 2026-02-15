import { useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
} from 'react-native';
import { router } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { createWorkoutTemplate } from '@/services/workout-templates';

export default function CreateWorkoutTemplateScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [name, setName] = useState('');
  const [notes, setNotes] = useState('');
  const [location, setLocation] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSave = async () => {
    const trimmedName = name.trim();
    if (!trimmedName) {
      setError('Name is required.');
      return;
    }

    setError(null);
    setIsSaving(true);

    try {
      await createWorkoutTemplate(api, {
        name: trimmedName,
        notes: notes.trim() || undefined,
        location: location.trim() || undefined,
      });
      router.back();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Failed to create workout template.';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={[styles.container, { backgroundColor: colors.background }]}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <Text style={[styles.title, { color: colors.text }]}>New Workout</Text>

        <Text style={[styles.label, { color: colors.text }]}>Name *</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. Push Day, Upper Body A"
          placeholderTextColor={colors.icon}
          value={name}
          onChangeText={setName}
          autoCapitalize="words"
          editable={!isSaving}
        />
        {error && <Text style={styles.errorText}>{error}</Text>}

        <Text style={[styles.label, { color: colors.text }]}>Notes</Text>
        <TextInput
          style={[
            styles.input,
            styles.textArea,
            { color: colors.text, borderColor: colors.icon + '40' },
          ]}
          placeholder="Optional notes about this workout"
          placeholderTextColor={colors.icon}
          value={notes}
          onChangeText={setNotes}
          multiline
          numberOfLines={3}
          textAlignVertical="top"
          editable={!isSaving}
        />

        <Text style={[styles.label, { color: colors.text }]}>Location</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. Home Gym, Planet Fitness"
          placeholderTextColor={colors.icon}
          value={location}
          onChangeText={setLocation}
          autoCapitalize="words"
          editable={!isSaving}
        />

        <TouchableOpacity
          style={[
            styles.saveButton,
            { backgroundColor: colors.tint },
            isSaving && styles.disabledButton,
          ]}
          onPress={handleSave}
          disabled={isSaving}
        >
          {isSaving ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <Text style={styles.saveButtonText}>Create Workout</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.cancelButton}
          onPress={() => router.back()}
          disabled={isSaving}
        >
          <Text style={[styles.cancelButtonText, { color: colors.icon }]}>Cancel</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    paddingHorizontal: 24,
    paddingTop: 60,
    paddingBottom: 32,
  },
  title: {
    fontSize: 28,
    fontWeight: '700',
    marginBottom: 24,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    marginBottom: 6,
  },
  input: {
    height: 48,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 16,
    fontSize: 16,
    marginBottom: 16,
  },
  textArea: {
    height: 100,
    paddingTop: 12,
  },
  errorText: {
    color: '#ef4444',
    fontSize: 13,
    marginTop: -12,
    marginBottom: 12,
  },
  saveButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 8,
    marginBottom: 12,
  },
  disabledButton: {
    opacity: 0.6,
  },
  saveButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  cancelButton: {
    height: 48,
    justifyContent: 'center',
    alignItems: 'center',
  },
  cancelButtonText: {
    fontSize: 16,
  },
});
