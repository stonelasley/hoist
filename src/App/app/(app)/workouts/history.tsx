import React from 'react';
import { View, StyleSheet } from 'react-native';
import { Stack, useRouter } from 'expo-router';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { Colors } from '@/constants/theme';
import { WorkoutHistoryList } from '@/components/WorkoutHistoryList';
import { getWorkoutHistory } from '@/services/workouts';
import { useApi } from '@/hooks/useApi';

export default function WorkoutHistoryScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const router = useRouter();
  const api = useApi();

  const handlePressWorkout = (id: number) => {
    router.push(`/(app)/workouts/${id}`);
  };

  const fetchHistory = async (params: any) => {
    return getWorkoutHistory(api, params);
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Stack.Screen
        options={{
          title: 'Workout History',
          headerStyle: { backgroundColor: colors.background },
          headerTintColor: colors.text,
        }}
      />
      <WorkoutHistoryList onPressWorkout={handlePressWorkout} fetchHistory={fetchHistory} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
});
