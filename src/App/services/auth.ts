import { API_BASE_URL } from '@/constants/api';

export type TokenResponse = {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
};

export type ValidationProblem = {
  type: string;
  title: string;
  status: number;
  errors: Record<string, string[]>;
};

async function apiPost<T>(
  endpoint: string,
  body: Record<string, unknown>
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    if (response.status === 400) {
      const validationError: ValidationProblem = await response.json();
      throw validationError;
    }

    if (response.status === 401) {
      const errorBody = await response.json();
      throw new Error(errorBody.detail || 'Unauthorized');
    }

    const errorBody = await response.json().catch(() => ({}));
    throw new Error(errorBody.detail || errorBody.title || `Request failed with status ${response.status}`);
  }

  // Handle empty responses (204 No Content, or 200 with no body)
  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  if (!text) {
    return undefined as T;
  }

  return JSON.parse(text);
}

export async function login(
  email: string,
  password: string
): Promise<TokenResponse> {
  return apiPost<TokenResponse>('/api/users/login', { email, password });
}

export async function register(
  firstName: string,
  lastName: string,
  email: string,
  password: string,
  age?: number
): Promise<void> {
  const body: Record<string, unknown> = {
    firstName,
    lastName,
    email,
    password,
  };

  if (age !== undefined) {
    body.age = age;
  }

  return apiPost<void>('/api/users/register', body);
}

export async function googleLogin(idToken: string): Promise<TokenResponse> {
  return apiPost<TokenResponse>('/api/users/google-login', { idToken });
}

export async function verifyEmail(
  userId: string,
  token: string
): Promise<void> {
  return apiPost<void>('/api/users/verify-email', { userId, token });
}

export async function resendVerification(email: string): Promise<void> {
  return apiPost<void>('/api/users/resend-verification', { email });
}

export async function forgotPassword(email: string): Promise<void> {
  return apiPost<void>('/api/users/forgot-password', { email });
}

export async function resetPassword(
  email: string,
  token: string,
  newPassword: string
): Promise<void> {
  return apiPost<void>('/api/users/reset-password', {
    email,
    token,
    newPassword,
  });
}

export async function refreshToken(
  refreshToken: string
): Promise<TokenResponse> {
  return apiPost<TokenResponse>('/api/users/refresh', { refreshToken });
}
