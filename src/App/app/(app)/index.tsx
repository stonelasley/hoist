import { useCallback, useMemo, useState } from 'react';
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
import { RecentWorkoutsList } from '@/components/RecentWorkoutsList';
import { InProgressWorkoutBanner } from '@/components/InProgressWorkoutBanner';
import { getWorkoutTemplates } from '@/services/workout-templates';
import { getExerciseTemplates } from '@/services/exercise-templates';
import { getLocations } from '@/services/locations';
import { getInProgressWorkout, getRecentWorkouts } from '@/services/workouts';
import type { WorkoutTemplateBriefDto } from '@/services/workout-templates';
import type { ExerciseTemplateBriefDto } from '@/services/exercise-templates';
import type { LocationDto } from '@/services/locations';
import type { WorkoutDetailDto, WorkoutBriefDto } from '@/services/workouts';

export default function LandingPage() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [workouts, setWorkouts] = useState<WorkoutTemplateBriefDto[]>([]);
  const [exercises, setExercises] = useState<ExerciseTemplateBriefDto[]>([]);
  const [locations, setLocations] = useState<LocationDto[]>([]);
  const [inProgressWorkout, setInProgressWorkout] = useState<WorkoutDetailDto | null>(null);
  const [recentWorkouts, setRecentWorkouts] = useState<WorkoutBriefDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [workoutLocationFilter, setWorkoutLocationFilter] = useState<number | null>(null);
  const [exerciseLocationFilter, setExerciseLocationFilter] = useState<number | null>(null);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      const results = await Promise.allSettled([
        getWorkoutTemplates(api, workoutLocationFilter ?? undefined),
        getExerciseTemplates(api),
        getLocations(api),
        getInProgressWorkout(api),
        getRecentWorkouts(api),
      ]);
      if (results[0].status === 'fulfilled') setWorkouts(results[0].value);
      if (results[1].status === 'fulfilled') setExercises(results[1].value);
      if (results[2].status === 'fulfilled') setLocations(results[2].value);
      if (results[3].status === 'fulfilled') setInProgressWorkout(results[3].value);
      if (results[4].status === 'fulfilled') setRecentWorkouts(results[4].value);

      const failures = results.filter((r) => r.status === 'rejected');
      if (failures.length > 0) {
        console.error('Some requests failed:', failures.map((f) => (f as PromiseRejectedResult).reason));
      }
    } catch (err) {
      console.error('Failed to load templates:', err);
    } finally {
      setIsLoading(false);
    }
  }, [api, workoutLocationFilter]);

  useFocusEffect(
    useCallback(() => {
      loadData();
    }, [loadData]),
  );

  const filteredExercises = useMemo(() => {
    if (exerciseLocationFilter === null) return exercises;
    return exercises.filter((e) => e.locationId === exerciseLocationFilter);
  }, [exercises, exerciseLocationFilter]);

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
        <TouchableOpacity
          style={styles.settingsButton}
          onPress={() => router.push('/(app)/settings')}
        >
          <Text style={[styles.settingsIcon, { color: colors.icon }]}>⚙</Text>
        </TouchableOpacity>
      </View>

      {inProgressWorkout && (
        <View style={[styles.section, styles.bannerSection]}>
          <InProgressWorkoutBanner
            workout={inProgressWorkout}
            onPress={() => router.push('/(app)/workouts/active')}
          />
        </View>
      )}

      {recentWorkouts.length > 0 && (
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={[styles.sectionTitle, { color: colors.text }]}>Recent Workouts</Text>
            <TouchableOpacity onPress={() => router.push('/(app)/workouts/history')}>
              <Text style={[styles.linkText, { color: colors.tint }]}>View All</Text>
            </TouchableOpacity>
          </View>
          <RecentWorkoutsList
            workouts={recentWorkouts}
            onPress={(id) => router.push(`/(app)/workouts/${id}`)}
          />
        </View>
      )}

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
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chipRow}>
          <TouchableOpacity
            style={[
              styles.chip,
              !workoutLocationFilter
                ? { backgroundColor: colors.tint }
                : { backgroundColor: colors.icon + '15' },
            ]}
            onPress={() => setWorkoutLocationFilter(null)}
          >
            <Text
              style={[
                styles.chipText,
                { color: !workoutLocationFilter ? '#fff' : colors.icon },
              ]}
            >
              All Locations
            </Text>
          </TouchableOpacity>
          {locations.map((loc) => (
            <TouchableOpacity
              key={loc.id}
              style={[
                styles.chip,
                workoutLocationFilter === loc.id
                  ? { backgroundColor: colors.tint }
                  : { backgroundColor: colors.icon + '15' },
              ]}
              onPress={() =>
                setWorkoutLocationFilter(workoutLocationFilter === loc.id ? null : loc.id)
              }
            >
              <Text
                style={[
                  styles.chipText,
                  { color: workoutLocationFilter === loc.id ? '#fff' : colors.icon },
                ]}
              >
                {loc.name}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
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
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chipRow}>
          <TouchableOpacity
            style={[
              styles.chip,
              !exerciseLocationFilter
                ? { backgroundColor: colors.tint }
                : { backgroundColor: colors.icon + '15' },
            ]}
            onPress={() => setExerciseLocationFilter(null)}
          >
            <Text
              style={[
                styles.chipText,
                { color: !exerciseLocationFilter ? '#fff' : colors.icon },
              ]}
            >
              All Locations
            </Text>
          </TouchableOpacity>
          {locations.map((loc) => (
            <TouchableOpacity
              key={loc.id}
              style={[
                styles.chip,
                exerciseLocationFilter === loc.id
                  ? { backgroundColor: colors.tint }
                  : { backgroundColor: colors.icon + '15' },
              ]}
              onPress={() =>
                setExerciseLocationFilter(exerciseLocationFilter === loc.id ? null : loc.id)
              }
            >
              <Text
                style={[
                  styles.chipText,
                  { color: exerciseLocationFilter === loc.id ? '#fff' : colors.icon },
                ]}
              >
                {loc.name}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
        <ExerciseTemplateList
          exercises={filteredExercises}
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
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingTop: 60,
    paddingBottom: 16,
  },
  headerTitle: {
    fontSize: 32,
    fontWeight: 'bold',
  },
  settingsButton: {
    width: 44,
    height: 44,
    justifyContent: 'center',
    alignItems: 'center',
  },
  settingsIcon: {
    fontSize: 24,
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
  chipRow: {
    flexDirection: 'row',
    marginBottom: 8,
  },
  chip: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 16,
    marginRight: 6,
  },
  chipText: {
    fontSize: 13,
    fontWeight: '500',
  },
  bannerSection: {
    marginBottom: 16,
  },
  linkText: {
    fontSize: 14,
    fontWeight: '600',
  },
});
