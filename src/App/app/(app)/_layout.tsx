import { Stack } from 'expo-router';

export default function AppLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="index" />
      <Stack.Screen
        name="workout-templates/create"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="workout-templates/[id]"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="exercise-templates/create"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="exercise-templates/[id]"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="settings/index"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="settings/locations/index"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="settings/locations/create"
        options={{ presentation: 'card' }}
      />
      <Stack.Screen
        name="settings/locations/[id]"
        options={{ presentation: 'card' }}
      />
    </Stack>
  );
}
