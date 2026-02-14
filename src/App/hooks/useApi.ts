import { useAuth } from '@/hooks/useAuth';
import { API_BASE_URL } from '@/constants/api';

type ApiMethods = {
  get: <T>(path: string) => Promise<T>;
  post: <T>(path: string, body?: unknown) => Promise<T>;
  put: <T>(path: string, body?: unknown) => Promise<T>;
  del: <T>(path: string) => Promise<T>;
};

export function useApi(): ApiMethods {
  const { accessToken } = useAuth();

  async function makeRequest<T>(
    path: string,
    method: string,
    body?: unknown
  ): Promise<T> {
    const headers: HeadersInit = {};

    if (accessToken) {
      headers['Authorization'] = `Bearer ${accessToken}`;
    }

    if (body && (method === 'POST' || method === 'PUT')) {
      headers['Content-Type'] = 'application/json';
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });

    if (!response.ok) {
      const error = await response.json();
      throw error;
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return undefined as T;
    }

    return response.json();
  }

  return {
    get: <T>(path: string) => makeRequest<T>(path, 'GET'),
    post: <T>(path: string, body?: unknown) => makeRequest<T>(path, 'POST', body),
    put: <T>(path: string, body?: unknown) => makeRequest<T>(path, 'PUT', body),
    del: <T>(path: string) => makeRequest<T>(path, 'DELETE'),
  };
}
