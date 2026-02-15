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
  View,
} from 'react-native';
import { router } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import {
  createExerciseTemplate,
  IMPLEMENT_TYPES,
  EXERCISE_TYPES,
} from '@/services/exercise-templates';

export default function CreateExerciseTemplateScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [name, setName] = useState('');
  const [implementTypeIndex, setImplementTypeIndex] = useState<number | null>(null);
  const [exerciseTypeIndex, setExerciseTypeIndex] = useState<number | null>(null);
  const [model, setModel] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!name.trim()) {
      newErrors.name = 'Name is required.';
    }
    if (implementTypeIndex === null) {
      newErrors.implementType = 'Equipment type is required.';
    }
    if (exerciseTypeIndex === null) {
      newErrors.exerciseType = 'Exercise type is required.';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validate()) return;

    setIsSaving(true);
    try {
      await createExerciseTemplate(api, {
        name: name.trim(),
        implementType: implementTypeIndex!,
        exerciseType: exerciseTypeIndex!,
        model: model.trim() || undefined,
      });
      router.back();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Failed to create exercise template.';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  const renderSelector = (
    label: string,
    options: readonly string[],
    selectedIndex: number | null,
    onSelect: (index: number) => void,
    errorKey: string,
  ) => (
    <View style={styles.selectorSection}>
      <Text style={[styles.label, { color: colors.text }]}>{label} *</Text>
      <View style={styles.optionsGrid}>
        {options.map((option, index) => {
          const isSelected = selectedIndex === index;
          return (
            <TouchableOpacity
              key={option}
              style={[
                styles.optionChip,
                isSelected
                  ? { backgroundColor: colors.tint, borderColor: colors.tint }
                  : { backgroundColor: colors.icon + '10', borderColor: colors.icon + '30' },
              ]}
              onPress={() => onSelect(index)}
              disabled={isSaving}
            >
              <Text
                style={[
                  styles.optionChipText,
                  { color: isSelected ? '#fff' : colors.text },
                ]}
              >
                {option}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>
      {errors[errorKey] && <Text style={styles.errorText}>{errors[errorKey]}</Text>}
    </View>
  );

  return (
    <KeyboardAvoidingView
      style={[styles.container, { backgroundColor: colors.background }]}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <Text style={[styles.title, { color: colors.text }]}>New Exercise</Text>

        <Text style={[styles.label, { color: colors.text }]}>Name *</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. Bench Press, Squat"
          placeholderTextColor={colors.icon}
          value={name}
          onChangeText={setName}
          autoCapitalize="words"
          editable={!isSaving}
        />
        {errors.name && <Text style={styles.errorText}>{errors.name}</Text>}

        {renderSelector(
          'Equipment Type',
          IMPLEMENT_TYPES,
          implementTypeIndex,
          setImplementTypeIndex,
          'implementType',
        )}

        {renderSelector(
          'Exercise Type',
          EXERCISE_TYPES,
          exerciseTypeIndex,
          setExerciseTypeIndex,
          'exerciseType',
        )}

        <Text style={[styles.label, { color: colors.text }]}>Model</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="Optional model or variation name"
          placeholderTextColor={colors.icon}
          value={model}
          onChangeText={setModel}
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
            <Text style={styles.saveButtonText}>Create Exercise</Text>
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
  selectorSection: {
    marginBottom: 16,
  },
  optionsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  optionChip: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 8,
    borderWidth: 1,
  },
  optionChipText: {
    fontSize: 14,
    fontWeight: '500',
  },
  errorText: {
    color: '#ef4444',
    fontSize: 13,
    marginTop: 4,
    marginBottom: 8,
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
