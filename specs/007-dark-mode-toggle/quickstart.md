# Quickstart: Dark Mode Toggle

**Feature**: 007-dark-mode-toggle
**Date**: 2026-02-18

## Prerequisites

- Node.js and npm installed
- Expo CLI available (`npx expo`)
- iOS Simulator or Android Emulator (or physical device with Expo Go)

## Setup

```bash
# Install new dependency
cd src/App
npm install @react-native-async-storage/async-storage
```

## Development

```bash
# Start Expo dev server
cd src/App
npx expo start
```

## Files to Create

| File | Purpose |
|------|---------|
| `src/App/hooks/useThemePreference.tsx` | Context provider + hook for theme preference state |
| `src/App/app/(app)/settings/appearance.tsx` | Appearance selection screen |

## Files to Modify

| File | Change |
|------|--------|
| `src/App/package.json` | Add `@react-native-async-storage/async-storage` dependency |
| `src/App/hooks/use-color-scheme.ts` | Read resolved scheme from ThemePreference context |
| `src/App/hooks/use-color-scheme.web.ts` | Same change for web platform |
| `src/App/app/_layout.tsx` | Wrap app with `ThemePreferenceProvider` |
| `src/App/app/(app)/settings/index.tsx` | Add "Appearance" row to settings list |
| `src/App/components/RatingPicker.tsx` | Fix import to use `@/hooks/use-color-scheme` instead of `react-native` |

## Verification

1. Open the app → Settings → Appearance
2. Select "Dark" → entire app should switch to dark colors immediately
3. Select "Light" → entire app should switch to light colors immediately
4. Select "System" → app should follow device setting
5. Force-close and reopen → preference should persist
6. Verify all screens render correctly in both modes (no hard-coded colors, no unreadable text)
