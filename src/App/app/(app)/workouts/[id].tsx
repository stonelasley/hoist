import { useEffect, useState } from 'react';
import {
  View,
  Text,
  ActivityIndicator,
  StyleSheet,
  useColorScheme,
  ScrollView,
  Pressable,
  TextInput,
} from 'react-native';
import { useLocalSearchParams, router } from 'expo-router';
import { Colors } from '@/constants/theme';
import { useApi } from '@/hooks/useApi';
import { RatingPicker } from '@/components/RatingPicker';
import {
  getWorkout,
  updateWorkout,
  type WorkoutDetailDto,
} from '@/services/workouts';

export default function WorkoutDetailScreen() {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];
  const params = useLocalSearchParams<{ id: string }>();
  const api = useApi();

  const [workout, setWorkout] = useState<WorkoutDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState(false);

  const [notes, setNotes] = useState('');
  const [rating, setRating] = useState<number | null>(null);

  useEffect(() => {
    loadWorkout();
  }, []);

  const loadWorkout = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getWorkout(api, parseInt(params.id, 10));

      // Redirect if workout is still in progress
      if (data.status === 'InProgress') {
        router.replace('/');
        return;
      }

      setWorkout(data);
      setNotes(data.notes || '');
      setRating(data.rating);
    } catch (err) {
      setError('Failed to load workout');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    if (!workout) return;

    try {
      await updateWorkout(api, workout.id, {
        id: workout.id,
        notes,
        rating: rating ?? undefined,
      });

      setEditing(false);
      loadWorkout();
    } catch (err) {
      setError('Failed to update workout');
      console.error(err);
    }
  };

  const getDuration = () => {
    if (!workout || !workout.endedAt) return '';
    const start = new Date(workout.startedAt);
    const end = new Date(workout.endedAt);
    const durationMs = end.getTime() - start.getTime();
    const durationMinutes = Math.floor(durationMs / 60000);
    const hours = Math.floor(durationMinutes / 60);
    const minutes = durationMinutes % 60;
    return hours > 0 ? `${hours}h ${minutes}m` : `${minutes}m`;
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
          {workout.templateName}
        </Text>
        <Text style={[styles.subtitle, { color: theme.icon }]}>
          {new Date(workout.startedAt).toLocaleDateString()}
        </Text>
        {workout.locationName && (
          <Text style={[styles.subtitle, { color: theme.icon }]}>
            {workout.locationName}
          </Text>
        )}
      </View>

      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: theme.text }]}>
          Duration
        </Text>
        <Text style={[styles.value, { color: theme.icon }]}>
          {getDuration()}
        </Text>
      </View>

      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: theme.text }]}>Rating</Text>
        {editing ? (
          <RatingPicker value={rating} onChange={setRating} />
        ) : (
          <View style={styles.ratingDisplay}>
            {rating ? (
              <Text style={[styles.value, { color: theme.icon }]}>
                {'★'.repeat(rating)}
                {'☆'.repeat(5 - rating)}
              </Text>
            ) : (
              <Text style={[styles.value, { color: theme.icon }]}>
                Not rated
              </Text>
            )}
          </View>
        )}
      </View>

      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: theme.text }]}>Notes</Text>
        {editing ? (
          <TextInput
            style={[
              styles.input,
              styles.multiline,
              { color: theme.text, backgroundColor: theme.background },
            ]}
            value={notes}
            onChangeText={setNotes}
            multiline
            placeholder="How did it go?"
            placeholderTextColor={theme.icon}
            maxLength={2000}
          />
        ) : (
          <Text style={[styles.value, { color: theme.icon }]}>
            {notes || 'No notes'}
          </Text>
        )}
      </View>

      <View style={styles.section}>
        <Text style={[styles.sectionTitle, { color: theme.text }]}>
          Exercises
        </Text>
        {workout.exercises.map((exercise) => (
          <View key={exercise.id} style={styles.exercise}>
            <Text style={[styles.exerciseName, { color: theme.text }]}>
              {exercise.exerciseName}
            </Text>
            {exercise.sets.map((set, idx) => (
              <Text key={set.id} style={[styles.set, { color: theme.icon }]}>
                Set {idx + 1}:{' '}
                {set.weight && set.reps
                  ? `${set.weight} ${set.weightUnit || 'kg'} × ${set.reps}`
                  : set.duration
                    ? `${Math.floor(set.duration / 60)}:${String(set.duration % 60).padStart(2, '0')}`
                    : 'No data'}
              </Text>
            ))}
          </View>
        ))}
      </View>

      <View style={styles.buttons}>
        {editing ? (
          <>
            <Pressable
              style={[styles.button, { backgroundColor: theme.tint }]}
              onPress={handleSave}
            >
              <Text style={[styles.buttonText, { color: '#fff' }]}>Save</Text>
            </Pressable>
            <Pressable
              style={[styles.button, { backgroundColor: theme.background }]}
              onPress={() => {
                setEditing(false);
                setNotes(workout.notes || '');
                setRating(workout.rating);
              }}
            >
              <Text style={[styles.buttonText, { color: theme.text }]}>
                Cancel
              </Text>
            </Pressable>
          </>
        ) : (
          <Pressable
            style={[styles.button, { backgroundColor: theme.tint }]}
            onPress={() => setEditing(true)}
          >
            <Text style={[styles.buttonText, { color: '#fff' }]}>Edit</Text>
          </Pressable>
        )}
      </View>
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
  section: {
    padding: 16,
    gap: 8,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: '600',
  },
  value: {
    fontSize: 16,
  },
  ratingDisplay: {
    flexDirection: 'row',
  },
  input: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    padding: 12,
    fontSize: 16,
  },
  multiline: {
    height: 100,
    textAlignVertical: 'top',
  },
  exercise: {
    paddingVertical: 12,
    gap: 4,
  },
  exerciseName: {
    fontSize: 16,
    fontWeight: '600',
  },
  set: {
    fontSize: 14,
    paddingLeft: 16,
  },
  buttons: {
    padding: 16,
    gap: 12,
  },
  button: {
    padding: 16,
    borderRadius: 8,
    alignItems: 'center',
  },
  buttonText: {
    fontSize: 16,
    fontWeight: '600',
  },
  errorText: {
    fontSize: 16,
  },
});
