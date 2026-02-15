export type ExerciseTemplateBriefDto = {
  id: number;
  name: string;
  implementType: string;
  exerciseType: string;
  imagePath: string | null;
  model: string | null;
};

export type ExerciseTemplateDetailDto = ExerciseTemplateBriefDto & {
  created: string;
  lastModified: string;
};

export type CreateExerciseTemplateRequest = {
  name: string;
  implementType: number;
  exerciseType: number;
  model?: string;
};

export type UpdateExerciseTemplateRequest = {
  id: number;
  name: string;
  implementType: number;
  exerciseType: number;
  model?: string;
};

export const IMPLEMENT_TYPES = [
  'Barbell',
  'Dumbbell',
  'Selectorized Machine',
  'Plate Loaded Machine',
  'Bodyweight',
  'Band',
  'Kettlebell',
  'Plate',
  'Medicine Ball',
] as const;

export const EXERCISE_TYPES = ['Reps', 'Duration', 'Distance'] as const;

type Api = {
  get: <T>(path: string) => Promise<T>;
  post: <T>(path: string, body?: unknown) => Promise<T>;
  put: <T>(path: string, body?: unknown) => Promise<T>;
  del: <T>(path: string) => Promise<T>;
};

export async function getExerciseTemplates(
  api: Api,
  search?: string,
  implementType?: string,
  exerciseType?: string,
): Promise<ExerciseTemplateBriefDto[]> {
  const params = new URLSearchParams();
  if (search) params.append('search', search);
  if (implementType) params.append('implementType', implementType);
  if (exerciseType) params.append('exerciseType', exerciseType);

  const query = params.toString();
  const path = `/api/ExerciseTemplates${query ? `?${query}` : ''}`;
  return api.get<ExerciseTemplateBriefDto[]>(path);
}

export async function getExerciseTemplate(
  api: Api,
  id: number,
): Promise<ExerciseTemplateDetailDto> {
  return api.get<ExerciseTemplateDetailDto>(`/api/ExerciseTemplates/${id}`);
}

export async function createExerciseTemplate(
  api: Api,
  data: CreateExerciseTemplateRequest,
): Promise<number> {
  return api.post<number>('/api/ExerciseTemplates', data);
}

export async function updateExerciseTemplate(
  api: Api,
  id: number,
  data: UpdateExerciseTemplateRequest,
): Promise<void> {
  return api.put<void>(`/api/ExerciseTemplates/${id}`, data);
}

export async function deleteExerciseTemplate(
  api: Api,
  id: number,
): Promise<void> {
  return api.del<void>(`/api/ExerciseTemplates/${id}`);
}

/**
 * Upload an image for an exercise template.
 * Uses direct fetch because the useApi hook always sets Content-Type to JSON,
 * but image upload requires multipart/form-data.
 */
export async function uploadExerciseTemplateImage(
  baseUrl: string,
  accessToken: string | null,
  id: number,
  imageUri: string,
  fileName: string,
  mimeType: string,
): Promise<void> {
  const formData = new FormData();
  formData.append('file', {
    uri: imageUri,
    name: fileName,
    type: mimeType,
  } as unknown as Blob);

  const headers: HeadersInit = {};
  if (accessToken) {
    headers['Authorization'] = `Bearer ${accessToken}`;
  }

  const response = await fetch(`${baseUrl}/api/ExerciseTemplates/${id}/Image`, {
    method: 'POST',
    headers,
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Image upload failed with status ${response.status}`);
  }
}

export async function deleteExerciseTemplateImage(
  api: Api,
  id: number,
): Promise<void> {
  return api.del<void>(`/api/ExerciseTemplates/${id}/Image`);
}
