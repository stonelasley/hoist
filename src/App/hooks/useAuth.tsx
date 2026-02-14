import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

type AuthState = {
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
};

type AuthContextType = AuthState & {
  setTokens: (accessToken: string, refreshToken: string) => Promise<void>;
  clearTokens: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const ACCESS_TOKEN_KEY = 'auth_access_token';
const REFRESH_TOKEN_KEY = 'auth_refresh_token';

// Web fallback storage (in-memory)
let webAccessToken: string | null = null;
let webRefreshToken: string | null = null;

const isWeb = Platform.OS === 'web';

async function getStoredToken(key: string): Promise<string | null> {
  if (isWeb) {
    return key === ACCESS_TOKEN_KEY ? webAccessToken : webRefreshToken;
  }
  return await SecureStore.getItemAsync(key);
}

async function setStoredToken(key: string, value: string): Promise<void> {
  if (isWeb) {
    if (key === ACCESS_TOKEN_KEY) {
      webAccessToken = value;
    } else {
      webRefreshToken = value;
    }
    return;
  }
  await SecureStore.setItemAsync(key, value);
}

async function deleteStoredToken(key: string): Promise<void> {
  if (isWeb) {
    if (key === ACCESS_TOKEN_KEY) {
      webAccessToken = null;
    } else {
      webRefreshToken = null;
    }
    return;
  }
  await SecureStore.deleteItemAsync(key);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>({
    accessToken: null,
    refreshToken: null,
    isAuthenticated: false,
    isLoading: true,
  });

  useEffect(() => {
    // Load tokens from storage on mount
    async function loadTokens() {
      try {
        const accessToken = await getStoredToken(ACCESS_TOKEN_KEY);
        const refreshToken = await getStoredToken(REFRESH_TOKEN_KEY);

        setAuthState({
          accessToken,
          refreshToken,
          isAuthenticated: !!(accessToken && refreshToken),
          isLoading: false,
        });
      } catch (error) {
        console.error('Failed to load tokens:', error);
        setAuthState({
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
          isLoading: false,
        });
      }
    }

    loadTokens();
  }, []);

  const setTokens = async (accessToken: string, refreshToken: string) => {
    try {
      await setStoredToken(ACCESS_TOKEN_KEY, accessToken);
      await setStoredToken(REFRESH_TOKEN_KEY, refreshToken);

      setAuthState({
        accessToken,
        refreshToken,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error) {
      console.error('Failed to store tokens:', error);
      throw error;
    }
  };

  const clearTokens = async () => {
    try {
      await deleteStoredToken(ACCESS_TOKEN_KEY);
      await deleteStoredToken(REFRESH_TOKEN_KEY);

      setAuthState({
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
      });
    } catch (error) {
      console.error('Failed to clear tokens:', error);
      throw error;
    }
  };

  const value: AuthContextType = {
    ...authState,
    setTokens,
    clearTokens,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
