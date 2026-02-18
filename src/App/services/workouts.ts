export type WorkoutDetailDto = {
  id: number;
  templateName: string;
  status: string;
  startedAt: string;
  endedAt: string | null;
  notes: string | null;
  rating: number | null;
  locationId: number | null;
  locationName: string | null;
  exercises: WorkoutExerciseDto[];
};

export type WorkoutExerciseDto = {
  id: number;
  exerciseTemplateId: number | null;
  exerciseName: string;
  implementType: string;
  exerciseType: string;
  position: number;
  sets: WorkoutSetDto[];
};

export type WorkoutSetDto = {
  id: number;
  position: number;
  weight: number | null;
  reps: number | null;
  duration: number | null;
  distance: number | null;
  bodyweight: number | null;
  bandColor: string | null;
  weightUnit: string | null;
  distanceUnit: string | null;
};

export type WorkoutBriefDto = {
  id: number;
  templateName: string;
  startedAt: string;
  endedAt: string | null;
  rating: number | null;
  locationName: string | null;
};

export type CreateWorkoutSetRequest = {
  weight?: number;
  reps?: number;
  duration?: number;
  distance?: number;
  bodyweight?: number;
  bandColor?: string;
  weightUnit?: string;
  distanceUnit?: string;
};

export type UpdateWorkoutSetRequest = CreateWorkoutSetRequest;

export type UpdateWorkoutExercisesRequest = {
  workoutId: number;
  exercises: { exerciseTemplateId: number }[];
};

export type CompleteWorkoutRequest = {
  id: number;
  notes?: string;
  rating?: number;
  startedAt?: string;
  endedAt?: string;
};

export type UpdateWorkoutRequest = {
  id: number;
  locationId?: number;
  notes?: string;
  rating?: number;
  startedAt?: string;
  endedAt?: string;
};

export type PaginatedWorkoutList = {
  items: WorkoutBriefDto[];
  nextCursor: string | null;
};

export type WorkoutHistoryParams = {
  sortBy?: string;
  sortDirection?: string;
  locationId?: number;
  minRating?: number;
  search?: string;
  cursor?: string;
  pageSize?: number;
};

type Api = {
  get: <T>(path: string) => Promise<T>;
  post: <T>(path: string, body?: unknown) => Promise<T>;
  put: <T>(path: string, body?: unknown) => Promise<T>;
  del: <T>(path: string) => Promise<T>;
};

export async function startWorkout(
  api: Api,
  workoutTemplateId: number,
): Promise<number> {
  return api.post<number>('/api/Workouts', { workoutTemplateId });
}

export async function getInProgressWorkout(
  api: Api,
): Promise<WorkoutDetailDto | null> {
  return api.get<WorkoutDetailDto | null>('/api/Workouts/InProgress');
}

export async function getRecentWorkouts(
  api: Api,
): Promise<WorkoutBriefDto[]> {
  return api.get<WorkoutBriefDto[]>('/api/Workouts/Recent');
}

export async function getWorkout(
  api: Api,
  id: number,
): Promise<WorkoutDetailDto> {
  return api.get<WorkoutDetailDto>(`/api/Workouts/${id}`);
}

export async function discardWorkout(
  api: Api,
  id: number,
): Promise<void> {
  return api.del<void>(`/api/Workouts/${id}`);
}

export async function updateWorkoutExercises(
  api: Api,
  workoutId: number,
  exerciseIds: number[],
): Promise<void> {
  return api.put<void>(`/api/Workouts/${workoutId}/Exercises`, {
    workoutId,
    exercises: exerciseIds.map((exerciseTemplateId) => ({ exerciseTemplateId })),
  });
}

export async function createWorkoutSet(
  api: Api,
  workoutId: number,
  exerciseId: number,
  data: CreateWorkoutSetRequest,
): Promise<number> {
  return api.post<number>(
    `/api/Workouts/${workoutId}/Exercises/${exerciseId}/Sets`,
    { ...data, workoutId, workoutExerciseId: exerciseId },
  );
}

export async function updateWorkoutSet(
  api: Api,
  workoutId: number,
  exerciseId: number,
  setId: number,
  data: UpdateWorkoutSetRequest,
): Promise<void> {
  return api.put<void>(
    `/api/Workouts/${workoutId}/Exercises/${exerciseId}/Sets/${setId}`,
    data,
  );
}

export async function deleteWorkoutSet(
  api: Api,
  workoutId: number,
  exerciseId: number,
  setId: number,
): Promise<void> {
  return api.del<void>(
    `/api/Workouts/${workoutId}/Exercises/${exerciseId}/Sets/${setId}`,
  );
}

export async function completeWorkout(
  api: Api,
  id: number,
  data: CompleteWorkoutRequest,
): Promise<void> {
  return api.put<void>(`/api/Workouts/${id}/Complete`, data);
}

export async function updateWorkout(
  api: Api,
  id: number,
  data: UpdateWorkoutRequest,
): Promise<void> {
  return api.put<void>(`/api/Workouts/${id}`, data);
}

export async function getWorkoutHistory(
  api: Api,
  params?: WorkoutHistoryParams,
): Promise<PaginatedWorkoutList> {
  const searchParams = new URLSearchParams();
  if (params?.sortBy) searchParams.append('sortBy', params.sortBy);
  if (params?.sortDirection) searchParams.append('sortDirection', params.sortDirection);
  if (params?.locationId !== undefined) searchParams.append('locationId', params.locationId.toString());
  if (params?.minRating !== undefined) searchParams.append('minRating', params.minRating.toString());
  if (params?.search) searchParams.append('search', params.search);
  if (params?.cursor) searchParams.append('cursor', params.cursor);
  if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
  const query = searchParams.toString();
  return api.get<PaginatedWorkoutList>(`/api/Workouts${query ? `?${query}` : ''}`);
}
