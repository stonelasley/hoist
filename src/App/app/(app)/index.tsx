import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
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
import WorkoutTemplateList from '@/components/WorkoutTemplateList';
import ExerciseTemplateList from '@/components/ExerciseTemplateList';
import { getWorkoutTemplates } from '@/services/workout-templates';
import { getExerciseTemplates } from '@/services/exercise-templates';
import type { WorkoutTemplateBriefDto } from '@/services/workout-templates';
import type { ExerciseTemplateBriefDto } from '@/services/exercise-templates';

export default function LandingPage() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [workouts, setWorkouts] = useState<WorkoutTemplateBriefDto[]>([]);
  const [exercises, setExercises] = useState<ExerciseTemplateBriefDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      const [workoutData, exerciseData] = await Promise.all([
        getWorkoutTemplates(api),
        getExerciseTemplates(api),
      ]);
      setWorkouts(workoutData);
      setExercises(exerciseData);
    } catch (err) {
      console.error('Failed to load templates:', err);
    } finally {
      setIsLoading(false);
    }
  }, [api]);

  useFocusEffect(
    useCallback(() => {
      loadData();
    }, [loadData]),
  );

  if (isLoading) {
    return (
      <View style={[styles.loadingContainer, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.tint} />
      </View>
    );
  }

  return (
    <ScrollView
      style={[styles.container, { backgroundColor: colors.background }]}
      contentContainerStyle={styles.scrollContent}
    >
      <View style={styles.header}>
        <Text style={[styles.headerTitle, { color: colors.text }]}>Hoist</Text>
      </View>

      <View style={styles.section}>
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>My Workouts</Text>
          <TouchableOpacity
            style={[styles.addButton, { backgroundColor: colors.tint }]}
            onPress={() => router.push('/(app)/workout-templates/create')}
          >
            <Text style={styles.addButtonText}>+</Text>
          </TouchableOpacity>
        </View>
        <WorkoutTemplateList
          templates={workouts}
          onPress={(id) => router.push(`/(app)/workout-templates/${id}`)}
          onCreatePress={() => router.push('/(app)/workout-templates/create')}
        />
      </View>

      <View style={styles.section}>
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: colors.text }]}>My Exercises</Text>
          <TouchableOpacity
            style={[styles.addButton, { backgroundColor: colors.tint }]}
            onPress={() => router.push('/(app)/exercise-templates/create')}
          >
            <Text style={styles.addButtonText}>+</Text>
          </TouchableOpacity>
        </View>
        <ExerciseTemplateList
          exercises={exercises}
          onPress={(id) => router.push(`/(app)/exercise-templates/${id}`)}
          onCreatePress={() => router.push('/(app)/exercise-templates/create')}
        />
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
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
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
    marginBottom: 24,
  },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  sectionTitle: {
    fontSize: 20,
    fontWeight: '700',
  },
  addButton: {
    width: 32,
    height: 32,
    borderRadius: 16,
    justifyContent: 'center',
    alignItems: 'center',
  },
  addButtonText: {
    color: '#fff',
    fontSize: 20,
    fontWeight: '600',
    lineHeight: 22,
  },
});
