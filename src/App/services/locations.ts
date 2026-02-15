export type LocationDto = {
  id: number;
  name: string;
  instagramHandle: string | null;
  latitude: number | null;
  longitude: number | null;
  notes: string | null;
  address: string | null;
  created: string;
  lastModified: string;
};

export type CreateLocationRequest = {
  name: string;
  instagramHandle?: string;
  latitude?: number;
  longitude?: number;
  notes?: string;
  address?: string;
};

export type UpdateLocationRequest = {
  id: number;
  name: string;
  instagramHandle?: string;
  latitude?: number;
  longitude?: number;
  notes?: string;
  address?: string;
};

type Api = {
  get: <T>(path: string) => Promise<T>;
  post: <T>(path: string, body?: unknown) => Promise<T>;
  put: <T>(path: string, body?: unknown) => Promise<T>;
  del: <T>(path: string) => Promise<T>;
};

export async function getLocations(api: Api): Promise<LocationDto[]> {
  return api.get<LocationDto[]>('/api/Locations');
}

export async function getLocation(api: Api, id: number): Promise<LocationDto> {
  return api.get<LocationDto>(`/api/Locations/${id}`);
}

export async function createLocation(
  api: Api,
  data: CreateLocationRequest,
): Promise<number> {
  return api.post<number>('/api/Locations', data);
}

export async function updateLocation(
  api: Api,
  id: number,
  data: UpdateLocationRequest,
): Promise<void> {
  return api.put<void>(`/api/Locations/${id}`, data);
}

export async function deleteLocation(api: Api, id: number): Promise<void> {
  return api.del<void>(`/api/Locations/${id}`);
}
