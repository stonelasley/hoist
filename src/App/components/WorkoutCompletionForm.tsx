import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  StyleSheet,
  useColorScheme,
  Pressable,
} from 'react-native';
import { Colors } from '@/constants/theme';
import { RatingPicker } from './RatingPicker';
import type { WorkoutDetailDto } from '@/services/workouts';

type WorkoutCompletionFormProps = {
  workout: WorkoutDetailDto;
  onConfirm: (data: {
    notes: string;
    rating: number | null;
    startedAt: string;
    endedAt: string;
  }) => void;
  onCancel: () => void;
};

export function WorkoutCompletionForm({
  workout,
  onConfirm,
  onCancel,
}: WorkoutCompletionFormProps) {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];

  const [notes, setNotes] = useState(workout.notes || '');
  const [rating, setRating] = useState<number | null>(workout.rating);
  const [startedAt, setStartedAt] = useState(workout.startedAt);
  const [endedAt, setEndedAt] = useState(
    workout.endedAt || new Date().toISOString(),
  );

  const handleConfirm = () => {
    onConfirm({ notes, rating, startedAt, endedAt });
  };

  const getDuration = () => {
    const start = new Date(startedAt);
    const end = new Date(endedAt);
    const durationMs = end.getTime() - start.getTime();
    const durationMinutes = Math.floor(durationMs / 60000);
    const hours = Math.floor(durationMinutes / 60);
    const minutes = durationMinutes % 60;
    return hours > 0 ? `${hours}h ${minutes}m` : `${minutes}m`;
  };

  return (
    <View style={styles.container}>
      <View style={styles.section}>
        <Text style={[styles.label, { color: theme.text }]}>Start Time</Text>
        <TextInput
          style={[
            styles.input,
            { color: theme.text, backgroundColor: theme.background },
          ]}
          value={new Date(startedAt).toLocaleString()}
          onChangeText={(text) => {
            try {
              const date = new Date(text);
              if (!isNaN(date.getTime())) {
                setStartedAt(date.toISOString());
              }
            } catch {
              // Ignore invalid dates
            }
          }}
        />
      </View>

      <View style={styles.section}>
        <Text style={[styles.label, { color: theme.text }]}>End Time</Text>
        <TextInput
          style={[
            styles.input,
            { color: theme.text, backgroundColor: theme.background },
          ]}
          value={new Date(endedAt).toLocaleString()}
          onChangeText={(text) => {
            try {
              const date = new Date(text);
              if (!isNaN(date.getTime())) {
                setEndedAt(date.toISOString());
              }
            } catch {
              // Ignore invalid dates
            }
          }}
        />
      </View>

      <View style={styles.section}>
        <Text style={[styles.label, { color: theme.text }]}>
          Duration: {getDuration()}
        </Text>
      </View>

      <View style={styles.section}>
        <Text style={[styles.label, { color: theme.text }]}>Notes</Text>
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
      </View>

      <View style={styles.section}>
        <Text style={[styles.label, { color: theme.text }]}>Rating</Text>
        <RatingPicker value={rating} onChange={setRating} />
      </View>

      <View style={styles.buttons}>
        <Pressable
          style={[styles.button, { backgroundColor: theme.tint }]}
          onPress={handleConfirm}
        >
          <Text style={[styles.buttonText, { color: '#fff' }]}>
            Complete Workout
          </Text>
        </Pressable>

        <Pressable
          style={[styles.button, { backgroundColor: theme.background }]}
          onPress={onCancel}
        >
          <Text style={[styles.buttonText, { color: theme.text }]}>Cancel</Text>
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    padding: 16,
    gap: 16,
  },
  section: {
    gap: 8,
  },
  label: {
    fontSize: 16,
    fontWeight: '600',
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
  buttons: {
    gap: 12,
    marginTop: 8,
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
});
