import { useEffect, useState } from 'react';
import {
  View,
  Text,
  ActivityIndicator,
  StyleSheet,
  useColorScheme,
  ScrollView,
} from 'react-native';
import { useLocalSearchParams, router } from 'expo-router';
import { Colors } from '@/constants/theme';
import { useApi } from '@/hooks/useApi';
import { WorkoutCompletionForm } from '@/components/WorkoutCompletionForm';
import {
  getWorkout,
  completeWorkout,
  type WorkoutDetailDto,
} from '@/services/workouts';

export default function CompleteWorkoutScreen() {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];
  const params = useLocalSearchParams<{ id: string }>();
  const api = useApi();

  const [workout, setWorkout] = useState<WorkoutDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadWorkout();
  }, []);

  const loadWorkout = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getWorkout(api, parseInt(params.id, 10));
      setWorkout(data);
    } catch (err) {
      setError('Failed to load workout');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleComplete = async (data: {
    notes: string;
    rating: number | null;
    startedAt: string;
    endedAt: string;
  }) => {
    if (!workout) return;

    try {
      await completeWorkout(api, workout.id, {
        id: workout.id,
        notes: data.notes,
        rating: data.rating ?? undefined,
        startedAt: data.startedAt,
        endedAt: data.endedAt,
      });

      router.replace('/');
    } catch (err) {
      setError('Failed to complete workout');
      console.error(err);
    }
  };

  const handleCancel = () => {
    router.back();
  };

  if (loading) {
    return (
      <View style={[styles.centered, { backgroundColor: theme.background }]}>
        <ActivityIndicator size="large" color={theme.tint} />
      </View>
    );
  }

  if (error || !workout) {
    return (
      <View style={[styles.centered, { backgroundColor: theme.background }]}>
        <Text style={[styles.errorText, { color: theme.text }]}>
          {error || 'Workout not found'}
        </Text>
      </View>
    );
  }

  return (
    <ScrollView
      style={[styles.container, { backgroundColor: theme.background }]}
    >
      <View style={styles.header}>
        <Text style={[styles.title, { color: theme.text }]}>
          Complete Workout
        </Text>
        <Text style={[styles.subtitle, { color: theme.icon }]}>
          {workout.templateName}
        </Text>
      </View>

      <WorkoutCompletionForm
        workout={workout}
        onConfirm={handleComplete}
        onCancel={handleCancel}
      />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  header: {
    padding: 16,
    gap: 4,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
  },
  subtitle: {
    fontSize: 16,
  },
  errorText: {
    fontSize: 16,
  },
});
