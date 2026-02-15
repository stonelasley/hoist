export type UserPreferencesDto = {
  weightUnit: string;
  distanceUnit: string;
  bodyweight: number | null;
};

export type UpsertUserPreferencesRequest = {
  weightUnit: string;
  distanceUnit: string;
  bodyweight?: number;
};

type Api = {
  get: <T>(path: string) => Promise<T>;
  post: <T>(path: string, body?: unknown) => Promise<T>;
  put: <T>(path: string, body?: unknown) => Promise<T>;
  del: <T>(path: string) => Promise<T>;
};

export async function getUserPreferences(api: Api): Promise<UserPreferencesDto> {
  return api.get<UserPreferencesDto>('/api/UserPreferences');
}

export async function upsertUserPreferences(api: Api, data: UpsertUserPreferencesRequest): Promise<void> {
  return api.put<void>('/api/UserPreferences', data);
}
