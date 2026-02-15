# Tasks: Workout & Exercise Templates

**Input**: Design documents from `/specs/002-workout-exercise-templates/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included — the constitution mandates test-driven quality (Principle V).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Domain entities, enums, and database infrastructure shared across all user stories

- [x] T001 [P] Create ImplementType enum with 9 values (Barbell=0 through MedicineBall=8) in src/Domain/Enums/ImplementType.cs
- [x] T002 [P] Create ExerciseType enum with 3 values (Reps=0, Duration=1, Distance=2) in src/Domain/Enums/ExerciseType.cs
- [x] T003 [P] Create ExerciseTemplate entity extending BaseAuditableEntity with Name, ImplementType, ExerciseType, ImagePath, Model, IsDeleted, DeletedAt, UserId in src/Domain/Entities/ExerciseTemplate.cs
- [x] T004 [P] Create WorkoutTemplate entity extending BaseAuditableEntity with Name, Notes, Location, UserId, and IList<WorkoutTemplateExercise> Items navigation in src/Domain/Entities/WorkoutTemplate.cs
- [x] T005 [P] Create WorkoutTemplateExercise entity extending BaseEntity with WorkoutTemplateId, ExerciseTemplateId, Position, and navigation properties in src/Domain/Entities/WorkoutTemplateExercise.cs
- [x] T006 Add DbSet<ExerciseTemplate>, DbSet<WorkoutTemplate>, DbSet<WorkoutTemplateExercise> to IApplicationDbContext in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T007 Add DbSet<ExerciseTemplate>, DbSet<WorkoutTemplate>, DbSet<WorkoutTemplateExercise> to ApplicationDbContext in src/Infrastructure/Data/ApplicationDbContext.cs
- [x] T008 [P] Create ExerciseTemplateConfiguration with Name (max 200, required), ImplementType, ExerciseType, ImagePath (max 500), Model (max 500), IsDeleted default false, UserId FK, global query filter for IsDeleted=false, unique filtered index on (UserId, Name) WHERE IsDeleted=0, index on (UserId, IsDeleted) in src/Infrastructure/Data/Configurations/ExerciseTemplateConfiguration.cs
- [x] T009 [P] Create WorkoutTemplateConfiguration with Name (max 200, required), Notes (max 2000), Location (max 200), UserId FK, index on (UserId) in src/Infrastructure/Data/Configurations/WorkoutTemplateConfiguration.cs
- [x] T010 [P] Create WorkoutTemplateExerciseConfiguration with WorkoutTemplateId FK (cascade delete), ExerciseTemplateId FK (restrict delete), Position required, index on (WorkoutTemplateId, Position) in src/Infrastructure/Data/Configurations/WorkoutTemplateExerciseConfiguration.cs
- [x] T011 Add seed data for sample exercise templates and workout templates in src/Infrastructure/Data/ApplicationDbContextInitialiser.cs — create 3-4 exercises (Bench Press/Barbell/Reps, Squat/Barbell/Reps, Running/Bodyweight/Distance, Plank/Bodyweight/Duration) and 1 workout template with 2-3 exercises for the test user
- [x] T012 Verify solution builds with `dotnet build -tl` from repo root — fix any compilation errors from new entities/DbSets

**Checkpoint**: All domain entities, enums, and database configuration in place. Solution compiles. Ready for use case implementation.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core application-layer DTOs and query infrastructure that multiple user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T013 [P] Create ExerciseTemplateBriefDto (id, name, implementType as string, exerciseType as string, imagePath, model) with AutoMapper Profile mapping from ExerciseTemplate in src/Application/ExerciseTemplates/Queries/GetExerciseTemplates/ExerciseTemplateBriefDto.cs
- [x] T014 [P] Create WorkoutTemplateBriefDto (id, name, notes, location, created, lastModified, exerciseCount) with AutoMapper Profile mapping from WorkoutTemplate in src/Application/WorkoutTemplates/Queries/GetWorkoutTemplates/WorkoutTemplateBriefDto.cs
- [x] T015 [P] Create ExerciseTemplateDetailDto (id, name, implementType, exerciseType, imagePath, model, created, lastModified) with AutoMapper Profile in src/Application/ExerciseTemplates/Queries/GetExerciseTemplate/ExerciseTemplateDetailDto.cs
- [x] T016 [P] Create WorkoutTemplateDetailDto (id, name, notes, location, created, lastModified, exercises list) and WorkoutTemplateExerciseDto (id, exerciseTemplateId, exerciseName, implementType, exerciseType, position) with AutoMapper Profile in src/Application/WorkoutTemplates/Queries/GetWorkoutTemplate/WorkoutTemplateDetailDto.cs
- [x] T017 Verify solution builds with `dotnet build -tl` — fix any DTO/mapping compilation errors

**Checkpoint**: Foundation ready — all shared DTOs defined. User story implementation can now begin.

---

## Phase 3: User Story 1 — View Landing Page with Workout Templates and Exercise Library (Priority: P1) MVP

**Goal**: After login, user sees their workout template list and exercise library with search/filter on the landing page.

**Independent Test**: Log in → verify both workout template list and exercise library display correctly, search and filter exercises by name/implement/type.

### Backend for User Story 1

- [x] T018 [P] [US1] Create GetExerciseTemplates query with search (string?), implementType (ImplementType?), exerciseType (ExerciseType?) parameters, handler filters by current user's non-deleted exercises with case-insensitive name LIKE search and optional enum filters, projects to ExerciseTemplateBriefDto in src/Application/ExerciseTemplates/Queries/GetExerciseTemplates/GetExerciseTemplates.cs
- [x] T019 [P] [US1] Create GetWorkoutTemplates query (no parameters beyond current user), handler returns all user's workout templates with exercise count, projects to WorkoutTemplateBriefDto ordered by LastModified descending in src/Application/WorkoutTemplates/Queries/GetWorkoutTemplates/GetWorkoutTemplates.cs
- [x] T020 [P] [US1] Create ExerciseTemplates endpoint group extending EndpointGroupBase, map GET / with search, implementType, exerciseType query params, RequireAuthorization, delegates to MediatR in src/Web/Endpoints/ExerciseTemplates.cs
- [x] T021 [P] [US1] Create WorkoutTemplates endpoint group extending EndpointGroupBase, map GET / RequireAuthorization, delegates to MediatR in src/Web/Endpoints/WorkoutTemplates.cs

### Tests for User Story 1

- [x] T022 [P] [US1] Write functional tests for GetExerciseTemplates: test returns user's exercises only, test search filters by name, test filter by ImplementType, test filter by ExerciseType, test combined search+filter, test excludes soft-deleted exercises, test empty result for user with no exercises in tests/Application.FunctionalTests/ExerciseTemplates/Queries/GetExerciseTemplatesTests.cs
- [x] T023 [P] [US1] Write functional tests for GetWorkoutTemplates: test returns user's templates only, test returns exercise count, test excludes other user's templates, test empty result in tests/Application.FunctionalTests/WorkoutTemplates/Queries/GetWorkoutTemplatesTests.cs

### Mobile for User Story 1

- [x] T024 [P] [US1] Create exercise templates API service with getExerciseTemplates(search?, implementType?, exerciseType?) function using useApi hook pattern in src/App/services/exercise-templates.ts
- [x] T025 [P] [US1] Create workout templates API service with getWorkoutTemplates() function using useApi hook pattern in src/App/services/workout-templates.ts
- [x] T026 [P] [US1] Create WorkoutTemplateList component displaying workout templates by name with created date, onPress navigates to detail, empty state prompt in src/App/components/WorkoutTemplateList.tsx
- [x] T027 [P] [US1] Create ExerciseTemplateList component with search TextInput, implement type filter picker, exercise type filter picker, list of exercises with name/implement/type, empty state prompt in src/App/components/ExerciseTemplateList.tsx
- [x] T028 [US1] Update landing page to display WorkoutTemplateList and ExerciseTemplateList sections, load both API calls in parallel on mount, use theme colors from constants/theme.ts in src/App/app/(app)/index.tsx

**Checkpoint**: User Story 1 complete — landing page displays workout templates and exercise library with search/filter. Independently testable.

---

## Phase 4: User Story 2 — Create and Edit Workout Templates (Priority: P1)

**Goal**: User can create a new workout template with name/notes/location and edit existing ones. Workout templates can be deleted.

**Independent Test**: Create a workout template → verify it appears in list → edit name/notes/location → verify changes persist → delete → verify removed.

### Backend for User Story 2

- [x] T029 [P] [US2] Create CreateWorkoutTemplate command (Name, Notes?, Location?), validator (Name not empty, max 200; Notes max 2000; Location max 200), handler creates entity with UserId from IUser, returns int Id in src/Application/WorkoutTemplates/Commands/CreateWorkoutTemplate/
- [x] T030 [P] [US2] Create UpdateWorkoutTemplate command (Id, Name, Notes?, Location?), validator (same rules + Id > 0), handler loads by Id and UserId, throws NotFoundException if not found/not owned, updates fields in src/Application/WorkoutTemplates/Commands/UpdateWorkoutTemplate/
- [x] T031 [P] [US2] Create DeleteWorkoutTemplate command (Id), handler loads by Id and UserId, throws NotFoundException if not found/not owned, removes entity (cascade deletes WorkoutTemplateExercise rows) in src/Application/WorkoutTemplates/Commands/DeleteWorkoutTemplate/
- [x] T032 [P] [US2] Create GetWorkoutTemplate query (Id), handler loads by Id and UserId with Include for exercises (ordered by Position) and their ExerciseTemplate, projects to WorkoutTemplateDetailDto, throws NotFoundException if not found/not owned in src/Application/WorkoutTemplates/Queries/GetWorkoutTemplate/GetWorkoutTemplate.cs
- [x] T033 [US2] Add POST, PUT /{id}, DELETE /{id}, and GET /{id} routes to WorkoutTemplates endpoint group, all RequireAuthorization, POST returns Created, PUT/DELETE return NoContent in src/Web/Endpoints/WorkoutTemplates.cs

### Tests for User Story 2

- [x] T034 [P] [US2] Write functional tests for CreateWorkoutTemplate: test creates with valid data, test validation fails without name, test sets created/modified dates, test associates with current user in tests/Application.FunctionalTests/WorkoutTemplates/Commands/CreateWorkoutTemplateTests.cs
- [x] T035 [P] [US2] Write functional tests for UpdateWorkoutTemplate: test updates name/notes/location, test validation fails with empty name, test returns 404 for other user's template, test updates modified date in tests/Application.FunctionalTests/WorkoutTemplates/Commands/UpdateWorkoutTemplateTests.cs
- [x] T036 [P] [US2] Write functional tests for DeleteWorkoutTemplate: test deletes template and associations, test returns 404 for other user's template, test exercises remain in library after workout deletion in tests/Application.FunctionalTests/WorkoutTemplates/Commands/DeleteWorkoutTemplateTests.cs
- [x] T037 [P] [US2] Write functional test for GetWorkoutTemplate: test returns detail with exercises, test returns 404 for not found/not owned in tests/Application.FunctionalTests/WorkoutTemplates/Queries/GetWorkoutTemplateTests.cs

### Mobile for User Story 2

- [x] T038 [P] [US2] Add createWorkoutTemplate(name, notes?, location?), updateWorkoutTemplate(id, name, notes?, location?), deleteWorkoutTemplate(id), getWorkoutTemplate(id) to workout templates API service in src/App/services/workout-templates.ts
- [x] T039 [P] [US2] Create workout template create screen with name input (required), notes input (optional, multiline), location input (optional), save button, validation errors display, navigates back on success in src/App/app/(app)/workout-templates/create.tsx
- [x] T040 [US2] Create workout template detail/edit screen that loads template by id, displays/edits name, notes, location, shows exercise list (read-only for now), delete button with confirmation alert, save button with validation in src/App/app/(app)/workout-templates/[id].tsx

**Checkpoint**: User Story 2 complete — full CRUD for workout templates. Independently testable.

---

## Phase 5: User Story 3 — Manage Exercise Templates in Workout Templates (Priority: P1)

**Goal**: While editing a workout template, user can add exercises from library (or create new inline), remove exercises, and view ordered exercise list.

**Independent Test**: Open workout template → add exercise from library → create new exercise inline → remove an exercise → verify list order and persistence.

### Backend for User Story 3

- [x] T041 [US3] Create UpdateWorkoutTemplateExercises command (WorkoutTemplateId, Exercises list of {ExerciseTemplateId}), validator (WorkoutTemplateId > 0, each ExerciseTemplateId > 0), handler loads workout by Id and UserId, validates each exercise exists and is owned by user, replaces all WorkoutTemplateExercise rows with new list using array-index-based Position values in src/Application/WorkoutTemplates/Commands/UpdateWorkoutTemplateExercises/
- [x] T042 [US3] Add PUT /{id}/Exercises route to WorkoutTemplates endpoint group, RequireAuthorization, returns NoContent in src/Web/Endpoints/WorkoutTemplates.cs

### Tests for User Story 3

- [x] T043 [P] [US3] Write functional tests for UpdateWorkoutTemplateExercises: test replaces exercise list, test allows duplicate exercises, test validates exercise ownership, test returns 404 for other user's workout, test preserves position ordering, test empty list removes all exercises in tests/Application.FunctionalTests/WorkoutTemplates/Commands/UpdateWorkoutTemplateExercisesTests.cs

### Mobile for User Story 3

- [x] T044 [P] [US3] Add updateWorkoutTemplateExercises(workoutTemplateId, exerciseIds[]) to workout templates API service in src/App/services/workout-templates.ts
- [x] T045 [P] [US3] Create ExercisePickerModal component: modal overlay showing exercise library list with search/filter (reuse ExerciseTemplateList pattern), select exercise to add, option to create new exercise inline (navigates to create exercise screen, returns with new exercise), dismiss modal in src/App/components/ExercisePickerModal.tsx
- [x] T046 [US3] Update workout template detail/edit screen to enable exercise management: display ordered exercise list with remove buttons, "Add Exercise" button opens ExercisePickerModal, save exercise list changes via updateWorkoutTemplateExercises API, edit exercise taps navigate to exercise detail screen in src/App/app/(app)/workout-templates/[id].tsx

**Checkpoint**: User Story 3 complete — exercises can be added/removed from workout templates. Independently testable.

---

## Phase 6: User Story 4 — Create and Edit Exercise Templates (Priority: P2)

**Goal**: User can create and edit exercise templates with all fields (name, implement, type, image, model) from the exercise library.

**Independent Test**: Create exercise with all required fields → verify in library → edit fields → upload image → verify image displays → soft-delete → verify hidden from library.

### Backend for User Story 4

- [x] T047 [P] [US4] Create CreateExerciseTemplate command (Name, ImplementType, ExerciseType, Model?), validator (Name not empty, max 200, unique per user via async DB check; ImplementType/ExerciseType must be valid enum; Model max 500), handler creates entity with UserId from IUser, returns int Id in src/Application/ExerciseTemplates/Commands/CreateExerciseTemplate/
- [x] T048 [P] [US4] Create UpdateExerciseTemplate command (Id, Name, ImplementType, ExerciseType, Model?), validator (same rules as create, unique name check excludes self), handler loads by Id and UserId, throws NotFoundException if not found/not owned, updates fields in src/Application/ExerciseTemplates/Commands/UpdateExerciseTemplate/
- [x] T049 [P] [US4] Create DeleteExerciseTemplate command (Id), handler loads by Id and UserId, throws NotFoundException if not found/not owned, sets IsDeleted=true and DeletedAt=now in src/Application/ExerciseTemplates/Commands/DeleteExerciseTemplate/
- [x] T050 [P] [US4] Create GetExerciseTemplate query (Id), handler loads by Id and UserId, projects to ExerciseTemplateDetailDto, throws NotFoundException if not found/not owned/soft-deleted in src/Application/ExerciseTemplates/Queries/GetExerciseTemplate/GetExerciseTemplate.cs
- [x] T051 [P] [US4] Create UploadExerciseImage command (Id, Stream, FileName, ContentType), validator (file required, max 5MB, content type must be image/jpeg, image/png, or image/webp), handler saves file to wwwroot/uploads/exercises/ with unique filename, updates ImagePath on entity in src/Application/ExerciseTemplates/Commands/UploadExerciseImage/
- [x] T052 [P] [US4] Create DeleteExerciseImage command (Id), handler loads exercise by Id and UserId, deletes file from disk if exists, clears ImagePath on entity in src/Application/ExerciseTemplates/Commands/DeleteExerciseImage/
- [x] T053 [US4] Add POST, PUT /{id}, DELETE /{id}, GET /{id}, POST /{id}/Image (multipart form), DELETE /{id}/Image routes to ExerciseTemplates endpoint group, all RequireAuthorization in src/Web/Endpoints/ExerciseTemplates.cs
- [x] T054 [US4] Create wwwroot/uploads/exercises/ directory and add .gitkeep file, ensure directory exists on app startup in src/Web/

### Tests for User Story 4

- [x] T055 [P] [US4] Write functional tests for CreateExerciseTemplate: test creates with valid data, test validation fails without name/implement/type, test enforces unique name per user, test allows same name for different users in tests/Application.FunctionalTests/ExerciseTemplates/Commands/CreateExerciseTemplateTests.cs
- [x] T056 [P] [US4] Write functional tests for UpdateExerciseTemplate: test updates all fields, test validation fails with empty name, test enforces unique name excluding self, test returns 404 for other user's exercise in tests/Application.FunctionalTests/ExerciseTemplates/Commands/UpdateExerciseTemplateTests.cs
- [x] T057 [P] [US4] Write functional tests for DeleteExerciseTemplate: test soft-deletes (IsDeleted=true, DeletedAt set), test soft-deleted exercise excluded from GetExerciseTemplates, test soft-deleted exercise still visible in workout template detail, test returns 404 for other user's exercise in tests/Application.FunctionalTests/ExerciseTemplates/Commands/DeleteExerciseTemplateTests.cs
- [x] T058 [P] [US4] Write functional test for GetExerciseTemplate: test returns detail, test returns 404 for not found/not owned/soft-deleted in tests/Application.FunctionalTests/ExerciseTemplates/Queries/GetExerciseTemplateTests.cs

### Mobile for User Story 4

- [x] T059 [P] [US4] Add createExerciseTemplate(name, implementType, exerciseType, model?), updateExerciseTemplate(id, name, implementType, exerciseType, model?), deleteExerciseTemplate(id), getExerciseTemplate(id), uploadExerciseImage(id, file), deleteExerciseImage(id) to exercise templates API service in src/App/services/exercise-templates.ts
- [x] T060 [P] [US4] Create exercise template create screen with name input, implement type picker (9 options), exercise type picker (3 options), model input (optional), save button, validation errors, navigates back on success in src/App/app/(app)/exercise-templates/create.tsx
- [x] T061 [US4] Create exercise template detail/edit screen that loads by id, displays/edits all fields, image upload via device camera/gallery using expo-image-picker, delete image button, soft-delete button with confirmation, save button with validation in src/App/app/(app)/exercise-templates/[id].tsx

**Checkpoint**: User Story 4 complete — full exercise template CRUD with image upload and soft-delete. Independently testable.

---

## Phase 7: User Story 5 — Reorder Exercises Within a Workout Template (Priority: P3)

**Goal**: User can drag-reorder exercises in a workout template and the order persists on save.

**Independent Test**: Open workout template with 3+ exercises → reorder via drag → save → reload → verify new order persists.

### Mobile for User Story 5

- [x] T062 [US5] Add drag-to-reorder capability to the exercise list in workout template detail/edit screen using react-native-gesture-handler and react-native-reanimated (already installed), update local exercise array order, submit reordered list via existing updateWorkoutTemplateExercises API on save in src/App/app/(app)/workout-templates/[id].tsx

**Checkpoint**: User Story 5 complete — exercise reordering works. Independently testable. No backend changes needed — reorder uses existing UpdateWorkoutTemplateExercises command.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T063 [P] Update (app) layout to register workout-templates and exercise-templates routes in src/App/app/(app)/_layout.tsx
- [x] T064 [P] Add JSON string enum serialization for ImplementType and ExerciseType so API responses return "Barbell" not 0 — configure in src/Web/DependencyInjection.cs or Program.cs JSON options
- [x] T065 [P] Write unit tests for CreateWorkoutTemplate, UpdateWorkoutTemplate, CreateExerciseTemplate, UpdateExerciseTemplate validators in tests/Application.UnitTests/
- [x] T066 Run full test suite with `dotnet test` and fix any failures
- [x] T067 Run `dotnet build -tl` to verify no warnings (TreatWarningsAsErrors)
- [x] T068 Run `cd src/App && npx expo lint` to verify no ESLint errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion (entities and DbSets must exist for DTOs)
- **User Stories (Phases 3-7)**: All depend on Phase 2 completion
  - US1 (Phase 3): No dependencies on other stories
  - US2 (Phase 4): No dependencies on other stories (can run parallel with US1)
  - US3 (Phase 5): Depends on US2 backend (needs workout template CRUD) and US4 backend (needs exercise create for inline creation)
  - US4 (Phase 6): No dependencies on other stories (can run parallel with US1, US2)
  - US5 (Phase 7): Depends on US3 (needs exercise list in workout template edit screen)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup) → Phase 2 (Foundational)
                       ↓
              ┌────────┼────────┐
              ↓        ↓        ↓
           US1(P3)  US2(P4)  US4(P6)     ← Can run in parallel
              │        │        │
              │        ↓        │
              │     US3(P5) ←───┘         ← Needs US2 + US4 backend
              │        ↓
              │     US5(P7)               ← Needs US3 mobile
              │        │
              └────────┴─→ Phase 8 (Polish)
```

