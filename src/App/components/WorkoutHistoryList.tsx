import React, { useState, useCallback } from 'react';
import {
  View,
  FlatList,
  Text,
  TouchableOpacity,
  TextInput,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { Colors } from '@/constants/theme';
import { WorkoutBriefDto, PaginatedWorkoutList } from '@/services/workouts';
import LocationPickerModal from '@/components/LocationPickerModal';

type SortOption = 'date' | 'rating';
type SortDirection = 'asc' | 'desc';

type Props = {
  onPressWorkout: (id: number) => void;
  fetchHistory: (params: {
    sortBy?: string;
    sortDirection?: string;
    locationId?: number;
    minRating?: number;
    search?: string;
    cursor?: string;
    pageSize?: number;
  }) => Promise<PaginatedWorkoutList>;
};

export function WorkoutHistoryList({ onPressWorkout, fetchHistory }: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const [workouts, setWorkouts] = useState<WorkoutBriefDto[]>([]);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [isInitialLoad, setIsInitialLoad] = useState(true);

  // Filter/sort state
  const [sortBy, setSortBy] = useState<SortOption>('date');
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc');
  const [selectedLocationId, setSelectedLocationId] = useState<number | undefined>(undefined);
  const [minRating, setMinRating] = useState<number | undefined>(undefined);
  const [searchText, setSearchText] = useState('');
  const [isLocationModalVisible, setIsLocationModalVisible] = useState(false);

  const loadWorkouts = useCallback(
    async (cursor?: string, append = false) => {
      if (append) {
        setIsLoadingMore(true);
      } else {
        setIsLoading(true);
      }

      try {
        const result = await fetchHistory({
          sortBy,
          sortDirection,
          locationId: selectedLocationId,
          minRating,
          search: searchText || undefined,
          cursor,
          pageSize: 20,
        });

        if (append) {
          setWorkouts((prev) => [...prev, ...result.items]);
        } else {
          setWorkouts(result.items);
        }
        setNextCursor(result.nextCursor);
      } catch (error) {
        console.error('Failed to load workout history:', error);
      } finally {
        setIsLoading(false);
        setIsLoadingMore(false);
        setIsInitialLoad(false);
      }
    },
    [sortBy, sortDirection, selectedLocationId, minRating, searchText, fetchHistory],
  );

  // Load initial data
  React.useEffect(() => {
    loadWorkouts();
  }, [loadWorkouts]);

  const handleLoadMore = () => {
    if (nextCursor && !isLoadingMore) {
      loadWorkouts(nextCursor, true);
    }
  };

  const handleRefresh = () => {
    loadWorkouts();
  };

  const toggleSortBy = () => {
    setSortBy((prev) => (prev === 'date' ? 'rating' : 'date'));
  };

  const toggleSortDirection = () => {
    setSortDirection((prev) => (prev === 'desc' ? 'asc' : 'desc'));
  };

  const handleLocationSelect = (locationId: number | null, locationName: string | null) => {
    setSelectedLocationId(locationId ?? undefined);
    setIsLocationModalVisible(false);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const formatDuration = (startedAt: string, endedAt: string | null) => {
    if (!endedAt) return '';
    const start = new Date(startedAt);
    const end = new Date(endedAt);
    const durationMs = end.getTime() - start.getTime();
    const minutes = Math.floor(durationMs / 60000);
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
  };

  const renderWorkoutCard = ({ item }: { item: WorkoutBriefDto }) => (
    <TouchableOpacity
      style={[styles.card, { backgroundColor: colors.background }]}
      onPress={() => onPressWorkout(item.id)}
    >
      <View style={styles.cardHeader}>
        <Text style={[styles.templateName, { color: colors.text }]}>
          {item.templateName}
        </Text>
        {item.rating && (
          <View style={styles.ratingContainer}>
            <Text style={[styles.rating, { color: colors.tint }]}>
              {'★'.repeat(item.rating)}
            </Text>
          </View>
        )}
      </View>
      <View style={styles.cardDetails}>
        <Text style={[styles.detailText, { color: colors.icon }]}>
          {item.endedAt ? formatDate(item.endedAt) : 'In Progress'}
        </Text>
        {item.endedAt && (
          <Text style={[styles.detailText, { color: colors.icon }]}>
            {formatDuration(item.startedAt, item.endedAt)}
          </Text>
        )}
        {item.locationName && (
          <Text style={[styles.detailText, { color: colors.icon }]}>
            {item.locationName}
          </Text>
        )}
      </View>
    </TouchableOpacity>
  );

  const renderHeader = () => (
    <View style={styles.header}>
      {/* Search */}
      <TextInput
        style={[styles.searchInput, { backgroundColor: colors.background, color: colors.text }]}
        placeholder="Search notes..."
        placeholderTextColor={colors.icon}
        value={searchText}
        onChangeText={setSearchText}
      />

      {/* Sort controls */}
      <View style={styles.controlRow}>
        <TouchableOpacity
          style={[styles.button, { backgroundColor: colors.tint }]}
          onPress={toggleSortBy}
        >
          <Text style={[styles.buttonText, { color: colors.background }]}>
            Sort: {sortBy === 'date' ? 'Date' : 'Rating'}
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.button, { backgroundColor: colors.tint }]}
          onPress={toggleSortDirection}
        >
          <Text style={[styles.buttonText, { color: colors.background }]}>
            {sortDirection === 'desc' ? '↓' : '↑'}
          </Text>
        </TouchableOpacity>
      </View>

      {/* Filter chips */}
      <View style={styles.filterRow}>
        <TouchableOpacity
          style={[styles.filterChip, selectedLocationId ? { backgroundColor: colors.tint } : { backgroundColor: colors.background }]}
          onPress={() => setIsLocationModalVisible(true)}
        >
          <Text style={[styles.filterChipText, selectedLocationId ? { color: colors.background } : { color: colors.text }]}>
            {selectedLocationId ? 'Location: Selected' : 'Location: All'}
          </Text>
        </TouchableOpacity>

        {selectedLocationId && (
          <TouchableOpacity
            style={[styles.clearButton, { backgroundColor: colors.background }]}
            onPress={() => setSelectedLocationId(undefined)}
          >
            <Text style={[styles.clearButtonText, { color: colors.text }]}>✕</Text>
          </TouchableOpacity>
        )}
      </View>

      {/* Rating filter */}
      <View style={styles.ratingFilterRow}>
        <Text style={[styles.filterLabel, { color: colors.text }]}>Min Rating:</Text>
        {[1, 2, 3, 4, 5].map((rating) => (
          <TouchableOpacity
            key={rating}
            style={[
              styles.ratingButton,
              minRating === rating
                ? { backgroundColor: colors.tint }
                : { backgroundColor: colors.background },
            ]}
            onPress={() => setMinRating(minRating === rating ? undefined : rating)}
          >
            <Text
              style={[
                styles.ratingButtonText,
                minRating === rating
                  ? { color: colors.background }
                  : { color: colors.text },
              ]}
            >
              {rating}
            </Text>
          </TouchableOpacity>
        ))}
      </View>
    </View>
  );

  const renderFooter = () => {
    if (!isLoadingMore) return null;
    return (
      <View style={styles.footer}>
        <ActivityIndicator size="small" color={colors.tint} />
      </View>
    );
  };

  const renderEmpty = () => {
    if (isInitialLoad || isLoading) {
      return (
        <View style={styles.emptyContainer}>
          <ActivityIndicator size="large" color={colors.tint} />
        </View>
      );
    }
    return (
      <View style={styles.emptyContainer}>
        <Text style={[styles.emptyText, { color: colors.icon }]}>
          No workouts found
        </Text>
      </View>
    );
  };

  return (
    <View style={styles.container}>
      <FlatList
        data={workouts}
        renderItem={renderWorkoutCard}
        keyExtractor={(item) => item.id.toString()}
        ListHeaderComponent={renderHeader}
        ListFooterComponent={renderFooter}
        ListEmptyComponent={renderEmpty}
        onEndReached={handleLoadMore}
        onEndReachedThreshold={0.5}
        onRefresh={handleRefresh}
        refreshing={isLoading}
      />
      <LocationPickerModal
        visible={isLocationModalVisible}
        onClose={() => setIsLocationModalVisible(false)}
        onSelect={handleLocationSelect}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    padding: 16,
    gap: 12,
  },
  searchInput: {
    height: 40,
    borderRadius: 8,
    paddingHorizontal: 12,
    fontSize: 14,
  },
  controlRow: {
    flexDirection: 'row',
    gap: 8,
  },
  button: {
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 8,
  },
  buttonText: {
    fontSize: 14,
    fontWeight: '600',
  },
  filterRow: {
    flexDirection: 'row',
    gap: 8,
    alignItems: 'center',
  },
  filterChip: {
    paddingVertical: 6,
    paddingHorizontal: 12,
    borderRadius: 16,
  },
  filterChipText: {
    fontSize: 12,
    fontWeight: '500',
  },
  clearButton: {
    width: 24,
    height: 24,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  clearButtonText: {
    fontSize: 16,
    fontWeight: '600',
  },
  ratingFilterRow: {
    flexDirection: 'row',
    gap: 8,
    alignItems: 'center',
  },
  filterLabel: {
    fontSize: 14,
    fontWeight: '500',
  },
  ratingButton: {
    width: 32,
    height: 32,
    borderRadius: 16,
    alignItems: 'center',
    justifyContent: 'center',
  },
  ratingButtonText: {
    fontSize: 14,
    fontWeight: '600',
  },
  card: {
    marginHorizontal: 16,
    marginBottom: 12,
    padding: 16,
    borderRadius: 12,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  templateName: {
    fontSize: 16,
    fontWeight: '600',
  },
  ratingContainer: {
    flexDirection: 'row',
  },
  rating: {
    fontSize: 14,
  },
  cardDetails: {
    flexDirection: 'row',
    gap: 12,
    flexWrap: 'wrap',
  },
  detailText: {
    fontSize: 12,
  },
  footer: {
    padding: 16,
    alignItems: 'center',
  },
  emptyContainer: {
    padding: 32,
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 14,
  },
});
