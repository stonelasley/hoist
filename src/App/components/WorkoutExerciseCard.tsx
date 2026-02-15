import { useState } from 'react';
import { StyleSheet, Text, TouchableOpacity, View, Alert } from 'react-native';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { useApi } from '@/hooks/useApi';
import SetEntryRow from './SetEntryRow';
import {
  createWorkoutSet,
  updateWorkoutSet,
  deleteWorkoutSet,
} from '@/services/workouts';
import type {
  WorkoutExerciseDto,
  WorkoutSetDto,
  CreateWorkoutSetRequest,
  UpdateWorkoutSetRequest,
} from '@/services/workouts';

type Props = {
  exercise: WorkoutExerciseDto;
  workoutId: number;
  onSetChanged: () => void;
  defaultBodyweight?: number | null;
  weightUnit?: string;
  distanceUnit?: string;
};

export default function WorkoutExerciseCard({
  exercise,
  workoutId,
  onSetChanged,
  defaultBodyweight,
  weightUnit,
  distanceUnit,
}: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [isAddingSet, setIsAddingSet] = useState(false);

  const handleAddSet = async () => {
    setIsAddingSet(true);
    try {
      await createWorkoutSet(api, workoutId, exercise.id, {});
      onSetChanged();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to add set.';
      Alert.alert('Error', message);
    } finally {
      setIsAddingSet(false);
    }
  };

  const handleUpdateSet = async (
    setId: number,
    setData: UpdateWorkoutSetRequest,
  ) => {
    await updateWorkoutSet(api, workoutId, exercise.id, setId, setData);
    onSetChanged();
  };

  const handleDeleteSet = async (setId: number) => {
    await deleteWorkoutSet(api, workoutId, exercise.id, setId);
    onSetChanged();
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={styles.header}>
        <View style={styles.headerContent}>
          <Text style={[styles.exerciseName, { color: colors.text }]} numberOfLines={1}>
            {exercise.exerciseName}
          </Text>
          <View style={[styles.badge, { backgroundColor: colors.tint + '20' }]}>
            <Text style={[styles.badgeText, { color: colors.tint }]}>
              {exercise.implementType}
            </Text>
          </View>
        </View>
      </View>

      <View style={styles.setsContainer}>
        {exercise.sets.map((set) => (
          <SetEntryRow
            key={set.id}
            set={set}
            exerciseImplementType={exercise.implementType}
            exerciseType={exercise.exerciseType}
            onSave={(setData) => handleUpdateSet(set.id, setData)}
            onDelete={() => handleDeleteSet(set.id)}
            defaultBodyweight={defaultBodyweight}
            weightUnit={weightUnit}
            distanceUnit={distanceUnit}
          />
        ))}
      </View>

      <TouchableOpacity
        style={[
          styles.addSetButton,
          { borderColor: colors.tint },
          isAddingSet && styles.addingSetButton,
        ]}
        onPress={handleAddSet}
        disabled={isAddingSet}
      >
        <Text style={[styles.addSetButtonText, { color: colors.tint }]}>
          {isAddingSet ? 'Adding Set...' : '+ Add Set'}
        </Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  header: {
    marginBottom: 16,
  },
  headerContent: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  exerciseName: {
    fontSize: 18,
    fontWeight: '600',
    flex: 1,
  },
  badge: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgeText: {
    fontSize: 12,
    fontWeight: '600',
  },
  setsContainer: {
    marginBottom: 8,
  },
  addSetButton: {
    height: 40,
    borderWidth: 1.5,
    borderRadius: 8,
    borderStyle: 'dashed',
    justifyContent: 'center',
    alignItems: 'center',
  },
  addingSetButton: {
    opacity: 0.5,
  },
  addSetButtonText: {
    fontSize: 14,
    fontWeight: '600',
  },
});
