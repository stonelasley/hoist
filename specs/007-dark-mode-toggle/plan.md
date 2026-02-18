# Implementation Plan: Dark Mode Toggle

**Branch**: `007-dark-mode-toggle` | **Date**: 2026-02-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-dark-mode-toggle/spec.md`

## Summary

Add a user-controllable appearance setting (Light / Dark / System) to the mobile app. The user's preference is persisted locally via AsyncStorage and applied globally through a React Context provider that overrides the existing `useColorScheme` hook. This is a mobile-only change — no backend modifications required.

## Technical Context

**Language/Version**: TypeScript 5.9 / React Native 0.81 / React 19
**Primary Dependencies**: Expo SDK 54, expo-router 6, `@react-native-async-storage/async-storage` (new), React Navigation 7
**Storage**: AsyncStorage (local device storage for theme preference)
**Testing**: Manual testing via Expo dev server (no existing mobile test framework in project)
**Target Platform**: iOS, Android, Web (Expo managed workflow)
**Project Type**: Mobile
**Performance Goals**: Theme switch reflected in <1 second (per SC-001)
**Constraints**: Must not break existing system-default behavior; zero disruption to current users
**Scale/Scope**: ~37 files currently call `useColorScheme` — the design minimizes changes by overriding at the hook level

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Architecture | N/A | No backend changes |
| II. CQRS via MediatR | N/A | No backend changes |
| III. Domain-Driven Design | N/A | No backend changes |
| IV. API-First with Minimal APIs | N/A | No backend changes |
| V. Test-Driven Quality | PASS | Mobile-only feature; no existing mobile test framework to extend |
| VI. Fitness Domain Integrity | N/A | Does not affect workout/exercise data |
| VII. Simplicity and YAGNI | PASS | Minimal new code; leverages existing color system; AsyncStorage is the simplest persistence option |
| VIII. Mobile-First with Expo | PASS | Uses Expo Router for new screen, functional components with hooks, centralized theme tokens, AsyncStorage is Expo-compatible |

**Constitution gate: PASSED** — No violations.

**Post-Phase 1 re-check**: Design confirmed compliant. The `use-color-scheme` hook override approach means 37+ consumer files require zero import changes. New npm package (`@react-native-async-storage/async-storage`) is Expo-compatible per Principle VII.

## Project Structure

### Documentation (this feature)

```text
specs/007-dark-mode-toggle/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/App/
├── hooks/
│   ├── useThemePreference.tsx     # NEW — ThemePreferenceProvider context + useThemePreference hook
│   ├── use-color-scheme.ts        # MODIFY — read resolved scheme from ThemePreference context
│   └── use-color-scheme.web.ts    # MODIFY — same change for web platform
├── app/
│   ├── _layout.tsx                # MODIFY — wrap with ThemePreferenceProvider
│   └── (app)/settings/
│       ├── index.tsx              # MODIFY — add "Appearance" row
│       └── appearance.tsx         # NEW — appearance selection screen
├── components/
│   └── RatingPicker.tsx           # MODIFY — fix direct react-native import to use @/hooks/use-color-scheme
└── package.json                   # MODIFY — add @react-native-async-storage/async-storage
```

**Structure Decision**: All changes are within the existing `src/App/` mobile project. No new projects, no backend changes, no new directories beyond what already exists.

## Design Decisions

### 1. Hook-Level Override (Key Design Choice)

Instead of updating 37+ files that call `useColorScheme()`, the `use-color-scheme` hook will be modified to read the resolved color scheme from the `ThemePreferenceProvider` context. This means:
- All existing consumers automatically respect the user's preference
- Zero import changes across the codebase
- The provider resolves `'system'` preference to the actual device scheme

### 2. ThemePreferenceProvider Context

Follows the same pattern as the existing `AuthProvider`:
- Wraps the app at the root level (inside `AuthProvider`, outside `ThemeProvider`)
- Exposes `preference` (the raw user choice: `'light' | 'dark' | 'system'`), `colorScheme` (the resolved value: `'light' | 'dark'`), and `setPreference` function
- Loads stored preference from AsyncStorage on mount with `isLoading` guard
- Defaults to `'system'` when no preference is stored or value is corrupted

### 3. AsyncStorage for Persistence

- Local-only preference — no backend round-trip needed
- `@react-native-async-storage/async-storage` is the standard Expo-compatible solution
- Key: `'theme_preference'`, values: `'light' | 'dark' | 'system'`
- Web platform: AsyncStorage uses localStorage under the hood — no special handling needed (unlike SecureStore)

### 4. Appearance Screen UI

- Added as a new row in the existing settings list
- Navigates to `/(app)/settings/appearance` screen
- Three radio-style options matching the segmented control pattern used in `preferences.tsx`
- Selection is applied immediately (no save button needed — simpler than unit preferences)

## Complexity Tracking

> No constitution violations — table intentionally empty.
