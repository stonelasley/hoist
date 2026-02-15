export type WorkoutTemplateBriefDto = {
  id: number;
  name: string;
  notes: string | null;
  locationId: number | null;
  locationName: string | null;
  created: string;
  lastModified: string;
  exerciseCount: number;
};

export type WorkoutTemplateExerciseDto = {
  id: number;
  exerciseTemplateId: number;
  exerciseName: string;
  implementType: string;
  exerciseType: string;
  position: number;
};

export type WorkoutTemplateDetailDto = {
  id: number;
  name: string;
  notes: string | null;
  locationId: number | null;
  locationName: string | null;
  created: string;
  lastModified: string;
  exercises: WorkoutTemplateExerciseDto[];
};

export type CreateWorkoutTemplateRequest = {
  name: string;
  notes?: string;
  locationId?: number;
};

export type UpdateWorkoutTemplateRequest = {
  id: number;
  name: string;
  notes?: string;
  locationId?: number;
};

type Api = {
  get: <T>(path: string) => Promise<T>;
  post: <T>(path: string, body?: unknown) => Promise<T>;
  put: <T>(path: string, body?: unknown) => Promise<T>;
  del: <T>(path: string) => Promise<T>;
};

export async function getWorkoutTemplates(
  api: Api,
  locationId?: number,
): Promise<WorkoutTemplateBriefDto[]> {
  const params = new URLSearchParams();
  if (locationId !== undefined) params.append('locationId', locationId.toString());
  const query = params.toString();
  const path = `/api/WorkoutTemplates${query ? `?${query}` : ''}`;
  return api.get<WorkoutTemplateBriefDto[]>(path);
}

export async function getWorkoutTemplate(
  api: Api,
  id: number,
): Promise<WorkoutTemplateDetailDto> {
  return api.get<WorkoutTemplateDetailDto>(`/api/WorkoutTemplates/${id}`);
}

export async function createWorkoutTemplate(
  api: Api,
  data: CreateWorkoutTemplateRequest,
): Promise<number> {
  return api.post<number>('/api/WorkoutTemplates', data);
}

export async function updateWorkoutTemplate(
  api: Api,
  id: number,
  data: UpdateWorkoutTemplateRequest,
): Promise<void> {
  return api.put<void>(`/api/WorkoutTemplates/${id}`, data);
}

export async function deleteWorkoutTemplate(
  api: Api,
  id: number,
): Promise<void> {
  return api.del<void>(`/api/WorkoutTemplates/${id}`);
}

export async function updateWorkoutTemplateExercises(
  api: Api,
  workoutTemplateId: number,
  exerciseIds: number[],
): Promise<void> {
  return api.put<void>(`/api/WorkoutTemplates/${workoutTemplateId}/Exercises`, {
    workoutTemplateId,
    exercises: exerciseIds.map((exerciseTemplateId) => ({ exerciseTemplateId })),
  });
}
