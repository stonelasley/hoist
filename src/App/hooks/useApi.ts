import { useCallback, useMemo } from 'react';
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

  const makeRequest = useCallback(async <T>(
    path: string,
    method: string,
    body?: unknown
  ): Promise<T> => {
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

    // Handle empty responses (204 No Content, or 200 with no body)
    const text = await response.text();
    if (!text) {
      return undefined as T;
    }

    return JSON.parse(text);
  }, [accessToken]);

  return useMemo(() => ({
    get: <T>(path: string) => makeRequest<T>(path, 'GET'),
    post: <T>(path: string, body?: unknown) => makeRequest<T>(path, 'POST', body),
    put: <T>(path: string, body?: unknown) => makeRequest<T>(path, 'PUT', body),
    del: <T>(path: string) => makeRequest<T>(path, 'DELETE'),
  }), [makeRequest]);
}
