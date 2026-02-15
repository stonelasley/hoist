import React from 'react';
import { View, Text, StyleSheet, Pressable } from 'react-native';
import { WorkoutBriefDto } from '@/services/workouts';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

type RecentWorkoutsListProps = {
  workouts: WorkoutBriefDto[];
  onPress: (id: number) => void;
};

export function RecentWorkoutsList({ workouts, onPress }: RecentWorkoutsListProps) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  const formatRelativeTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 1000 / 60);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) {
      return `${diffDays}d ago`;
    } else if (diffHours > 0) {
      return `${diffHours}h ago`;
    } else if (diffMins > 0) {
      return `${diffMins}m ago`;
    } else {
      return 'just now';
    }
  };

  if (workouts.length === 0) {
    return (
      <View style={[styles.emptyContainer, { backgroundColor: `${colors.icon}15` }]}>
        <Text style={[styles.emptyText, { color: colors.icon }]}>
          No recent workouts. Start your first workout!
        </Text>
      </View>
    );
  }

  const formatDuration = (startedAt: string, endedAt: string | null) => {
    if (!endedAt) return '';
    const start = new Date(startedAt);
    const end = new Date(endedAt);
    const durationMs = end.getTime() - start.getTime();
    const minutes = Math.floor(durationMs / 1000 / 60);
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;

    if (hours > 0) {
      return `${hours}h ${remainingMinutes}m`;
    }
    return `${minutes}m`;
  };

  const renderStars = (rating: number | null) => {
    if (!rating) return null;
    return '⭐'.repeat(rating);
  };

  return (
    <View style={styles.container}>
      {workouts.map((workout) => (
        <Pressable
          key={workout.id}
          style={({ pressed }) => [
            styles.card,
            {
              backgroundColor: colorScheme === 'dark' ? '#1c1c1e' : '#f5f5f5',
              borderColor: `${colors.icon}30`,
              opacity: pressed ? 0.7 : 1,
            }
          ]}
          onPress={() => onPress(workout.id)}
        >
          <View style={styles.header}>
            <Text style={[styles.templateName, { color: colors.text }]}>
              {workout.templateName}
            </Text>
            {workout.rating && (
              <Text style={styles.stars}>
                {renderStars(workout.rating)}
              </Text>
            )}
          </View>

          <View style={styles.details}>
            <Text style={[styles.detailText, { color: colors.icon }]}>
              {workout.endedAt && formatRelativeTime(workout.endedAt)}
            </Text>
            <Text style={[styles.detailText, { color: colors.icon }]}>
              • {formatDuration(workout.startedAt, workout.endedAt)}
            </Text>
            {workout.locationName && (
              <Text style={[styles.detailText, { color: colors.icon }]}>
                • {workout.locationName}
              </Text>
            )}
          </View>
        </Pressable>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    gap: 12,
  },
  card: {
    padding: 16,
    borderRadius: 12,
    borderWidth: 1,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  templateName: {
    fontSize: 16,
    fontWeight: '600',
    flex: 1,
  },
  stars: {
    fontSize: 14,
    marginLeft: 8,
  },
  details: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  detailText: {
    fontSize: 14,
  },
  emptyContainer: {
    padding: 24,
    borderRadius: 12,
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 14,
    textAlign: 'center',
  },
});
