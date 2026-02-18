import { useThemePreference } from './useThemePreference';

export function useColorScheme(): 'light' | 'dark' {
  const { colorScheme } = useThemePreference();
  return colorScheme;
}
