import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import {
  getInProgressWorkout,
  discardWorkout,
  updateWorkoutExercises,
} from '@/services/workouts';
import type { WorkoutDetailDto } from '@/services/workouts';
import WorkoutExerciseCard from '@/components/WorkoutExerciseCard';
import ExercisePickerModal from '@/components/ExercisePickerModal';
import { getUserPreferences } from '@/services/user-preferences';

export default function ActiveWorkoutScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [workout, setWorkout] = useState<WorkoutDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showExercisePicker, setShowExercisePicker] = useState(false);
  const [defaultBodyweight, setDefaultBodyweight] = useState<number | null>(null);
  const [weightUnit, setWeightUnit] = useState<string>('Lbs');
  const [distanceUnit, setDistanceUnit] = useState<string>('Miles');

  const loadWorkout = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await getInProgressWorkout(api);
      setWorkout(data);
      const prefs = await getUserPreferences(api);
      setDefaultBodyweight(prefs.bodyweight);
      setWeightUnit(prefs.weightUnit);
      setDistanceUnit(prefs.distanceUnit);
    } catch (err) {
      console.error('Failed to load workout:', err);
      Alert.alert('Error', 'Failed to load workout.');
    } finally {
      setIsLoading(false);
    }
  }, [api]);

  useFocusEffect(
    useCallback(() => {
      loadWorkout();
    }, [loadWorkout]),
  );

  const handleDiscardWorkout = () => {
    const doDiscard = async () => {
      if (!workout) return;
      try {
        await discardWorkout(api, workout.id);
        router.replace('/');
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Failed to discard workout.';
        if (Platform.OS === 'web') {
          window.alert(message);
        } else {
          Alert.alert('Error', message);
        }
      }
    };

    if (Platform.OS === 'web') {
      if (window.confirm('Are you sure you want to discard this workout? All progress will be lost.')) {
        doDiscard();
      }
    } else {
      Alert.alert(
        'Discard Workout',
        'Are you sure you want to discard this workout? All progress will be lost.',
        [
          { text: 'Cancel', style: 'cancel' },
          {
            text: 'Discard',
            style: 'destructive',
            onPress: doDiscard,
          },
        ],
      );
    }
  };

  const handleCompleteWorkout = () => {
    if (!workout) return;
    router.push(`/(app)/workouts/complete?id=${workout.id}`);
  };

  const handleExerciseSelected = async (exerciseId: number) => {
    setShowExercisePicker(false);
    if (!workout) return;

    try {
      const currentExerciseIds = workout.exercises
        .filter((e) => e.exerciseTemplateId !== null)
        .map((e) => e.exerciseTemplateId!);

      const updatedExerciseIds = [...currentExerciseIds, exerciseId];
      await updateWorkoutExercises(api, workout.id, updatedExerciseIds);
      await loadWorkout();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to add exercise.';
      Alert.alert('Error', message);
    }
  };

  const formatStartTime = (startedAt: string): string => {
    const date = new Date(startedAt);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  if (isLoading) {
    return (
      <View style={[styles.loadingContainer, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.tint} />
      </View>
    );
  }

  if (!workout) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.emptyContainer}>
          <Text style={[styles.emptyTitle, { color: colors.text }]}>No Active Workout</Text>
          <Text style={[styles.emptyText, { color: colors.icon }]}>
            You don&apos;t have any workout in progress.
          </Text>
          <TouchableOpacity
            style={[styles.backButton, { backgroundColor: colors.tint }]}
            onPress={() => router.back()}
          >
            <Text style={styles.backButtonText}>Go Back</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.workoutHeader}>
          <Text style={[styles.title, { color: colors.text }]}>{workout.templateName}</Text>
          <View style={styles.metaRow}>
            <Text style={[styles.metaText, { color: colors.icon }]}>
              Started at {formatStartTime(workout.startedAt)}
            </Text>
            {workout.locationName && (
              <View style={[styles.locationBadge, { backgroundColor: colors.icon + '15' }]}>
                <Text style={[styles.locationBadgeText, { color: colors.icon }]}>
                  {workout.locationName}
                </Text>
              </View>
            )}
          </View>
        </View>

        <View style={styles.exercisesSection}>
          {workout.exercises.length === 0 ? (
            <View style={styles.noExercisesContainer}>
              <Text style={[styles.noExercisesText, { color: colors.icon }]}>
                No exercises added yet. Tap &quot;Add/Remove Exercises&quot; to get started.
              </Text>
            </View>
          ) : (
            workout.exercises.map((exercise) => (
              <WorkoutExerciseCard
                key={exercise.id}
                exercise={exercise}
                workoutId={workout.id}
                onSetChanged={loadWorkout}
                defaultBodyweight={defaultBodyweight}
                weightUnit={weightUnit}
                distanceUnit={distanceUnit}
              />
            ))
          )}
        </View>

        <TouchableOpacity
          style={[styles.manageExercisesButton, { borderColor: colors.tint }]}
          onPress={() => setShowExercisePicker(true)}
        >
          <Text style={[styles.manageExercisesButtonText, { color: colors.tint }]}>
            Add/Remove Exercises
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.completeButton, { backgroundColor: colors.tint }]}
          onPress={handleCompleteWorkout}
        >
          <Text style={styles.completeButtonText}>Complete Workout</Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.discardButton} onPress={handleDiscardWorkout}>
          <Text style={styles.discardButtonText}>Discard Workout</Text>
        </TouchableOpacity>
      </ScrollView>

      <ExercisePickerModal
        visible={showExercisePicker}
        onClose={() => setShowExercisePicker(false)}
        onSelect={handleExerciseSelected}
        onCreateNew={() => {
          setShowExercisePicker(false);
          router.push('/(app)/exercise-templates/create');
        }}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    paddingHorizontal: 16,
    paddingTop: 60,
    paddingBottom: 32,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 32,
  },
  emptyTitle: {
    fontSize: 22,
    fontWeight: '700',
    marginBottom: 8,
  },
  emptyText: {
    fontSize: 15,
    textAlign: 'center',
    marginBottom: 24,
  },
  backButton: {
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
  },
  backButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  workoutHeader: {
    marginBottom: 24,
  },
  title: {
    fontSize: 28,
    fontWeight: '700',
    marginBottom: 8,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  metaText: {
    fontSize: 14,
  },
  locationBadge: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
  },
  locationBadgeText: {
    fontSize: 12,
    fontWeight: '500',
  },
  exercisesSection: {
    marginBottom: 16,
  },
  noExercisesContainer: {
    paddingVertical: 40,
    alignItems: 'center',
  },
  noExercisesText: {
    fontSize: 14,
    textAlign: 'center',
  },
  manageExercisesButton: {
    height: 48,
    borderWidth: 1.5,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  manageExercisesButtonText: {
    fontSize: 16,
    fontWeight: '600',
  },
  completeButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  completeButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  discardButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: '#ef4444',
  },
  discardButtonText: {
    color: '#ef4444',
    fontSize: 16,
    fontWeight: '600',
  },
});
