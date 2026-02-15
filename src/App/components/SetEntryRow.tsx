import { useState } from 'react';
import {
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
  Alert,
} from 'react-native';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import BandColorPicker from './BandColorPicker';
import DurationPicker from './DurationPicker';
import type { WorkoutSetDto, UpdateWorkoutSetRequest } from '@/services/workouts';

type Props = {
  set: WorkoutSetDto;
  exerciseImplementType: string;
  exerciseType: string;
  onSave: (setData: UpdateWorkoutSetRequest) => Promise<void>;
  onDelete: () => Promise<void>;
  defaultBodyweight?: number | null;
  weightUnit?: string;
  distanceUnit?: string;
};

const WEIGHT_LABEL: Record<string, string> = { Lbs: 'lbs', Kg: 'kg' };
const DISTANCE_LABEL: Record<string, string> = {
  Miles: 'mi',
  Kilometers: 'km',
  Meters: 'm',
  Yards: 'yd',
};

export default function SetEntryRow({
  set,
  exerciseImplementType,
  exerciseType,
  onSave,
  onDelete,
  defaultBodyweight,
  weightUnit = 'Lbs',
  distanceUnit = 'Miles',
}: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  const [weight, setWeight] = useState(set.weight?.toString() ?? '');
  const [reps, setReps] = useState(set.reps?.toString() ?? '');
  const [bodyweight, setBodyweight] = useState(
    set.bodyweight?.toString() ?? (defaultBodyweight?.toString() ?? '')
  );
  const [bandColor, setBandColor] = useState(set.bandColor ?? '');
  const [duration, setDuration] = useState(set.duration ?? null);
  const [distance, setDistance] = useState(set.distance?.toString() ?? '');
  const [isSaving, setIsSaving] = useState(false);

  const isWeightImplement = [
    'Barbell',
    'Dumbbell',
    'SelectorizedMachine',
    'PlateLoadedMachine',
    'Kettlebell',
    'Plate',
    'MedicineBall',
  ].includes(exerciseImplementType);

  const isBodyweight = exerciseImplementType === 'Bodyweight';
  const isBand = exerciseImplementType === 'Band';
  const isReps = exerciseType === 'Reps';
  const isDuration = exerciseType === 'Duration';
  const isDistance = exerciseType === 'Distance';

  const handleSave = async () => {
    setIsSaving(true);
    try {
      const setData: UpdateWorkoutSetRequest = {};

      if (isWeightImplement) {
        if (weight) setData.weight = parseFloat(weight);
        setData.weightUnit = weightUnit;
      }

      if (isBodyweight && bodyweight) {
        setData.bodyweight = parseFloat(bodyweight);
      }

      if (isBand && bandColor) {
        setData.bandColor = bandColor;
      }

      if (isReps && reps) {
        setData.reps = parseInt(reps, 10);
      }

      if (isDuration && duration) {
        setData.duration = duration;
      }

      if (isDistance) {
        if (distance) setData.distance = parseFloat(distance);
        if (duration) setData.duration = duration;
        setData.distanceUnit = distanceUnit;
      }

      await onSave(setData);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to save set.';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = () => {
    Alert.alert('Delete Set', 'Are you sure you want to delete this set?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: async () => {
          try {
            await onDelete();
          } catch (err) {
            const message = err instanceof Error ? err.message : 'Failed to delete set.';
            Alert.alert('Error', message);
          }
        },
      },
    ]);
  };

  return (
    <View style={[styles.container, { borderColor: colors.icon + '20' }]}>
      <View style={styles.header}>
        <Text style={[styles.setNumber, { color: colors.icon }]}>Set {set.position}</Text>
        <TouchableOpacity onPress={handleDelete} style={styles.deleteButton}>
          <Text style={styles.deleteButtonText}>Delete</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.inputRow}>
        {isWeightImplement && (
          <>
            <View style={styles.inputGroup}>
              <Text style={[styles.inputLabel, { color: colors.icon }]}>Weight ({WEIGHT_LABEL[weightUnit] ?? 'lbs'})</Text>
              <TextInput
                style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
                placeholder="0"
                placeholderTextColor={colors.icon}
                value={weight}
                onChangeText={setWeight}
                keyboardType="decimal-pad"
              />
            </View>
            {isReps && (
              <View style={styles.inputGroup}>
                <Text style={[styles.inputLabel, { color: colors.icon }]}>Reps</Text>
                <TextInput
                  style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
                  placeholder="0"
                  placeholderTextColor={colors.icon}
                  value={reps}
                  onChangeText={setReps}
                  keyboardType="number-pad"
                />
              </View>
            )}
            {isDuration && (
              <View style={styles.inputGroup}>
                <Text style={[styles.inputLabel, { color: colors.icon }]}>Duration</Text>
                <DurationPicker value={duration} onChange={setDuration} />
              </View>
            )}
            {isDistance && (
              <>
                <View style={styles.inputGroup}>
                  <Text style={[styles.inputLabel, { color: colors.icon }]}>Distance ({DISTANCE_LABEL[distanceUnit] ?? 'mi'})</Text>
                  <TextInput
                    style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
                    placeholder="0"
                    placeholderTextColor={colors.icon}
                    value={distance}
                    onChangeText={setDistance}
                    keyboardType="decimal-pad"
                  />
                </View>
                <View style={styles.inputGroup}>
                  <Text style={[styles.inputLabel, { color: colors.icon }]}>Duration</Text>
                  <DurationPicker value={duration} onChange={setDuration} />
                </View>
              </>
            )}
          </>
        )}

        {isBodyweight && (
          <>
            <View style={styles.inputGroup}>
              <Text style={[styles.inputLabel, { color: colors.icon }]}>Bodyweight ({WEIGHT_LABEL[weightUnit] ?? 'lbs'})</Text>
              <TextInput
                style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
                placeholder="0"
                placeholderTextColor={colors.icon}
                value={bodyweight}
                onChangeText={setBodyweight}
                keyboardType="decimal-pad"
              />
            </View>
            {isReps && (
              <View style={styles.inputGroup}>
                <Text style={[styles.inputLabel, { color: colors.icon }]}>Reps</Text>
                <TextInput
                  style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
                  placeholder="0"
                  placeholderTextColor={colors.icon}
                  value={reps}
                  onChangeText={setReps}
                  keyboardType="number-pad"
                />
              </View>
            )}
            {isDuration && (
              <View style={styles.inputGroup}>
                <Text style={[styles.inputLabel, { color: colors.icon }]}>Duration</Text>
                <DurationPicker value={duration} onChange={setDuration} />
              </View>
            )}
          </>
        )}

        {isBand && (
          <View style={styles.bandSection}>
            <Text style={[styles.inputLabel, { color: colors.icon }]}>Band Color</Text>
            <BandColorPicker value={bandColor} onChange={setBandColor} />
            {isReps && (
              <View style={[styles.inputGroup, styles.bandRepsInput]}>
                <Text style={[styles.inputLabel, { color: colors.icon }]}>Reps</Text>
                <TextInput
                  style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
                  placeholder="0"
                  placeholderTextColor={colors.icon}
                  value={reps}
                  onChangeText={setReps}
                  keyboardType="number-pad"
                />
              </View>
            )}
            {isDuration && (
              <View style={[styles.inputGroup, styles.bandRepsInput]}>
                <Text style={[styles.inputLabel, { color: colors.icon }]}>Duration</Text>
                <DurationPicker value={duration} onChange={setDuration} />
              </View>
            )}
          </View>
        )}
      </View>

      <TouchableOpacity
        style={[
          styles.saveButton,
          { backgroundColor: colors.tint },
          isSaving && styles.savingButton,
        ]}
        onPress={handleSave}
        disabled={isSaving}
      >
        <Text style={styles.saveButtonText}>{isSaving ? 'Saving...' : 'Save'}</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    borderWidth: 1,
    borderRadius: 8,
    padding: 12,
    marginBottom: 8,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  setNumber: {
    fontSize: 14,
    fontWeight: '600',
  },
  deleteButton: {
    paddingHorizontal: 8,
    paddingVertical: 4,
  },
  deleteButtonText: {
    color: '#ef4444',
    fontSize: 13,
    fontWeight: '500',
  },
  inputRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
    marginBottom: 12,
  },
  inputGroup: {
    flex: 1,
    minWidth: 100,
  },
  inputLabel: {
    fontSize: 12,
    fontWeight: '500',
    marginBottom: 4,
  },
  input: {
    height: 40,
    borderWidth: 1,
    borderRadius: 6,
    paddingHorizontal: 12,
    fontSize: 15,
  },
  bandSection: {
    width: '100%',
  },
  bandRepsInput: {
    marginTop: 8,
    maxWidth: 120,
  },
  saveButton: {
    height: 36,
    borderRadius: 6,
    justifyContent: 'center',
    alignItems: 'center',
  },
  savingButton: {
    opacity: 0.6,
  },
  saveButtonText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
});
