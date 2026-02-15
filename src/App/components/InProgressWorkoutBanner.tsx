import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, Pressable } from 'react-native';
import { WorkoutDetailDto } from '@/services/workouts';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

type InProgressWorkoutBannerProps = {
  workout: WorkoutDetailDto;
  onPress: () => void;
};

export function InProgressWorkoutBanner({ workout, onPress }: InProgressWorkoutBannerProps) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const [elapsedTime, setElapsedTime] = useState('');

  const formatRelativeTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 1000 / 60);
    const diffHours = Math.floor(diffMins / 60);

    if (diffHours > 0) {
      return `${diffHours}h ago`;
    } else if (diffMins > 0) {
      return `${diffMins}m ago`;
    } else {
      return 'just now';
    }
  };

  useEffect(() => {
    const updateElapsed = () => {
      const start = new Date(workout.startedAt);
      const now = new Date();
      const durationMs = now.getTime() - start.getTime();
      const minutes = Math.floor(durationMs / 1000 / 60);
      const hours = Math.floor(minutes / 60);
      const remainingMinutes = minutes % 60;

      if (hours > 0) {
        setElapsedTime(`${hours}h ${remainingMinutes}m`);
      } else {
        setElapsedTime(`${minutes}m`);
      }
    };

    updateElapsed();
    const interval = setInterval(updateElapsed, 60000); // Update every minute

    return () => clearInterval(interval);
  }, [workout.startedAt]);

  return (
    <Pressable
      style={({ pressed }) => [
        styles.banner,
        {
          backgroundColor: `${colors.tint}20`,
          borderColor: colors.tint,
          opacity: pressed ? 0.8 : 1,
        }
      ]}
      onPress={onPress}
    >
      <View style={styles.content}>
        <View style={styles.textContainer}>
          <Text style={[styles.title, { color: colors.tint }]}>
            Workout in Progress
          </Text>
          <Text style={[styles.templateName, { color: colors.text }]}>
            {workout.templateName}
          </Text>
          <Text style={[styles.detail, { color: colors.icon }]}>
            Started {formatRelativeTime(workout.startedAt)}
          </Text>
        </View>
        <View style={[styles.badge, { backgroundColor: colors.tint }]}>
          <Text style={[styles.badgeText, { color: '#FFFFFF' }]}>
            {elapsedTime}
          </Text>
        </View>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  banner: {
    padding: 16,
    borderRadius: 12,
    borderWidth: 2,
  },
  content: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  textContainer: {
    flex: 1,
    gap: 4,
  },
  title: {
    fontSize: 12,
    fontWeight: '600',
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  templateName: {
    fontSize: 18,
    fontWeight: '700',
  },
  detail: {
    fontSize: 14,
  },
  badge: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 8,
  },
  badgeText: {
    fontSize: 16,
    fontWeight: '700',
  },
});
