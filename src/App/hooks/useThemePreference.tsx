import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useColorScheme as useRNColorScheme } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

type ThemePreference = 'light' | 'dark' | 'system';

type ThemePreferenceContextType = {
  preference: ThemePreference;
  colorScheme: 'light' | 'dark';
  setPreference: (preference: ThemePreference) => void;
  isLoading: boolean;
};

const ThemePreferenceContext = createContext<ThemePreferenceContextType | undefined>(undefined);

const STORAGE_KEY = 'theme_preference';

const VALID_PREFERENCES: ThemePreference[] = ['light', 'dark', 'system'];

function isValidPreference(value: string | null): value is ThemePreference {
  return value !== null && VALID_PREFERENCES.includes(value as ThemePreference);
}

export function ThemePreferenceProvider({ children }: { children: ReactNode }) {
  const systemColorScheme = useRNColorScheme();
  const [preference, setPreferenceState] = useState<ThemePreference>('system');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadPreference() {
      try {
        const stored = await AsyncStorage.getItem(STORAGE_KEY);
        if (isValidPreference(stored)) {
          setPreferenceState(stored);
        }
      } catch (error) {
        console.error('Failed to load theme preference:', error);
      } finally {
        setIsLoading(false);
      }
    }

    loadPreference();
  }, []);

  const setPreference = (newPreference: ThemePreference) => {
    setPreferenceState(newPreference);
    AsyncStorage.setItem(STORAGE_KEY, newPreference).catch((error) => {
      console.error('Failed to save theme preference:', error);
    });
  };

  const colorScheme: 'light' | 'dark' =
    preference === 'system'
      ? (systemColorScheme ?? 'light')
      : preference;

  const value: ThemePreferenceContextType = {
    preference,
    colorScheme,
    setPreference,
    isLoading,
  };

  return (
    <ThemePreferenceContext.Provider value={value}>
      {children}
    </ThemePreferenceContext.Provider>
  );
}

export function useThemePreference(): ThemePreferenceContextType {
  const context = useContext(ThemePreferenceContext);
  if (context === undefined) {
    throw new Error('useThemePreference must be used within a ThemePreferenceProvider');
  }
  return context;
}
