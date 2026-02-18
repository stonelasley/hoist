# Research: Dark Mode Toggle

**Feature**: 007-dark-mode-toggle
**Date**: 2026-02-18

## R1: Local Storage for Theme Preference

**Decision**: Use `@react-native-async-storage/async-storage`

**Rationale**: AsyncStorage is the standard key-value storage for React Native / Expo apps. It works across iOS, Android, and web (via localStorage). The theme preference is non-sensitive data, so SecureStore (used for auth tokens) would be overkill. AsyncStorage is already included in the Expo managed workflow ecosystem and requires no native module linking.

**Alternatives considered**:
- `expo-secure-store`: Designed for sensitive data (tokens, passwords). Doesn't work on web without fallback. Overkill for a theme preference.
- MMKV (`react-native-mmkv`): Faster than AsyncStorage but requires a new native dependency and dev client. Over-engineered for a single preference value.
- Server-side storage (like existing unit preferences): Would require backend API changes and network dependency for a UI-only setting. Spec explicitly states local-only.

## R2: Propagating Theme Override to All Consumers

**Decision**: Override the existing `use-color-scheme` hook to read from a React Context, rather than updating 37+ consumer files.

**Rationale**: The app has a custom `@/hooks/use-color-scheme` module that currently re-exports React Native's `useColorScheme`. By modifying this module to read from a `ThemePreferenceProvider` context, all existing consumers automatically respect the user's preference with zero import changes. This is the lowest-risk, lowest-effort approach.

**Alternatives considered**:
- Update every file that calls `useColorScheme()` to use a new hook: High effort (~37 files), high risk of missing one, creates a large diff.
- Monkey-patch React Native's `useColorScheme`: Fragile, breaks on RN updates, not recommended.
- Use React Navigation's `ThemeProvider` alone: Only affects navigation chrome, not custom component styles that index into `Colors[]`.

## R3: Appearance Screen UX Pattern

**Decision**: Dedicated screen with radio-style selection, immediate application (no save button).

**Rationale**: The three-option model (Light / Dark / System) is best presented as a dedicated screen with clearly labeled options. Unlike unit preferences which have multiple fields and a save button, the appearance setting is a single choice that can be applied immediately on tap — matching the behavior users expect from iOS Settings > Appearance and Android Settings > Display.

**Alternatives considered**:
- Inline toggle/switch on settings screen: A toggle only supports two states (on/off), but we need three options (Light / Dark / System).
- Modal/bottom sheet: Adds unnecessary complexity for a simple selection. A full screen is consistent with the existing settings navigation pattern.
- Segmented control on settings screen: Could work but doesn't scale if more options are added later, and inconsistent with the row-navigation pattern used for other settings.

## R4: Web Platform Handling

**Decision**: No special web handling needed.

**Rationale**: AsyncStorage uses `localStorage` on web automatically. The `use-color-scheme.web.ts` variant already exists for SSR hydration concerns — it will be updated to use the same context approach as the native variant. React Native's `useColorScheme` on web reads from `prefers-color-scheme` media query, which will serve as the fallback when preference is `'system'`.
