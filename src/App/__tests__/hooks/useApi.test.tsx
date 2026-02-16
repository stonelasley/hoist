import React from 'react';
import { renderHook, act, waitFor } from '@testing-library/react-native';
import { useApi } from '@/hooks/useApi';
import { useAuth } from '@/hooks/useAuth';

// Mock useAuth hook
jest.mock('@/hooks/useAuth', () => ({
  useAuth: jest.fn(),
}));

const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

// Mock fetch globally
const mockFetch = jest.fn();
global.fetch = mockFetch as any;

/** Helper to create a mock Response with both json() and text() */
function mockResponse(status: number, body?: unknown) {
  const ok = status >= 200 && status < 300;
  const text = body !== undefined ? JSON.stringify(body) : '';
  return {
    ok,
    status,
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(text),
  };
}

describe('useApi hook', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockFetch.mockReset();

    // Default mock for useAuth
    mockUseAuth.mockReturnValue({
      accessToken: 'test_access_token',
      refreshToken: 'test_refresh_token',
      isAuthenticated: true,
      isLoading: false,
      setTokens: jest.fn(),
      clearTokens: jest.fn(),
    });
  });

  describe('get method', () => {
    it('should make GET request with auth header', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, { data: 'test data' }));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.get('/api/users/profile');
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/profile'),
        expect.objectContaining({
          method: 'GET',
          headers: {
            Authorization: 'Bearer test_access_token',
          },
        })
      );
      expect(response).toEqual({ data: 'test data' });
    });

    it('should make GET request without auth header when not authenticated', async () => {
      mockUseAuth.mockReturnValue({
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
        setTokens: jest.fn(),
        clearTokens: jest.fn(),
      });

      mockFetch.mockResolvedValueOnce(mockResponse(200, { data: 'public data' }));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.get('/api/public');
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/public'),
        expect.objectContaining({
          method: 'GET',
          headers: {},
          body: undefined,
        })
      );
      expect(response).toEqual({ data: 'public data' });
    });
  });

  describe('post method', () => {
    it('should make POST request with body and auth header', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, { success: true }));

      const { result } = renderHook(() => useApi());

      const requestBody = { name: 'Test', email: 'test@example.com' };

      let response: any;
      await act(async () => {
        response = await result.current.post('/api/users', requestBody);
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            Authorization: 'Bearer test_access_token',
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(requestBody),
        })
      );
      expect(response).toEqual({ success: true });
    });

    it('should make POST request without body', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(204));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.post('/api/actions/trigger');
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/actions/trigger'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            Authorization: 'Bearer test_access_token',
          },
          body: undefined,
        })
      );
      expect(response).toBeUndefined();
    });
  });

  describe('put method', () => {
    it('should make PUT request with body and auth header', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, { updated: true }));

      const { result } = renderHook(() => useApi());

      const updateData = { name: 'Updated Name' };

      let response: any;
      await act(async () => {
        response = await result.current.put('/api/users/123', updateData);
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/123'),
        expect.objectContaining({
          method: 'PUT',
          headers: {
            Authorization: 'Bearer test_access_token',
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(updateData),
        })
      );
      expect(response).toEqual({ updated: true });
    });
  });

  describe('del method', () => {
    it('should make DELETE request with auth header', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(204));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.del('/api/users/123');
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/123'),
        expect.objectContaining({
          method: 'DELETE',
          headers: {
            Authorization: 'Bearer test_access_token',
          },
          body: undefined,
        })
      );
      expect(response).toBeUndefined();
    });
  });

  describe('error handling', () => {
    it('should throw error when response is not ok', async () => {
      const errorResponse = {
        detail: 'Resource not found',
        status: 404,
      };

      mockFetch.mockResolvedValueOnce(mockResponse(404, errorResponse));

      const { result } = renderHook(() => useApi());

      await expect(
        act(async () => {
          await result.current.get('/api/users/999');
        })
      ).rejects.toEqual(errorResponse);
    });

    it('should handle 401 unauthorized responses', async () => {
      const errorResponse = {
        detail: 'Unauthorized',
        status: 401,
      };

      mockFetch.mockResolvedValueOnce(mockResponse(401, errorResponse));

      const { result } = renderHook(() => useApi());

      await expect(
        act(async () => {
          await result.current.get('/api/protected');
        })
      ).rejects.toEqual(errorResponse);
    });

    it('should handle 400 validation errors', async () => {
      const validationError = {
        type: 'ValidationProblem',
        title: 'Validation failed',
        status: 400,
        errors: {
          email: ['Email is required'],
        },
      };

      mockFetch.mockResolvedValueOnce(mockResponse(400, validationError));

      const { result } = renderHook(() => useApi());

      await expect(
        act(async () => {
          await result.current.post('/api/users', {});
        })
      ).rejects.toEqual(validationError);
    });

    it('should handle 500 server errors', async () => {
      const serverError = {
        detail: 'Internal server error',
        status: 500,
      };

      mockFetch.mockResolvedValueOnce(mockResponse(500, serverError));

      const { result } = renderHook(() => useApi());

      await expect(
        act(async () => {
          await result.current.get('/api/error');
        })
      ).rejects.toEqual(serverError);
    });
  });

  describe('204 No Content handling', () => {
    it('should return undefined for 204 GET responses', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(204));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.get('/api/status');
      });

      expect(response).toBeUndefined();
    });

    it('should return undefined for 204 POST responses', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(204));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.post('/api/action', { data: 'test' });
      });

      expect(response).toBeUndefined();
    });

    it('should return undefined for 204 DELETE responses', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(204));

      const { result } = renderHook(() => useApi());

      let response: any;
      await act(async () => {
        response = await result.current.del('/api/users/123');
      });

      expect(response).toBeUndefined();
    });
  });

  describe('API base URL', () => {
    it('should construct correct API URLs', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, {}));

      const { result } = renderHook(() => useApi());

      await act(async () => {
        await result.current.get('/api/test');
      });

      // Should use API_BASE_URL from constants
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringMatching(/\/api\/test$/),
        expect.any(Object)
      );
    });
  });

  describe('content type handling', () => {
    it('should set Content-Type header for POST with body', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, {}));

      const { result } = renderHook(() => useApi());

      await act(async () => {
        await result.current.post('/api/test', { data: 'test' });
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
    });

    it('should set Content-Type header for PUT with body', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, {}));

      const { result } = renderHook(() => useApi());

      await act(async () => {
        await result.current.put('/api/test', { data: 'test' });
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
    });

    it('should not set Content-Type header for GET', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(200, {}));

      const { result } = renderHook(() => useApi());

      await act(async () => {
        await result.current.get('/api/test');
      });

      const callArgs = mockFetch.mock.calls[0][1] as any;
      expect(callArgs.headers['Content-Type']).toBeUndefined();
    });

    it('should not set Content-Type header for DELETE', async () => {
      mockFetch.mockResolvedValueOnce(mockResponse(204));

      const { result } = renderHook(() => useApi());

      await act(async () => {
        await result.current.del('/api/test');
      });

      const callArgs = mockFetch.mock.calls[0][1] as any;
      expect(callArgs.headers['Content-Type']).toBeUndefined();
    });
  });
});
