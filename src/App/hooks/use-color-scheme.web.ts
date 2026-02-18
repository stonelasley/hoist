import { useThemePreference } from './useThemePreference';

/**
 * Web variant - the ThemePreferenceProvider handles system color scheme
 * detection and user preference resolution, so this is identical to the
 * native variant.
 */
export function useColorScheme(): 'light' | 'dark' {
  const { colorScheme } = useThemePreference();
  return colorScheme;
}
