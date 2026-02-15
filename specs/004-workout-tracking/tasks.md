# Tasks: Workout Tracking

**Input**: Design documents from `/specs/004-workout-tracking/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are included per Constitution Principle V (Test-Driven Quality). Backend functional and unit tests are required.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Domain enums, entities, and database infrastructure shared across all user stories

- [x] T001 [P] Create WorkoutStatus enum (InProgress=0, Completed=1) in src/Domain/Enums/WorkoutStatus.cs
- [x] T002 [P] Create WeightUnit enum (Lbs=0, Kg=1) in src/Domain/Enums/WeightUnit.cs
- [x] T003 [P] Create DistanceUnit enum (Miles=0, Kilometers=1, Meters=2, Yards=3) in src/Domain/Enums/DistanceUnit.cs
- [x] T004 [P] Create UserPreferences entity (BaseAuditableEntity, fields: UserId, WeightUnit, DistanceUnit, Bodyweight) in src/Domain/Entities/UserPreferences.cs
- [x] T005 [P] Create Workout entity (BaseAuditableEntity, fields: WorkoutTemplateId?, TemplateName, Status, StartedAt, EndedAt?, Notes?, Rating?, LocationId?, LocationName?, UserId, navigation props for Exercises collection) in src/Domain/Entities/Workout.cs
- [x] T006 [P] Create WorkoutExercise entity (BaseAuditableEntity, fields: WorkoutId, ExerciseTemplateId?, ExerciseName, ImplementType, ExerciseType, Position, navigation props for Sets collection and Workout) in src/Domain/Entities/WorkoutExercise.cs
- [x] T007 [P] Create WorkoutSet entity (BaseAuditableEntity, fields: WorkoutExerciseId, Position, Weight?, Reps?, Duration?, Distance?, Bodyweight?, BandColor?, WeightUnit?, DistanceUnit?, navigation prop for WorkoutExercise) in src/Domain/Entities/WorkoutSet.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Database configuration, DbContext updates, and interface changes that MUST be complete before ANY user story

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T008 Add DbSet properties for Workout, WorkoutExercise, WorkoutSet, and UserPreferences to IApplicationDbContext interface in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T009 Add DbSet properties for Workout, WorkoutExercise, WorkoutSet, and UserPreferences to ApplicationDbContext in src/Infrastructure/Data/ApplicationDbContext.cs
- [x] T010 [P] Create UserPreferencesConfiguration (unique index on UserId, FK to ApplicationUser with NoAction delete, decimal precision 6,2 for Bodyweight, default values for WeightUnit=Lbs and DistanceUnit=Miles) in src/Infrastructure/Data/Configurations/UserPreferencesConfiguration.cs
- [x] T011 [P] Create WorkoutConfiguration (FKs: nullable to WorkoutTemplate with SetNull, nullable to Location with SetNull, required to ApplicationUser with Cascade; indexes on (UserId,Status), (UserId,EndedAt), (UserId,Rating), (LocationId); max lengths: TemplateName 200, Notes 2000, LocationName 200; default Status=InProgress) in src/Infrastructure/Data/Configurations/WorkoutConfiguration.cs
- [x] T012 [P] Create WorkoutExerciseConfiguration (FK to Workout with Cascade delete, nullable FK to ExerciseTemplate with SetNull, index on (WorkoutId,Position), max length ExerciseName 200) in src/Infrastructure/Data/Configurations/WorkoutExerciseConfiguration.cs
- [x] T013 [P] Create WorkoutSetConfiguration (FK to WorkoutExercise with Cascade delete, index on (WorkoutExerciseId,Position), decimal precisions: Weight 8,2, Distance 10,4, Bodyweight 6,2) in src/Infrastructure/Data/Configurations/WorkoutSetConfiguration.cs
- [x] T014 Verify solution builds with `dotnet build -tl` and fix any compilation errors from new entities/configurations

**Checkpoint**: Foundation ready - all entities, configs, and DbContext are wired up. User story implementation can now begin.

---

## Phase 3: User Story 1 - Start and Record a Workout (Priority: P1) MVP

**Goal**: Users can start a workout from a template, record sets with measurements appropriate to each exercise type/implement, and manage exercises during the session. Sets are persisted immediately.

**Independent Test**: Start a workout from a template, add sets for barbell (weight+reps), bodyweight (bodyweight+reps), and distance (distance+duration) exercises, verify data persists via API.

### Implementation for User Story 1

- [x] T015 [P] [US1] Create WorkoutBriefDto (id, templateName, startedAt, endedAt, rating, locationName) with AutoMapper profile mapping from Workout in src/Application/Workouts/Queries/GetRecentWorkouts/WorkoutBriefDto.cs
- [x] T016 [P] [US1] Create WorkoutSetDto (id, position, weight, reps, duration, distance, bodyweight, bandColor, weightUnit, distanceUnit) with AutoMapper profile mapping from WorkoutSet in src/Application/Workouts/Queries/GetWorkout/WorkoutSetDto.cs
- [x] T017 [P] [US1] Create WorkoutExerciseDto (id, exerciseTemplateId, exerciseName, implementType, exerciseType, position, sets list) with AutoMapper profile mapping from WorkoutExercise in src/Application/Workouts/Queries/GetWorkout/WorkoutExerciseDto.cs
- [x] T018 [US1] Create WorkoutDetailDto (id, templateName, status, startedAt, endedAt, notes, rating, locationId, locationName, exercises list) with AutoMapper profile mapping from Workout in src/Application/Workouts/Queries/GetWorkout/WorkoutDetailDto.cs
- [x] T019 [US1] Implement StartWorkoutCommand (WorkoutTemplateId) and handler: validate template exists and is owned by user, check no InProgress workout exists for user, create Workout with Status=InProgress and StartedAt=now, snapshot TemplateName/LocationId/LocationName from template, create WorkoutExercise records for each template exercise snapshotting ExerciseName/ImplementType/ExerciseType, return new workout Id. Include StartWorkoutCommandValidator (WorkoutTemplateId required). In src/Application/Workouts/Commands/StartWorkout/
- [x] T020 [US1] Implement GetInProgressWorkoutQuery and handler: find user's workout with Status=InProgress, include Exercises with Sets, project to WorkoutDetailDto, return null/empty if none exists. In src/Application/Workouts/Queries/GetInProgressWorkout/
- [x] T021 [US1] Implement GetWorkoutQuery (Id) and handler: find workout by Id owned by current user, include Exercises with Sets ordered by Position, project to WorkoutDetailDto. In src/Application/Workouts/Queries/GetWorkout/
- [x] T022 [US1] Implement CreateWorkoutSetCommand (WorkoutId, WorkoutExerciseId, Weight?, Reps?, Duration?, Distance?, Bodyweight?, BandColor?, WeightUnit?, DistanceUnit?) and handler: validate workout is owned by user and exercise belongs to workout, auto-assign Position as next sequential value, create WorkoutSet, save immediately, return set Id. Include CreateWorkoutSetCommandValidator (non-negative numerics, at least one measurement field populated). In src/Application/Workouts/Commands/CreateWorkoutSet/
- [x] T023 [US1] Implement UpdateWorkoutSetCommand (WorkoutId, WorkoutExerciseId, SetId, same fields as Create) and handler: validate ownership chain (user→workout→exercise→set), update fields. Include UpdateWorkoutSetCommandValidator. In src/Application/Workouts/Commands/UpdateWorkoutSet/
- [x] T024 [US1] Implement DeleteWorkoutSetCommand (WorkoutId, WorkoutExerciseId, SetId) and handler: validate ownership chain, delete set, resequence remaining set positions. In src/Application/Workouts/Commands/DeleteWorkoutSet/
- [x] T025 [US1] Implement UpdateWorkoutExercisesCommand (WorkoutId, Exercises list of ExerciseTemplateId) and handler: validate workout is InProgress and owned by user, validate each exercise template exists and is owned by user, remove exercises not in new list (cascade deletes their sets), add new exercises with snapshot fields, reorder based on array position, preserve existing sets for retained exercises. Include validator. In src/Application/Workouts/Commands/UpdateWorkoutExercises/
- [x] T026 [US1] Implement DiscardWorkoutCommand (Id) and handler: validate workout is InProgress and owned by user, hard delete workout (cascades to exercises and sets). In src/Application/Workouts/Commands/DiscardWorkout/
- [x] T027 [US1] Create Workouts endpoint group (EndpointGroupBase): POST / (StartWorkout), GET /InProgress (GetInProgressWorkout), GET /{id} (GetWorkout), DELETE /{id} (DiscardWorkout), PUT /{id}/Exercises (UpdateWorkoutExercises), POST /{workoutId}/Exercises/{exerciseId}/Sets (CreateWorkoutSet), PUT /{workoutId}/Exercises/{exerciseId}/Sets/{setId} (UpdateWorkoutSet), DELETE /{workoutId}/Exercises/{exerciseId}/Sets/{setId} (DeleteWorkoutSet). All require authorization. In src/Web/Endpoints/Workouts.cs
- [x] T028 [US1] Verify backend builds and NSwag generates updated API spec with `dotnet build -tl`

### Backend Tests for User Story 1

- [x] T029 [P] [US1] Write functional tests for StartWorkoutCommand: test successful start from template (verifies workout created with correct snapshots, exercises populated), test rejection when another workout is in progress, test rejection with invalid template Id. In tests/Application.FunctionalTests/Workouts/Commands/StartWorkoutTests.cs
- [x] T030 [P] [US1] Write functional tests for CreateWorkoutSetCommand: test creating weight+reps set, bodyweight+reps set, band+duration set, distance+duration set, test rejection with invalid workout/exercise Id, test rejection when no measurement fields populated. In tests/Application.FunctionalTests/Workouts/Commands/CreateWorkoutSetTests.cs
- [x] T031 [P] [US1] Write functional tests for UpdateWorkoutSetCommand and DeleteWorkoutSetCommand: test update changes fields, test delete removes set and resequences positions, test ownership validation. In tests/Application.FunctionalTests/Workouts/Commands/UpdateDeleteWorkoutSetTests.cs
- [x] T032 [P] [US1] Write functional tests for UpdateWorkoutExercisesCommand: test adding new exercise, removing exercise (cascade deletes sets), reordering, preserving sets on retained exercises. In tests/Application.FunctionalTests/Workouts/Commands/UpdateWorkoutExercisesTests.cs
- [x] T033 [P] [US1] Write functional tests for DiscardWorkoutCommand: test successful discard of in-progress workout, test rejection for completed workout. In tests/Application.FunctionalTests/Workouts/Commands/DiscardWorkoutTests.cs
- [x] T034 [P] [US1] Write functional tests for GetInProgressWorkoutQuery and GetWorkoutQuery: test returns full workout detail with exercises and sets, test returns null/404 when not found, test user scoping. In tests/Application.FunctionalTests/Workouts/Queries/GetWorkoutTests.cs
- [x] T035 [P] [US1] Write unit tests for StartWorkoutCommandValidator and CreateWorkoutSetCommandValidator in tests/Application.UnitTests/Workouts/

### Mobile Implementation for User Story 1

- [x] T036 [US1] Create workouts API service with TypeScript types (WorkoutDetailDto, WorkoutExerciseDto, WorkoutSetDto, WorkoutBriefDto) and functions: startWorkout, getInProgressWorkout, getWorkout, discardWorkout, updateWorkoutExercises, createWorkoutSet, updateWorkoutSet, deleteWorkoutSet. In src/App/services/workouts.ts
- [x] T037 [P] [US1] Create SetEntryRow component: renders a single set row with appropriate input fields based on exercise ImplementType and ExerciseType (weight+reps, bodyweight+reps, bodyweight+duration, band+reps, band+duration, distance+duration). Accepts onSave/onDelete callbacks. Uses theme tokens. In src/App/components/SetEntryRow.tsx
- [x] T038 [P] [US1] Create BandColorPicker component: displays selectable band color swatches using existing Colour values (White, Red, Orange, Yellow, Green, Blue, Purple, Grey). In src/App/components/BandColorPicker.tsx
- [x] T039 [P] [US1] Create DurationPicker component: input for duration in mm:ss format, converts to/from seconds for API. In src/App/components/DurationPicker.tsx
- [x] T040 [US1] Create WorkoutExerciseCard component: displays exercise name, implement type icon, and a list of SetEntryRow components. Has "Add Set" button that creates a new set via API immediately. Supports swipe-to-delete on sets. In src/App/components/WorkoutExerciseCard.tsx
- [x] T041 [US1] Create active workout screen: displays workout header (template name, start time, location with edit option), list of WorkoutExerciseCard components, "Add/Remove Exercises" button (navigates to exercise picker for UpdateWorkoutExercises), "Complete Workout" button, "Discard" option. Loads in-progress workout on mount via getInProgressWorkout. In src/App/app/(app)/workouts/active.tsx
- [x] T042 [US1] Add "Start Workout" button to workout template detail screen that calls startWorkout API then navigates to active workout screen. Handle case where another workout is already in progress (show alert to resume or discard). In src/App/app/(app)/workout-templates/[id].tsx

**Checkpoint**: User Story 1 complete — users can start a workout from a template, record sets for all exercise types, manage exercises, and discard workouts. Sets persist immediately to server.

---

## Phase 4: User Story 2 - Complete a Workout (Priority: P1)

**Goal**: Users can complete an in-progress workout by setting end time, adding notes, rating, and editing times.

**Independent Test**: Complete an in-progress workout with notes and rating, verify it transitions to Completed status with correct end time.

### Implementation for User Story 2

- [x] T043 [US2] Implement CompleteWorkoutCommand (Id, Notes?, Rating?, StartedAt?, EndedAt?) and handler: validate workout is InProgress and owned by user, set Status=Completed, set EndedAt to provided value or current time, update Notes/Rating/StartedAt if provided, save. Include CompleteWorkoutCommandValidator (rating 1-5 if provided, endedAt after startedAt, notes max 2000 chars). In src/Application/Workouts/Commands/CompleteWorkout/
- [x] T044 [US2] Implement UpdateWorkoutCommand (Id, LocationId?, Notes?, Rating?, StartedAt?, EndedAt?) and handler: validate workout exists and is owned by user (works for both InProgress and Completed), update provided fields, snapshot LocationName if LocationId changed. Include UpdateWorkoutCommandValidator. In src/Application/Workouts/Commands/UpdateWorkout/
- [x] T045 [US2] Add PUT /{id}/Complete (CompleteWorkout) and PUT /{id} (UpdateWorkout) routes to Workouts endpoint group in src/Web/Endpoints/Workouts.cs
- [x] T046 [US2] Verify backend builds with `dotnet build -tl`

### Backend Tests for User Story 2

- [x] T047 [P] [US2] Write functional tests for CompleteWorkoutCommand: test successful completion (status changes, endedAt set), test with notes and rating, test rejection for already-completed workout, test endedAt defaults to now. In tests/Application.FunctionalTests/Workouts/Commands/CompleteWorkoutTests.cs
- [x] T048 [P] [US2] Write functional tests for UpdateWorkoutCommand: test updating notes/rating on completed workout, test updating times, test updating location (snapshot name updated), test rejection for non-owned workout. In tests/Application.FunctionalTests/Workouts/Commands/UpdateWorkoutTests.cs
- [x] T049 [P] [US2] Write unit tests for CompleteWorkoutCommandValidator and UpdateWorkoutCommandValidator in tests/Application.UnitTests/Workouts/

### Mobile Implementation for User Story 2

- [x] T050 [US2] Add completeWorkout and updateWorkout functions to workouts API service in src/App/services/workouts.ts
- [x] T051 [P] [US2] Create RatingPicker component: 1-5 star rating selector using tappable star icons, supports null (no rating). Uses theme tokens. In src/App/components/RatingPicker.tsx
- [x] T052 [P] [US2] Create WorkoutCompletionForm component: displays editable start/end time pickers, notes text input (max 2000 chars), RatingPicker, computed duration display, and confirm/cancel buttons. In src/App/components/WorkoutCompletionForm.tsx
- [x] T053 [US2] Create workout completion screen: shown when user taps "Complete Workout" from active session. Pre-fills end time with current time. Uses WorkoutCompletionForm. On confirm, calls completeWorkout API and navigates to landing page. In src/App/app/(app)/workouts/complete.tsx
- [x] T054 [US2] Create view/edit completed workout screen: loads completed workout by ID via getWorkout, displays all exercises and sets (read-only or editable), shows notes/rating/times with edit capability using UpdateWorkout API. In src/App/app/(app)/workouts/[id].tsx

**Checkpoint**: User Stories 1 + 2 complete — full workout lifecycle works end-to-end (start → record → complete → view/edit).

---

## Phase 5: User Story 3 - Configure Unit Preferences (Priority: P2)

**Goal**: Users can set weight unit (lbs/kg), distance unit (miles/km/m/yards), and current bodyweight in settings.

**Independent Test**: Set preferences via settings screen, verify they are returned correctly by API.

### Implementation for User Story 3

- [x] T055 [P] [US3] Implement GetUserPreferencesQuery and handler: find UserPreferences for current user, return defaults (Lbs, Miles, null bodyweight) if no record exists (do not create), project to UserPreferencesDto. Create UserPreferencesDto (weightUnit string, distanceUnit string, bodyweight decimal?). In src/Application/UserPreferences/Queries/GetUserPreferences/
- [x] T056 [P] [US3] Implement UpsertUserPreferencesCommand (WeightUnit, DistanceUnit, Bodyweight?) and handler: find existing UserPreferences for user, if exists update fields, if not create new record. Include UpsertUserPreferencesCommandValidator (valid enum values, bodyweight positive if provided). In src/Application/UserPreferences/Commands/UpsertUserPreferences/
- [x] T057 [US3] Create UserPreferences endpoint group (EndpointGroupBase): GET / (GetUserPreferences), PUT / (UpsertUserPreferences). Both require authorization. In src/Web/Endpoints/UserPreferences.cs
- [x] T058 [US3] Verify backend builds with `dotnet build -tl`

### Backend Tests for User Story 3

- [x] T059 [P] [US3] Write functional tests for GetUserPreferencesQuery: test returns defaults when no record exists, test returns saved preferences after upsert. In tests/Application.FunctionalTests/UserPreferences/Queries/GetUserPreferencesTests.cs
- [x] T060 [P] [US3] Write functional tests for UpsertUserPreferencesCommand: test create on first call, test update on subsequent call, test validation (invalid enum, negative bodyweight). In tests/Application.FunctionalTests/UserPreferences/Commands/UpsertUserPreferencesTests.cs
- [x] T061 [P] [US3] Write unit tests for UpsertUserPreferencesCommandValidator in tests/Application.UnitTests/UserPreferences/

### Mobile Implementation for User Story 3

- [x] T062 [US3] Create user-preferences API service with TypeScript types (UserPreferencesDto) and functions: getUserPreferences, upsertUserPreferences. In src/App/services/user-preferences.ts
- [x] T063 [US3] Create preferences settings screen: loads current preferences on mount, displays weight unit picker (Lbs/Kg), distance unit picker (Miles/Km/M/Yards), bodyweight input field with unit label, save button calls upsertUserPreferences. Uses theme tokens. In src/App/app/(app)/settings/preferences.tsx
- [x] T064 [US3] Add "Unit Preferences" navigation item to settings screen that routes to preferences.tsx in src/App/app/(app)/settings/index.tsx

**Checkpoint**: User Story 3 complete — users can configure weight/distance units and bodyweight in settings.

---

## Phase 6: User Story 6 - Set Current Bodyweight (Priority: P2)

**Goal**: Bodyweight is pre-filled in sets for bodyweight-implement exercises using the user's recorded bodyweight.

**Independent Test**: Set bodyweight in preferences, start a workout with a bodyweight exercise, verify bodyweight is pre-filled when adding a set.

**Note**: US6 is combined with US3 for backend (UserPreferences already stores bodyweight). This phase covers the mobile integration of bodyweight pre-fill into the active workout flow.

### Implementation for User Story 6

- [x] T065 [US6] Update active workout screen to fetch user preferences on mount and pass bodyweight value to WorkoutExerciseCard components for bodyweight-implement exercises in src/App/app/(app)/workouts/active.tsx
- [x] T066 [US6] Update SetEntryRow component to accept a defaultBodyweight prop and pre-fill the bodyweight field when the exercise ImplementType is Bodyweight and the user has a recorded bodyweight in src/App/components/SetEntryRow.tsx

**Checkpoint**: User Story 6 complete — bodyweight exercises auto-fill the user's recorded bodyweight.

---

## Phase 7: User Story 4 - View Recent Workouts on Landing Page (Priority: P2)

**Goal**: Landing page shows the 3 most recently completed workouts and any in-progress workout with resume option.

**Independent Test**: Complete 3+ workouts, verify landing page displays the 3 most recent with template name, date, duration, and rating.

### Implementation for User Story 4

- [x] T067 [US4] Implement GetRecentWorkoutsQuery and handler: find current user's 3 most recent workouts with Status=Completed, ordered by EndedAt descending, project to WorkoutBriefDto list. In src/Application/Workouts/Queries/GetRecentWorkouts/
- [x] T068 [US4] Add GET /Recent route (GetRecentWorkouts) to Workouts endpoint group in src/Web/Endpoints/Workouts.cs
- [x] T069 [US4] Verify backend builds with `dotnet build -tl`

### Backend Tests for User Story 4

- [x] T070 [P] [US4] Write functional tests for GetRecentWorkoutsQuery: test returns max 3, test ordering by EndedAt, test excludes in-progress workouts, test empty result for new user. In tests/Application.FunctionalTests/Workouts/Queries/GetRecentWorkoutsTests.cs

### Mobile Implementation for User Story 4

- [x] T071 [US4] Add getRecentWorkouts function to workouts API service in src/App/services/workouts.ts
- [x] T072 [P] [US4] Create RecentWorkoutsList component: displays list of WorkoutBriefDto cards showing template name, date, formatted duration (EndedAt - StartedAt), rating stars, and location name. Tapping a card navigates to workouts/[id]. Empty state shows message encouraging user to start first workout. In src/App/components/RecentWorkoutsList.tsx
- [x] T073 [P] [US4] Create InProgressWorkoutBanner component: shown when getInProgressWorkout returns a workout. Displays template name, start time, elapsed time. Tapping navigates to workouts/active. In src/App/components/InProgressWorkoutBanner.tsx
- [x] T074 [US4] Update landing page to include InProgressWorkoutBanner (conditionally, at top), RecentWorkoutsList section, and "View All History" navigation link. Fetch in-progress workout and recent workouts on mount. In src/App/app/(app)/index.tsx

**Checkpoint**: User Story 4 complete — landing page shows in-progress workout banner and 3 most recent completed workouts.

---

## Phase 8: User Story 5 - View Full Workout History (Priority: P3)

**Goal**: Full workout history screen with cursor-based pagination, sort by date/rating, filter by location/rating, and search by notes.

**Independent Test**: Navigate to history, verify default sort by date, apply location filter, search by notes text, page through results.

### Implementation for User Story 5

- [x] T075 [US5] Implement GetWorkoutHistoryQuery (SortBy?, SortDirection?, LocationId?, MinRating?, Search?, Cursor?, PageSize?) and handler: query completed workouts for current user, apply filters (LocationId WHERE, MinRating WHERE, Search LIKE on Notes), apply sort (date=EndedAt or rating=Rating, with direction), implement cursor-based pagination using (EndedAt,Id) or (Rating,Id) keyset, return PaginatedWorkoutList (items + nextCursor as Base64-encoded JSON). In src/Application/Workouts/Queries/GetWorkoutHistory/
- [x] T076 [US5] Add GET / route (GetWorkoutHistory with [AsParameters] query object) to Workouts endpoint group in src/Web/Endpoints/Workouts.cs
- [x] T077 [US5] Verify backend builds with `dotnet build -tl`

### Backend Tests for User Story 5

- [x] T078 [P] [US5] Write functional tests for GetWorkoutHistoryQuery: test default sort by date desc, test sort by rating, test location filter, test minRating filter, test notes search (case-insensitive), test cursor pagination (first page + next page), test empty result. In tests/Application.FunctionalTests/Workouts/Queries/GetWorkoutHistoryTests.cs

### Mobile Implementation for User Story 5

- [x] T079 [US5] Add getWorkoutHistory function (with sort/filter/search/cursor params) to workouts API service in src/App/services/workouts.ts
- [x] T080 [US5] Create WorkoutHistoryList component: infinite-scroll FlatList of workout cards, sort controls (date/rating toggle with direction), filter chips for location (opens LocationPickerModal) and minimum rating, search text input for notes. Uses cursor-based pagination — loads next page when user scrolls near bottom. In src/App/components/WorkoutHistoryList.tsx
- [x] T081 [US5] Create workout history screen: renders WorkoutHistoryList, passes navigation handlers for tapping individual workouts (navigates to workouts/[id]). In src/App/app/(app)/workouts/history.tsx

**Checkpoint**: User Story 5 complete — full workout history with sort, filter, search, and pagination.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final integration, cleanup, and cross-cutting validation

- [x] T082 Run `dotnet format` to ensure consistent code style across the solution
- [x] T083 Run full backend test suite with `dotnet test` and fix any failures
- [x] T084 Verify all functional tests pass for snapshot preservation: delete a workout template that has recorded workouts, verify workout data and template name are preserved. Delete (soft) an exercise template referenced by workout sets, verify set data and exercise name are preserved
- [x] T085 Verify all functional tests pass for edge cases: attempt to start second workout while one is in-progress (expect 400), complete workout with zero-value sets (expect success), update completed workout fields (expect success)
- [x] T086 Run quickstart.md validation — verify all scaffold commands, build commands, and test commands work as documented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — core workout tracking
- **US2 (Phase 4)**: Depends on US1 (builds on workout lifecycle commands and endpoints)
- **US3 (Phase 5)**: Depends on Phase 2 only — can run in parallel with US1/US2
- **US6 (Phase 6)**: Depends on US1 (active workout screen) + US3 (preferences API)
- **US4 (Phase 7)**: Depends on US2 (needs completed workouts to display)
- **US5 (Phase 8)**: Depends on US2 (needs completed workouts for history)
- **Polish (Phase 9)**: Depends on all user stories

### User Story Dependencies

- **US1 (P1)**: Start after Phase 2 — no story dependencies
- **US2 (P1)**: Start after US1 — extends the Workouts endpoint with Complete and Update
- **US3 (P2)**: Start after Phase 2 — independent of US1/US2 (separate entity and endpoints)
- **US6 (P2)**: Start after US1 + US3 — integrates bodyweight pre-fill into active workout
- **US4 (P2)**: Start after US2 — needs completed workouts and the GetRecentWorkouts query
- **US5 (P3)**: Start after US2 — needs completed workouts for history query

### Within Each User Story

- DTOs and AutoMapper profiles before commands/queries
- Commands/queries before endpoints
- Endpoints before backend tests
- Backend tests before mobile services
- Mobile services before mobile components
- Mobile components before mobile screens

### Parallel Opportunities

- Phase 1: All 7 enum/entity tasks (T001-T007) can run in parallel
- Phase 2: All 4 configuration tasks (T010-T013) can run in parallel
- US1: DTOs T015-T017 can run in parallel; Backend tests T029-T035 can run in parallel
- US2: Tests T047-T049 in parallel; Components T051-T052 in parallel
- US3: Backend (T055-T056) in parallel; Tests T059-T061 in parallel
- US4: Components T072-T073 in parallel; Tests T070 with mobile T071
- US3 and US1 can run in parallel after Phase 2

---

## Parallel Example: Phase 1 (Setup)

```
# Launch all entity/enum creation tasks together:
Task T001: "Create WorkoutStatus enum in src/Domain/Enums/WorkoutStatus.cs"
Task T002: "Create WeightUnit enum in src/Domain/Enums/WeightUnit.cs"
Task T003: "Create DistanceUnit enum in src/Domain/Enums/DistanceUnit.cs"
Task T004: "Create UserPreferences entity in src/Domain/Entities/UserPreferences.cs"
Task T005: "Create Workout entity in src/Domain/Entities/Workout.cs"
Task T006: "Create WorkoutExercise entity in src/Domain/Entities/WorkoutExercise.cs"
Task T007: "Create WorkoutSet entity in src/Domain/Entities/WorkoutSet.cs"
```

## Parallel Example: User Story 1 Backend Tests

```
# Launch all US1 functional tests together:
Task T029: "StartWorkout functional tests"
Task T030: "CreateWorkoutSet functional tests"
Task T031: "UpdateWorkoutSet/DeleteWorkoutSet functional tests"
Task T032: "UpdateWorkoutExercises functional tests"
Task T033: "DiscardWorkout functional tests"
Task T034: "GetWorkout query functional tests"
Task T035: "Validator unit tests"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (enums + entities)
2. Complete Phase 2: Foundational (configs + DbContext)
3. Complete Phase 3: US1 — Start and Record a Workout
4. Complete Phase 4: US2 — Complete a Workout
5. **STOP and VALIDATE**: Full workout lifecycle works end-to-end
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 + US2 → Full workout lifecycle (MVP!)
3. US3 + US6 → Unit preferences + bodyweight pre-fill
4. US4 → Recent workouts on landing page
5. US5 → Full workout history with search/filter
6. Each increment adds value without breaking previous functionality

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All entities extend BaseAuditableEntity for automatic audit fields
- All endpoints require authorization via .RequireAuthorization()
- Snapshot fields are set once at workout creation and never updated from source
