import {
  login,
  register,
  googleLogin,
  verifyEmail,
  resendVerification,
  forgotPassword,
  resetPassword,
  refreshToken,
  ValidationProblem,
  TokenResponse,
} from '@/services/auth';

// Mock fetch globally
const mockFetch = jest.fn();
global.fetch = mockFetch as any;

describe('auth service', () => {
  beforeEach(() => {
    mockFetch.mockReset();
  });

  describe('login', () => {
    it('should call POST /api/users/login with credentials', async () => {
      const mockTokenResponse: TokenResponse = {
        tokenType: 'Bearer',
        accessToken: 'access_token_123',
        expiresIn: 3600,
        refreshToken: 'refresh_token_456',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        text: () => Promise.resolve(JSON.stringify(mockTokenResponse)),
      });

      const result = await login('test@example.com', 'Password1!');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/login'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ email: 'test@example.com', password: 'Password1!' }),
        })
      );
      expect(result).toEqual(mockTokenResponse);
    });

    it('should throw error on 401 unauthorized', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: () => Promise.resolve({ detail: 'Invalid credentials' }),
      });

      await expect(login('test@example.com', 'wrongpass')).rejects.toThrow(
        'Invalid credentials'
      );
    });

    it('should throw error with default message when 401 has no detail', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: () => Promise.resolve({}),
      });

      await expect(login('test@example.com', 'wrongpass')).rejects.toThrow(
        'Unauthorized'
      );
    });
  });

  describe('register', () => {
    it('should call POST /api/users/register with all fields including age', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      await register('John', 'Doe', 'john@example.com', 'Password1!', 25);

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/register'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            firstName: 'John',
            lastName: 'Doe',
            email: 'john@example.com',
            password: 'Password1!',
            age: 25,
          }),
        })
      );
    });

    it('should call POST /api/users/register without age when not provided', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      await register('Jane', 'Smith', 'jane@example.com', 'Password2!');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/register'),
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({
            firstName: 'Jane',
            lastName: 'Smith',
            email: 'jane@example.com',
            password: 'Password2!',
          }),
        })
      );
    });

    it('should handle 400 ValidationProblem errors', async () => {
      const validationError: ValidationProblem = {
        type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
        title: 'One or more validation errors occurred.',
        status: 400,
        errors: {
          email: ['Email is already in use.'],
          password: ['Password must be at least 8 characters.'],
        },
      };

      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: () => Promise.resolve(validationError),
      });

      await expect(
        register('Test', 'User', 'test@example.com', 'weak')
      ).rejects.toEqual(validationError);
    });
  });

  describe('googleLogin', () => {
    it('should call POST /api/users/google-login with idToken', async () => {
      const mockTokenResponse: TokenResponse = {
        tokenType: 'Bearer',
        accessToken: 'google_access_token',
        expiresIn: 3600,
        refreshToken: 'google_refresh_token',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        text: () => Promise.resolve(JSON.stringify(mockTokenResponse)),
      });

      const result = await googleLogin('google_id_token_xyz');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/google-login'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ idToken: 'google_id_token_xyz' }),
        })
      );
      expect(result).toEqual(mockTokenResponse);
    });
  });

  describe('verifyEmail', () => {
    it('should call POST /api/users/verify-email with userId and token', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      await verifyEmail('user_123', 'verification_token_abc');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/verify-email'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            userId: 'user_123',
            token: 'verification_token_abc',
          }),
        })
      );
    });

    it('should handle errors when verification fails', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: () => Promise.resolve({
          type: 'ValidationError',
          title: 'Invalid token',
          status: 400,
          errors: { token: ['Token is invalid or expired.'] },
        }),
      });

      await expect(verifyEmail('user_123', 'bad_token')).rejects.toBeTruthy();
    });
  });

  describe('resendVerification', () => {
    it('should call POST /api/users/resend-verification with email', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      await resendVerification('test@example.com');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/resend-verification'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ email: 'test@example.com' }),
        })
      );
    });
  });

  describe('forgotPassword', () => {
    it('should call POST /api/users/forgot-password with email', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      await forgotPassword('test@example.com');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/forgot-password'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ email: 'test@example.com' }),
        })
      );
    });
  });

  describe('resetPassword', () => {
    it('should call POST /api/users/reset-password with email, token, and newPassword', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      await resetPassword('test@example.com', 'reset_token_xyz', 'NewPassword1!');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/reset-password'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            email: 'test@example.com',
            token: 'reset_token_xyz',
            newPassword: 'NewPassword1!',
          }),
        })
      );
    });

    it('should handle validation errors for weak passwords', async () => {
      const validationError: ValidationProblem = {
        type: 'ValidationError',
        title: 'Validation failed',
        status: 400,
        errors: {
          newPassword: ['Password must contain uppercase, lowercase, and number.'],
        },
      };

      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: () => Promise.resolve(validationError),
      });

      await expect(
        resetPassword('test@example.com', 'token', 'weak')
      ).rejects.toEqual(validationError);
    });
  });

  describe('refreshToken', () => {
    it('should call POST /api/users/refresh with refreshToken', async () => {
      const mockTokenResponse: TokenResponse = {
        tokenType: 'Bearer',
        accessToken: 'new_access_token',
        expiresIn: 3600,
        refreshToken: 'new_refresh_token',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        text: () => Promise.resolve(JSON.stringify(mockTokenResponse)),
      });

      const result = await refreshToken('old_refresh_token');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/users/refresh'),
        expect.objectContaining({
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ refreshToken: 'old_refresh_token' }),
        })
      );
      expect(result).toEqual(mockTokenResponse);
    });

    it('should throw error on 401 for invalid refresh token', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: () => Promise.resolve({ detail: 'Invalid refresh token' }),
      });

      await expect(refreshToken('invalid_token')).rejects.toThrow(
        'Invalid refresh token'
      );
    });
  });

  describe('error handling', () => {
    it('should handle 500 server errors with error message', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: () => Promise.resolve({ detail: 'Internal server error' }),
      });

      await expect(login('test@example.com', 'password')).rejects.toThrow(
        'Internal server error'
      );
    });

    it('should handle errors with title property when detail is missing', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: () => Promise.resolve({ title: 'Server Error' }),
      });

      await expect(login('test@example.com', 'password')).rejects.toThrow(
        'Server Error'
      );
    });

    it('should handle errors with generic message when no detail or title', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 403,
        json: () => Promise.resolve({}),
      });

      await expect(login('test@example.com', 'password')).rejects.toThrow(
        'Request failed with status 403'
      );
    });

    it('should handle JSON parse errors gracefully', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: () => Promise.reject(new Error('Invalid JSON')),
      });

      await expect(login('test@example.com', 'password')).rejects.toThrow(
        'Request failed with status 500'
      );
    });
  });

  describe('204 No Content handling', () => {
    it('should return undefined for 204 responses', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      });

      const result = await register('Test', 'User', 'test@example.com', 'Password1!');
      expect(result).toBeUndefined();
    });
  });
});
