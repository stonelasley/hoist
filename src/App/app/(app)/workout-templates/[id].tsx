import { useCallback, useState } from 'react';
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
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import {
  getWorkoutTemplate,
  updateWorkoutTemplate,
  deleteWorkoutTemplate,
  updateWorkoutTemplateExercises,
} from '@/services/workout-templates';
import type { WorkoutTemplateExerciseDto } from '@/services/workout-templates';
import ExercisePickerModal from '@/components/ExercisePickerModal';

export default function WorkoutTemplateDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [name, setName] = useState('');
  const [notes, setNotes] = useState('');
  const [location, setLocation] = useState('');
  const [exercises, setExercises] = useState<WorkoutTemplateExerciseDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [showPicker, setShowPicker] = useState(false);

  const loadTemplate = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      const template = await getWorkoutTemplate(api, parseInt(id, 10));
      setName(template.name);
      setNotes(template.notes ?? '');
      setLocation(template.location ?? '');
      setExercises(template.exercises);
    } catch (err) {
      console.error('Failed to load workout template:', err);
      Alert.alert('Error', 'Failed to load workout template.');
    } finally {
      setIsLoading(false);
    }
  }, [api, id]);

  useFocusEffect(
    useCallback(() => {
      loadTemplate();
    }, [loadTemplate]),
  );

  const handleSave = async () => {
    const trimmedName = name.trim();
    if (!trimmedName) {
      Alert.alert('Validation', 'Name is required.');
      return;
    }

    setIsSaving(true);
    try {
      const templateId = parseInt(id!, 10);

      await updateWorkoutTemplate(api, templateId, {
        id: templateId,
        name: trimmedName,
        notes: notes.trim() || undefined,
        location: location.trim() || undefined,
      });

      await updateWorkoutTemplateExercises(
        api,
        templateId,
        exercises.map((e) => e.exerciseTemplateId),
      );

      router.back();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Failed to save workout template.';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = () => {
    Alert.alert(
      'Delete Workout',
      'Are you sure you want to delete this workout template? This cannot be undone.',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Delete',
          style: 'destructive',
          onPress: async () => {
            try {
              await deleteWorkoutTemplate(api, parseInt(id!, 10));
              router.back();
            } catch (err: unknown) {
              const message =
                err instanceof Error ? err.message : 'Failed to delete workout template.';
              Alert.alert('Error', message);
            }
          },
        },
      ],
    );
  };

  const handleExerciseSelected = (exerciseId: number) => {
    setShowPicker(false);
    // Add a placeholder exercise entry - the name will be resolved from the server on next load
    const newExercise: WorkoutTemplateExerciseDto = {
      id: 0,
      exerciseTemplateId: exerciseId,
      exerciseName: 'Loading...',
      implementType: '',
      exerciseType: '',
      position: exercises.length,
    };
    setExercises((prev) => [...prev, newExercise]);
  };

  const handleRemoveExercise = (index: number) => {
    setExercises((prev) => prev.filter((_, i) => i !== index));
  };

  const handleMoveExerciseUp = (index: number) => {
    if (index === 0) return;
    setExercises((prev) => {
      const newExercises = [...prev];
      [newExercises[index - 1], newExercises[index]] = [
        newExercises[index],
        newExercises[index - 1],
      ];
      return newExercises;
    });
  };

  const handleMoveExerciseDown = (index: number) => {
    if (index === exercises.length - 1) return;
    setExercises((prev) => {
      const newExercises = [...prev];
      [newExercises[index], newExercises[index + 1]] = [
        newExercises[index + 1],
        newExercises[index],
      ];
      return newExercises;
    });
  };

  if (isLoading) {
    return (
      <View style={[styles.loadingContainer, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.tint} />
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={[styles.container, { backgroundColor: colors.background }]}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <Text style={[styles.title, { color: colors.text }]}>Edit Workout</Text>

        <Text style={[styles.label, { color: colors.text }]}>Name *</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="Workout name"
          placeholderTextColor={colors.icon}
          value={name}
          onChangeText={setName}
          autoCapitalize="words"
          editable={!isSaving}
        />

        <Text style={[styles.label, { color: colors.text }]}>Notes</Text>
        <TextInput
          style={[
            styles.input,
            styles.textArea,
            { color: colors.text, borderColor: colors.icon + '40' },
          ]}
          placeholder="Optional notes"
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
          placeholder="Optional location"
          placeholderTextColor={colors.icon}
          value={location}
          onChangeText={setLocation}
          autoCapitalize="words"
          editable={!isSaving}
        />

        <View style={styles.exercisesSection}>
          <View style={styles.exercisesHeader}>
            <Text style={[styles.label, { color: colors.text, marginBottom: 0 }]}>
              Exercises ({exercises.length})
            </Text>
            <TouchableOpacity
              style={[styles.addExerciseButton, { backgroundColor: colors.tint }]}
              onPress={() => setShowPicker(true)}
            >
              <Text style={styles.addExerciseButtonText}>+ Add</Text>
            </TouchableOpacity>
          </View>

          {exercises.length === 0 ? (
            <Text style={[styles.noExercisesText, { color: colors.icon }]}>
              No exercises added yet. Tap &quot;+ Add&quot; to add exercises.
            </Text>
          ) : (
            exercises.map((exercise, index) => (
              <View
                key={`${exercise.exerciseTemplateId}-${index}`}
                style={[styles.exerciseRow, { borderColor: colors.icon + '20' }]}
              >
                <View style={styles.exerciseRowContent}>
                  <View style={styles.reorderControls}>
                    <TouchableOpacity
                      style={[
                        styles.reorderButton,
                        index === 0 && styles.reorderButtonDisabled,
                      ]}
                      onPress={() => handleMoveExerciseUp(index)}
                      disabled={index === 0}
                    >
                      <Text
                        style={[
                          styles.reorderButtonText,
                          { color: index === 0 ? colors.icon + '40' : colors.icon },
                        ]}
                      >
                        ↑
                      </Text>
                    </TouchableOpacity>
                    <TouchableOpacity
                      style={[
                        styles.reorderButton,
                        index === exercises.length - 1 && styles.reorderButtonDisabled,
                      ]}
                      onPress={() => handleMoveExerciseDown(index)}
                      disabled={index === exercises.length - 1}
                    >
                      <Text
                        style={[
                          styles.reorderButtonText,
                          {
                            color:
                              index === exercises.length - 1 ? colors.icon + '40' : colors.icon,
                          },
                        ]}
                      >
                        ↓
                      </Text>
                    </TouchableOpacity>
                  </View>
                  <Text style={[styles.exercisePosition, { color: colors.icon }]}>
                    {index + 1}.
                  </Text>
                  <View style={styles.exerciseInfo}>
                    <Text
                      style={[styles.exerciseName, { color: colors.text }]}
                      numberOfLines={1}
                    >
                      {exercise.exerciseName}
                    </Text>
                    {exercise.implementType ? (
                      <Text style={[styles.exerciseMeta, { color: colors.icon }]}>
                        {exercise.implementType} - {exercise.exerciseType}
                      </Text>
                    ) : null}
                  </View>
                  <TouchableOpacity
                    style={styles.removeButton}
                    onPress={() => handleRemoveExercise(index)}
                  >
                    <Text style={styles.removeButtonText}>Remove</Text>
                  </TouchableOpacity>
                </View>
              </View>
            ))
          )}
        </View>

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
            <Text style={styles.saveButtonText}>Save Changes</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity style={styles.deleteButton} onPress={handleDelete} disabled={isSaving}>
          <Text style={styles.deleteButtonText}>Delete Workout</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.cancelButton}
          onPress={() => router.back()}
          disabled={isSaving}
        >
          <Text style={[styles.cancelButtonText, { color: colors.icon }]}>Cancel</Text>
        </TouchableOpacity>
      </ScrollView>

      <ExercisePickerModal
        visible={showPicker}
        onClose={() => setShowPicker(false)}
        onSelect={handleExerciseSelected}
        onCreateNew={() => {
          setShowPicker(false);
          router.push('/(app)/exercise-templates/create');
        }}
      />
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
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
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
  exercisesSection: {
    marginBottom: 24,
  },
  exercisesHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  addExerciseButton: {
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 6,
  },
  addExerciseButtonText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
  noExercisesText: {
    fontSize: 14,
    textAlign: 'center',
    paddingVertical: 20,
  },
  exerciseRow: {
    borderBottomWidth: 1,
    paddingVertical: 12,
  },
  exerciseRowContent: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  reorderControls: {
    flexDirection: 'column',
    marginRight: 8,
  },
  reorderButton: {
    width: 24,
    height: 20,
    justifyContent: 'center',
    alignItems: 'center',
  },
  reorderButtonDisabled: {
    opacity: 0.3,
  },
  reorderButtonText: {
    fontSize: 16,
    fontWeight: '600',
    lineHeight: 18,
  },
  exercisePosition: {
    fontSize: 14,
    fontWeight: '600',
    width: 28,
  },
  exerciseInfo: {
    flex: 1,
    gap: 2,
  },
  exerciseName: {
    fontSize: 15,
    fontWeight: '500',
  },
  exerciseMeta: {
    fontSize: 12,
  },
  removeButton: {
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  removeButtonText: {
    color: '#ef4444',
    fontSize: 13,
    fontWeight: '500',
  },
  saveButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
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
  deleteButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#ef4444',
  },
  deleteButtonText: {
    color: '#ef4444',
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