### Within Each User Story

- Backend commands/queries before endpoints
- Endpoints before tests (tests hit the API)
- Mobile services before components
- Components before screens
- All backend [P] tasks within a phase can run in parallel
- All mobile [P] tasks within a phase can run in parallel

### Parallel Opportunities

- T001-T005 (all domain entities/enums) can run in parallel
- T008-T010 (all EF configurations) can run in parallel
- T013-T016 (all DTOs) can run in parallel
- T018-T021 (US1 backend) can run in parallel
- T024-T027 (US1 mobile) can run in parallel
- T029-T032 (US2 backend commands/queries) can run in parallel
- T047-T052 (US4 backend) can run in parallel
- US1, US2, and US4 can be worked on simultaneously by different agents

---

## Parallel Example: User Story 1

```bash
# Backend — launch all in parallel:
Task: T018 "GetExerciseTemplates query"
Task: T019 "GetWorkoutTemplates query"
Task: T020 "ExerciseTemplates endpoint GET"
Task: T021 "WorkoutTemplates endpoint GET"

# Tests — launch in parallel:
Task: T022 "GetExerciseTemplates functional tests"
Task: T023 "GetWorkoutTemplates functional tests"

# Mobile — launch services in parallel, then components, then screen:
Task: T024 "exercise-templates.ts service"
Task: T025 "workout-templates.ts service"
# then:
Task: T026 "WorkoutTemplateList component"
Task: T027 "ExerciseTemplateList component"
# then:
Task: T028 "Landing page update"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T012)
2. Complete Phase 2: Foundational (T013-T017)
3. Complete Phase 3: User Story 1 (T018-T028)
4. **STOP and VALIDATE**: Log in → landing page shows workout templates and exercises with search/filter
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Landing page works (MVP!)
3. Add US2 → Can create/edit/delete workout templates
4. Add US4 → Can create/edit exercise templates with images (can run parallel with US2)
5. Add US3 → Can manage exercises within workout templates
6. Add US5 → Can reorder exercises
7. Polish → Tests, linting, cleanup

### Parallel Agent Strategy

With multiple agents:

1. All agents complete Setup + Foundational together
2. Once Foundational is done:
   - Agent A: User Story 1 (landing page)
   - Agent B: User Story 2 (workout CRUD)
   - Agent C: User Story 4 (exercise CRUD)
3. After B and C complete:
   - Agent A or B: User Story 3 (exercise management in workouts)
4. After US3:
   - Any agent: User Story 5 (reorder)
5. All agents: Polish phase

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable after its phase
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- The UpdateWorkoutTemplateExercises command (T041) handles add, remove, AND reorder in one operation — the client submits the full ordered list
- Image upload is a separate endpoint (POST /{id}/Image) to keep exercise creation simple
- Soft-deleted exercises need IgnoreQueryFilters() when loading workout template exercises to show referenced deleted exercises
