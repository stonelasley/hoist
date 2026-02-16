# Tasks: Auth Route Guard

**Input**: Design documents from `/specs/006-auth-route-guard/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, quickstart.md

**Tests**: Not explicitly requested in the feature specification. Test tasks are omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No setup tasks needed — this feature modifies an existing file and deletes unused files. No new dependencies, no new project structure.

*(No tasks in this phase)*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Remove unused boilerplate that could interfere with auth routing

- [x] T001 Delete unused Expo boilerplate file `src/App/app/(tabs)/_layout.tsx`
- [x] T002 [P] Delete unused Expo boilerplate file `src/App/app/(tabs)/index.tsx`
- [x] T003 [P] Delete unused Expo boilerplate file `src/App/app/(tabs)/explore.tsx`

**Checkpoint**: Unused `(tabs)` route group removed. Expo Router no longer auto-discovers these routes.

---

## Phase 3: User Story 1 & 2 - Auth-Based Redirect (Priority: P1) 🎯 MVP

**Goal**: Unauthenticated users are redirected to login; authenticated users bypass login and land on the landing page. These two stories are implemented together as they are two sides of the same `AuthGate` component.

**Independent Test**:
- Clear tokens → open app → login screen appears
- Log in → close app → reopen → landing page appears (native) or login screen (web, tokens in-memory)
- Manually navigate to `/(app)` while unauthenticated → redirected to login

### Implementation

- [x] T004 [US1] [US2] Add `AuthGate` component to `src/App/app/_layout.tsx` that reads `isAuthenticated` and `isLoading` from `useAuth()`, reads current route segment via `useSegments()` from `expo-router`, and uses `useEffect` to call `router.replace('/(auth)/login')` when not authenticated and in `(app)` group, or `router.replace('/(app)')` when authenticated and in `(auth)` group. Remove the explicit `<Stack.Screen>` declarations for `(auth)` and `(app)` and use `<Slot />` instead, since the auth gate controls navigation.

**Checkpoint**: Auth redirect works for both unauthenticated and authenticated users. Core MVP is complete.

---

## Phase 4: User Story 3 - Loading State (Priority: P2)

**Goal**: Display a loading indicator while auth state initializes, preventing screen flash.

**Independent Test**: Observe app startup — loading indicator appears briefly, then correct screen shows with no flash of wrong screen.

### Implementation

- [x] T005 [US3] In the `AuthGate` component in `src/App/app/_layout.tsx`, return an `ActivityIndicator` centered on screen (using theme colors from `@/constants/theme`) when `isLoading` is `true`, instead of rendering `<Slot />`. This prevents any navigation screen from rendering before auth state is known.

**Checkpoint**: No screen flash on startup. Loading indicator appears briefly, then correct destination.

---

## Phase 5: User Story 4 - Boilerplate Cleanup Verification (Priority: P3)

**Goal**: Verify the `(tabs)` deletion from Phase 2 doesn't break anything.

**Independent Test**: Navigate to root URL → auth guard handles routing, no tabs layout appears.

*(Covered by T001-T003 in Phase 2. No additional tasks needed.)*

**Checkpoint**: All user stories complete.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [x] T006 Run quickstart.md manual test scenarios (all 7 tests) to validate end-to-end behavior
- [x] T007 Verify existing login flow still works: login → `router.replace('/(app)')` in `src/App/app/(auth)/login.tsx` should continue to function correctly with the auth gate

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately
- **User Stories 1 & 2 (Phase 3)**: Can start after Phase 2 (or in parallel, since different files)
- **User Story 3 (Phase 4)**: Depends on Phase 3 (extends the same `AuthGate` component)
- **Polish (Phase 6)**: Depends on all previous phases

### User Story Dependencies

- **US1 + US2 (P1)**: Independent — implemented together in `_layout.tsx`
- **US3 (P2)**: Extends the `AuthGate` from US1/US2 — depends on Phase 3
- **US4 (P3)**: Covered by Phase 2 deletions — no dependencies on other stories

### Parallel Opportunities

- T001, T002, T003 can all run in parallel (deleting independent files)
- T004 and T001-T003 can run in parallel (different files)
- T005 depends on T004 (same component)

---

## Parallel Example

```bash
# All file deletions can run in parallel:
Task: "Delete src/App/app/(tabs)/_layout.tsx"
Task: "Delete src/App/app/(tabs)/index.tsx"
Task: "Delete src/App/app/(tabs)/explore.tsx"

# Auth gate implementation can run in parallel with deletions:
Task: "Add AuthGate to src/App/app/_layout.tsx"
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2)

1. Delete `(tabs)` boilerplate (T001-T003)
2. Add `AuthGate` to root layout (T004)
3. **STOP and VALIDATE**: Test unauthenticated redirect and authenticated bypass
4. Deploy/demo if ready

### Full Feature

1. Complete MVP (T001-T004)
2. Add loading state (T005)
3. Run full validation (T006-T007)

---

## Notes

- This is a small, focused feature: 1 file modified, 3 files deleted, 0 new files
- T004 is the core task — it contains the entire auth gate logic
- T005 is a refinement of T004 (adds loading state handling to the same component)
- No backend changes, no new dependencies, no data model changes
- Commit after T003 (cleanup) and after T004 (MVP) at minimum
