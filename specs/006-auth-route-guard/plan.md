# Implementation Plan: Auth Route Guard

**Branch**: `006-auth-route-guard` | **Date**: 2026-02-16 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-auth-route-guard/spec.md`

## Summary

Add auth-based route protection to the Expo Router root layout. Unauthenticated users are redirected to the login screen; authenticated users bypass login and land on the app landing page. A loading indicator prevents screen flash during auth initialization. Unused Expo boilerplate (tabs layout) is removed.

## Technical Context

**Language/Version**: TypeScript 5.9 / React Native 0.81 / React 19
**Primary Dependencies**: Expo SDK 54, expo-router 6, React Navigation 7
**Storage**: N/A (no data model changes)
**Testing**: Jest with `@testing-library/react-native`
**Target Platform**: iOS, Android, Web
**Project Type**: Mobile (Expo React Native)
**Performance Goals**: Auth redirect completes within 1 second of app launch
**Constraints**: No screen flash during auth state resolution
**Scale/Scope**: 1 file modified, 3 files deleted, 0 new files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Architecture | N/A | No backend changes |
| II. CQRS via MediatR | N/A | No backend changes |
| III. Domain-Driven Design | N/A | No backend changes |
| IV. API-First with Minimal APIs | N/A | No backend changes |
| V. Test-Driven Quality | PASS | Existing useAuth tests cover auth state; manual testing for navigation |
| VI. Fitness Domain Integrity | N/A | No domain logic changes |
| VII. Simplicity and YAGNI | PASS | Uses standard Expo Router auth pattern; minimal code addition |
| VIII. Mobile-First with Expo React Native | PASS | Uses Expo Router `useSegments`/`useRouter` for file-based routing; functional component with hooks; theme colors from centralized constants |

All gates pass. No violations.

## Project Structure

### Documentation (this feature)

```text
specs/006-auth-route-guard/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/App/
├── app/
│   ├── _layout.tsx              # MODIFY: Add AuthGate with redirect logic
│   ├── (auth)/
│   │   ├── _layout.tsx          # NO CHANGE
│   │   ├── login.tsx            # NO CHANGE (already uses router.replace)
│   │   └── ...
│   ├── (app)/
│   │   ├── _layout.tsx          # NO CHANGE
│   │   ├── index.tsx            # NO CHANGE
│   │   └── ...
│   └── (tabs)/                  # DELETE: Unused Expo boilerplate
│       ├── _layout.tsx
│       ├── index.tsx
│       └── explore.tsx
└── hooks/
    └── useAuth.tsx              # NO CHANGE (provides isAuthenticated, isLoading)
```

**Structure Decision**: Mobile-only change. Single file modification (`_layout.tsx`) plus deletion of 3 unused boilerplate files. No new files needed — the auth gate logic belongs in the root layout as an inner component.

## Design

### Auth Gate Pattern

The root layout (`app/_layout.tsx`) wraps navigation in an `AuthGate` component that:

1. Reads `isAuthenticated` and `isLoading` from `useAuth()`
2. Reads current route segment via `useSegments()` from expo-router
3. Uses `useEffect` to redirect based on auth state:
   - `isLoading` → render `ActivityIndicator` (no navigation stack visible)
   - Not authenticated + in `(app)` group → `router.replace('/(auth)/login')`
   - Authenticated + in `(auth)` group → `router.replace('/(app)')`
4. Renders `<Slot />` for all other cases (normal navigation)

### Key Decisions

- **No `<Stack>` while loading**: The `AuthGate` returns an `ActivityIndicator` before the `<Slot />` renders, preventing any screen flash.
- **`router.replace` not `router.push`**: Prevents back-navigation to the wrong route group.
- **Inner component pattern**: `AuthGate` is defined inside `RootLayout` to stay within the `AuthProvider` context. It wraps `<Slot />` rather than the `<Stack>`.
- **Segment check**: Only the first segment is checked (`(auth)` vs `(app)`) — no deep route tracking needed.
- **Logout handling**: When `clearTokens()` is called, `isAuthenticated` becomes `false`, which triggers the `useEffect` in `AuthGate` to redirect to login. No additional logout logic needed.
- **Remove `(tabs)` from Stack**: The root layout currently declares `<Stack.Screen name="(tabs)" />` implicitly by having the directory. Deleting the files removes it from routing.
