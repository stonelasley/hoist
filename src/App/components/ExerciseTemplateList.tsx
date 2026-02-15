import { useState, useMemo } from 'react';
import {
  FlatList,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { IMPLEMENT_TYPES, EXERCISE_TYPES } from '@/services/exercise-templates';
import type { ExerciseTemplateBriefDto } from '@/services/exercise-templates';

type Props = {
  exercises: ExerciseTemplateBriefDto[];
  onPress: (id: number) => void;
  onCreatePress: () => void;
  showSearch?: boolean;
  showFilters?: boolean;
};

function ExerciseTemplateItem({
  item,
  onPress,
  colors,
}: {
  item: ExerciseTemplateBriefDto;
  onPress: (id: number) => void;
  colors: (typeof Colors)['light'];
}) {
  return (
    <TouchableOpacity
      style={[styles.card, { backgroundColor: colors.background, borderColor: colors.icon + '30' }]}
      onPress={() => onPress(item.id)}
      activeOpacity={0.7}
    >
      <View style={styles.cardContent}>
        <Text style={[styles.cardTitle, { color: colors.text }]} numberOfLines={1}>
          {item.name}
        </Text>
        <View style={styles.tagRow}>
          <View style={[styles.tag, { backgroundColor: colors.tint + '15' }]}>
            <Text style={[styles.tagText, { color: colors.tint }]}>{item.implementType}</Text>
          </View>
          <View style={[styles.tag, { backgroundColor: colors.icon + '15' }]}>
            <Text style={[styles.tagText, { color: colors.icon }]}>{item.exerciseType}</Text>
          </View>
        </View>
      </View>
    </TouchableOpacity>
  );
}

export default function ExerciseTemplateList({
  exercises,
  onPress,
  onCreatePress,
  showSearch = false,
  showFilters = false,
}: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  const [searchText, setSearchText] = useState('');
  const [selectedImplementType, setSelectedImplementType] = useState<string | null>(null);
  const [selectedExerciseType, setSelectedExerciseType] = useState<string | null>(null);

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

  const renderFilters = () => {
    if (!showFilters) return null;

    return (
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
              style={[
                styles.chipText,
                { color: !selectedImplementType ? '#fff' : colors.icon },
              ]}
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
              style={[
                styles.chipText,
                { color: !selectedExerciseType ? '#fff' : colors.icon },
              ]}
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
    );
  };

  if (exercises.length === 0 && !showSearch && !showFilters) {
    return (
      <View style={styles.emptyContainer}>
        <Text style={[styles.emptyTitle, { color: colors.text }]}>No Exercises Yet</Text>
        <Text style={[styles.emptySubtitle, { color: colors.icon }]}>
          Create your first exercise template to get started.
        </Text>
        <TouchableOpacity
          style={[styles.createButton, { backgroundColor: colors.tint }]}
          onPress={onCreatePress}
        >
          <Text style={styles.createButtonText}>Create Exercise</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View>
      {showSearch && (
        <TextInput
          style={[styles.searchInput, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="Search exercises..."
          placeholderTextColor={colors.icon}
          value={searchText}
          onChangeText={setSearchText}
          autoCapitalize="none"
          autoCorrect={false}
        />
      )}
      {renderFilters()}
      <FlatList
        data={filteredExercises}
        keyExtractor={(item) => item.id.toString()}
        renderItem={({ item }) => (
          <ExerciseTemplateItem item={item} onPress={onPress} colors={colors} />
        )}
        scrollEnabled={false}
        contentContainerStyle={styles.listContent}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={[styles.emptySubtitle, { color: colors.icon }]}>
              {searchText || selectedImplementType || selectedExerciseType
                ? 'No exercises match your filters.'
                : 'No exercises yet.'}
            </Text>
          </View>
        }
      />
    </View>
  );
}

const styles = StyleSheet.create({
  searchInput: {
    height: 44,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 14,
    fontSize: 15,
    marginBottom: 10,
  },
  filtersSection: {
    marginBottom: 10,
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
  listContent: {
    gap: 8,
  },
  card: {
    borderWidth: 1,
    borderRadius: 10,
    padding: 14,
  },
  cardContent: {
    gap: 6,
  },
  cardTitle: {
    fontSize: 16,
    fontWeight: '600',
  },
  tagRow: {
    flexDirection: 'row',
    gap: 6,
  },
  tag: {
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 6,
  },
  tagText: {
    fontSize: 12,
    fontWeight: '500',
  },
  emptyContainer: {
    alignItems: 'center',
    paddingVertical: 32,
    paddingHorizontal: 24,
  },
  emptyTitle: {
    fontSize: 18,
    fontWeight: '600',
    marginBottom: 8,
  },
  emptySubtitle: {
    fontSize: 14,
    textAlign: 'center',
    marginBottom: 20,
  },
  createButton: {
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
  },
  createButtonText: {
    color: '#fff',
    fontSize: 15,
    fontWeight: '600',
  },
});
