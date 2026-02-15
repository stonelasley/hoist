import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Modal,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import {
  getExerciseTemplates,
  IMPLEMENT_TYPES,
  EXERCISE_TYPES,
} from '@/services/exercise-templates';
import type { ExerciseTemplateBriefDto } from '@/services/exercise-templates';

type Props = {
  visible: boolean;
  onClose: () => void;
  onSelect: (exerciseId: number) => void;
  onCreateNew: () => void;
};

export default function ExercisePickerModal({
  visible,
  onClose,
  onSelect,
  onCreateNew,
}: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [exercises, setExercises] = useState<ExerciseTemplateBriefDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedImplementType, setSelectedImplementType] = useState<string | null>(null);
  const [selectedExerciseType, setSelectedExerciseType] = useState<string | null>(null);

  const loadExercises = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await getExerciseTemplates(api);
      setExercises(data);
    } catch (err) {
      console.error('Failed to load exercises:', err);
    } finally {
      setIsLoading(false);
    }
  }, [api]);

  useEffect(() => {
    if (visible) {
      loadExercises();
      setSearchText('');
      setSelectedImplementType(null);
      setSelectedExerciseType(null);
    }
  }, [visible, loadExercises]);

  const filteredExercises = useMemo(() => {
    let result = exercises;

    if (searchText.trim()) {
      const query = searchText.trim().toLowerCase();
      result = result.filter((e) => e.name.toLowerCase().includes(query));
    }

    if (selectedImplementType) {
      result = result.filter((e) => e.implementType === selectedImplementType);
    }

    if (selectedExerciseType) {
      result = result.filter((e) => e.exerciseType === selectedExerciseType);
    }

    return result;
  }, [exercises, searchText, selectedImplementType, selectedExerciseType]);

  return (
    <Modal
      visible={visible}
      animationType="slide"
      presentationStyle="pageSheet"
      onRequestClose={onClose}
    >
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.header}>
          <TouchableOpacity onPress={onClose}>
            <Text style={[styles.headerAction, { color: colors.tint }]}>Cancel</Text>
          </TouchableOpacity>
          <Text style={[styles.headerTitle, { color: colors.text }]}>Add Exercise</Text>
          <View style={styles.headerSpacer} />
        </View>

        <View style={styles.searchSection}>
          <TextInput
            style={[styles.searchInput, { color: colors.text, borderColor: colors.icon + '40' }]}
            placeholder="Search exercises..."
            placeholderTextColor={colors.icon}
            value={searchText}
            onChangeText={setSearchText}
            autoCapitalize="none"
            autoCorrect={false}
          />
        </View>

        <View style={styles.filtersSection}>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chipRow}>
            <TouchableOpacity
              style={[
                styles.chip,
                !selectedImplementType
                  ? { backgroundColor: colors.tint }
                  : { backgroundColor: colors.icon + '15' },
              ]}
              onPress={() => setSelectedImplementType(null)}
            >
              <Text
                style={[styles.chipText, { color: !selectedImplementType ? '#fff' : colors.icon }]}
              >
                All Equipment
              </Text>
            </TouchableOpacity>
            {IMPLEMENT_TYPES.map((type) => (
              <TouchableOpacity
                key={type}
                style={[
                  styles.chip,
                  selectedImplementType === type
                    ? { backgroundColor: colors.tint }
                    : { backgroundColor: colors.icon + '15' },
                ]}
                onPress={() =>
                  setSelectedImplementType(selectedImplementType === type ? null : type)
                }
              >
                <Text
                  style={[
                    styles.chipText,
                    { color: selectedImplementType === type ? '#fff' : colors.icon },
                  ]}
                >
                  {type}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chipRow}>
            <TouchableOpacity
              style={[
                styles.chip,
                !selectedExerciseType
                  ? { backgroundColor: colors.tint }
                  : { backgroundColor: colors.icon + '15' },
              ]}
              onPress={() => setSelectedExerciseType(null)}
            >
              <Text
                style={[styles.chipText, { color: !selectedExerciseType ? '#fff' : colors.icon }]}
              >
                All Types
              </Text>
            </TouchableOpacity>
            {EXERCISE_TYPES.map((type) => (
              <TouchableOpacity
                key={type}
                style={[
                  styles.chip,
                  selectedExerciseType === type
                    ? { backgroundColor: colors.tint }
                    : { backgroundColor: colors.icon + '15' },
                ]}
                onPress={() =>
                  setSelectedExerciseType(selectedExerciseType === type ? null : type)
                }
              >
                <Text
                  style={[
                    styles.chipText,
                    { color: selectedExerciseType === type ? '#fff' : colors.icon },
                  ]}
                >
                  {type}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>

        {isLoading ? (
          <View style={styles.loadingContainer}>
            <ActivityIndicator size="large" color={colors.tint} />
          </View>
        ) : (
          <FlatList
            data={filteredExercises}
            keyExtractor={(item) => item.id.toString()}
            renderItem={({ item }) => (
              <TouchableOpacity
                style={[styles.exerciseRow, { borderColor: colors.icon + '20' }]}
                onPress={() => onSelect(item.id)}
                activeOpacity={0.7}
              >
                <View style={styles.exerciseInfo}>
                  <Text style={[styles.exerciseName, { color: colors.text }]} numberOfLines={1}>
                    {item.name}
                  </Text>
                  <Text style={[styles.exerciseMeta, { color: colors.icon }]}>
                    {item.implementType} - {item.exerciseType}
                  </Text>
                </View>
              </TouchableOpacity>
            )}
            contentContainerStyle={styles.listContent}
            ListEmptyComponent={
              <View style={styles.emptyContainer}>
                <Text style={[styles.emptyText, { color: colors.icon }]}>
                  {searchText || selectedImplementType || selectedExerciseType
                    ? 'No exercises match your filters.'
                    : 'No exercises available.'}
                </Text>
              </View>
            }
          />
        )}

        <TouchableOpacity
          style={[styles.createNewButton, { backgroundColor: colors.tint }]}
          onPress={onCreateNew}
        >
          <Text style={styles.createNewButtonText}>Create New Exercise</Text>
        </TouchableOpacity>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingTop: 16,
    paddingBottom: 12,
  },
  headerAction: {
    fontSize: 16,
    fontWeight: '500',
  },
  headerTitle: {
    fontSize: 17,
    fontWeight: '600',
  },
  headerSpacer: {
    width: 50,
  },
  searchSection: {
    paddingHorizontal: 16,
    marginBottom: 8,
  },
  searchInput: {
    height: 44,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 14,
    fontSize: 15,
  },
  filtersSection: {
    paddingLeft: 16,
    marginBottom: 8,
  },
  chipRow: {
    flexDirection: 'row',
    marginBottom: 6,
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
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  listContent: {
    paddingHorizontal: 16,
  },
  exerciseRow: {
    paddingVertical: 14,
    borderBottomWidth: 1,
  },
  exerciseInfo: {
    gap: 2,
  },
  exerciseName: {
    fontSize: 16,
    fontWeight: '500',
  },
  exerciseMeta: {
    fontSize: 13,
  },
  emptyContainer: {
    alignItems: 'center',
    paddingVertical: 40,
  },
  emptyText: {
    fontSize: 14,
  },
  createNewButton: {
    height: 48,
    marginHorizontal: 16,
    marginBottom: 32,
    marginTop: 8,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
  },
  createNewButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
