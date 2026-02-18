# Tasks: Dark Mode Toggle

**Input**: Design documents from `/specs/007-dark-mode-toggle/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Exact file paths included in descriptions

---

## Phase 1: Setup

**Purpose**: Install new dependency required for local preference storage

- [x] T001 Install `@react-native-async-storage/async-storage` in `src/App/package.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the theme preference context and wire it into the existing hook system so all 37+ consumers automatically respect the user's choice

**CRITICAL**: No user story UI work can begin until this phase is complete

- [x] T002 Create `ThemePreferenceProvider` context and `useThemePreference` hook in `src/App/hooks/useThemePreference.tsx` — follows `useAuth` pattern: exposes `preference` (`'light' | 'dark' | 'system'`), `colorScheme` (`'light' | 'dark'`), and `setPreference`; loads from AsyncStorage on mount with `isLoading` guard; defaults to `'system'`; persists to AsyncStorage on change; validates stored values and falls back to `'system'` on corruption
- [x] T003 [P] Modify `src/App/hooks/use-color-scheme.ts` to read resolved `colorScheme` from `ThemePreferenceProvider` context instead of re-exporting React Native's `useColorScheme`
- [x] T004 [P] Modify `src/App/hooks/use-color-scheme.web.ts` to read resolved `colorScheme` from `ThemePreferenceProvider` context instead of the current web-specific re-export
- [x] T005 Wrap app with `ThemePreferenceProvider` in `src/App/app/_layout.tsx` — place inside `AuthProvider`, before React Navigation `ThemeProvider`; update `ThemeProvider` to use resolved scheme from context; handle loading state
- [x] T006 [P] Fix `src/App/components/RatingPicker.tsx` to import `useColorScheme` from `@/hooks/use-color-scheme` instead of directly from `react-native`

**Checkpoint**: After this phase, the app should behave identically to before (defaulting to system theme) but the theme is now controlled by the new context provider.

---

## Phase 3: User Story 1 — Toggle Dark Mode from Settings (Priority: P1) MVP

**Goal**: User can navigate to Settings > Appearance and switch between Light, Dark, and System modes with immediate visual effect across the entire app.

**Independent Test**: Open Settings, tap Appearance, select each option and verify the app's color scheme changes immediately.

### Implementation for User Story 1

- [x] T007 [US1] Add "Appearance" row to the settings list in `src/App/app/(app)/settings/index.tsx` — add a new `TouchableOpacity` row matching existing pattern (text + chevron), navigating to `/(app)/settings/appearance`
- [x] T008 [US1] Create appearance selection screen at `src/App/app/(app)/settings/appearance.tsx` — three radio-style options (Light, Dark, System) using the app's existing styling patterns; read current preference and call `setPreference` from `useThemePreference`; selection applies immediately (no save button); current selection shown with visual indicator (checkmark or tint highlight)

**Checkpoint**: User Story 1 is fully functional — user can toggle between appearance modes and see immediate changes.

---

## Phase 4: User Story 2 — Preference Persists Across Sessions (Priority: P2)

**Goal**: The user's selected appearance mode survives app restarts.

**Independent Test**: Select Dark mode, force-close the app, reopen — app should launch in dark mode.

**Note**: Persistence is already implemented as part of the foundational `ThemePreferenceProvider` (T002), which reads from and writes to AsyncStorage. This phase exists for verification and edge case handling only.

### Implementation for User Story 2

- [x] T009 [US2] Verify persistence and edge case handling in `src/App/hooks/useThemePreference.tsx` — confirm: (1) preference loads from AsyncStorage before first render completes, (2) corrupted/unrecognized stored values fall back to `'system'`, (3) first-time users with no stored value default to `'system'`, (4) real-time system theme changes are reflected when `'system'` is selected

**Checkpoint**: Both user stories are fully functional and independently testable.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup and validation

- [x] T010 Run quickstart.md verification steps — test all three modes on iOS/Android/web, verify persistence, check all screens render correctly in both light and dark modes
- [x] T011 Run ESLint and fix any linting issues across modified files in `src/App/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (AsyncStorage must be installed)
- **User Story 1 (Phase 3)**: Depends on Phase 2 (context + hook override must exist)
- **User Story 2 (Phase 4)**: Depends on Phase 2 (persistence is built into the provider); can run in parallel with Phase 3
- **Polish (Phase 5)**: Depends on Phases 3 and 4

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Foundational phase — no dependency on other stories
- **User Story 2 (P2)**: Depends only on Foundational phase — can run in parallel with US1

### Within Foundational Phase

- T002 must complete before T003, T004, T005 (they depend on the context it creates)
- T003 and T004 can run in parallel [P] (different files, same change pattern)
- T005 depends on T002 (needs to import the provider)
- T006 can run in parallel [P] with any other task (independent fix)

### Parallel Opportunities

```
Phase 2 (after T002):
  T003 ──┐
  T004 ──┤── all parallel (different files)
  T006 ──┘
  T005 ──── sequential (depends on T002)

Phase 3 (after Phase 2):
  T007 ──┐
  T008 ──┘── parallel (different files)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Install dependency
2. Complete Phase 2: Context + hook override (this also gives US2 persistence for free)
3. Complete Phase 3: Appearance UI
4. **STOP and VALIDATE**: Toggle between modes, verify all screens update
5. Demo-ready with full functionality

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready (app behaves identically to before)
2. Add Phase 3 (US1) → Toggle works → Demo (MVP!)
3. Validate Phase 4 (US2) → Persistence confirmed
4. Phase 5 → Polish and final verification

---

## Notes

- Total tasks: 11
- This is a small, focused feature — all tasks are within `src/App/`
- No backend changes, no new API contracts, no database migrations
- The hook-level override design means zero changes to the 37+ existing consumers of `useColorScheme`
- US2 persistence is inherently built into the foundational provider (T002), making Phase 4 a verification step rather than new implementation
