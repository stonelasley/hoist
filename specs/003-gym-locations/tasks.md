# Tasks: Gym Locations

**Input**: Design documents from `/specs/003-gym-locations/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in the feature specification — test tasks are omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new project setup needed — this feature extends the existing solution. This phase is empty.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the Location domain entity, EF Core configuration, DbContext registration, and modify WorkoutTemplate/ExerciseTemplate entities to use LocationId FK. These changes are required before ANY user story can be implemented.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T001 Create Location domain entity with Name, InstagramHandle, Latitude, Longitude, Notes, Address, UserId, IsDeleted, DeletedAt extending BaseAuditableEntity in `src/Domain/Entities/Location.cs`
- [x] T002 Modify WorkoutTemplate entity: replace `Location` (string?) property with `LocationId` (int?) FK and `Location` (Location?) navigation property in `src/Domain/Entities/WorkoutTemplate.cs`
- [x] T003 Modify ExerciseTemplate entity: add `LocationId` (int?) FK and `Location` (Location?) navigation property in `src/Domain/Entities/ExerciseTemplate.cs`
- [x] T004 Add `DbSet<Location> Locations` to IApplicationDbContext interface in `src/Application/Common/Interfaces/IApplicationDbContext.cs`
- [x] T005 Add `DbSet<Location> Locations` to ApplicationDbContext in `src/Infrastructure/Data/ApplicationDbContext.cs`
- [x] T006 Create LocationConfiguration with property constraints (Name max 200, InstagramHandle max 30, Latitude decimal(9,6), Longitude decimal(10,6), Notes max 2000, Address max 500), IsDeleted default false, global query filter `e => !e.IsDeleted`, composite index on (UserId, IsDeleted), and FK to ApplicationUser with Cascade delete in `src/Infrastructure/Data/Configurations/LocationConfiguration.cs`
- [x] T007 Update WorkoutTemplateConfiguration: remove `Location` string property config, add `LocationId` FK to Location with SetNull delete behavior and FK index in `src/Infrastructure/Data/Configurations/WorkoutTemplateConfiguration.cs`
- [x] T008 Update ExerciseTemplateConfiguration: add `LocationId` FK to Location with SetNull delete behavior and FK index in `src/Infrastructure/Data/Configurations/ExerciseTemplateConfiguration.cs`
- [x] T009 Update seed data in ApplicationDbContextInitialiser.TrySeedAsync(): create two Location entities ("The Refinery" with InstagramHandle "therefinerysc" and Notes "Main gym"; "Home Gym" with Notes "Garage setup") for test user, associate "Push Day" workout template with "The Refinery", associate "Bench Press" and "Squat" exercise templates with "The Refinery" in `src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`
- [x] T010 Verify solution builds cleanly with `dotnet build -tl` — fix any compilation errors from entity changes

**Checkpoint**: Foundation ready — Location entity exists, FKs configured, DB recreates cleanly on startup. User story implementation can begin.

---

## Phase 3: User Story 1 — Create and Manage Locations (Priority: P1) MVP

**Goal**: Users can create, view, edit, and soft-delete locations from a "My Locations" screen under settings. Full backend CRUD API + mobile screens.

**Independent Test**: Create a location with name and optional details, verify it appears in the list, edit it, delete it and verify it's hidden.

### Backend — Location CRUD Commands & Queries

- [x] T011 [P] [US1] Create CreateLocationCommand (record with Name, InstagramHandle, Latitude, Longitude, Notes, Address), handler that strips leading "@" from InstagramHandle, sets UserId from IUser, and returns int Id. Create in `src/Application/Locations/Commands/CreateLocation/CreateLocation.cs`
- [x] T012 [P] [US1] Create CreateLocationCommandValidator with rules: Name NotEmpty MaxLength(200), InstagramHandle MaxLength(30), Latitude InclusiveBetween(-90,90) when not null, Longitude InclusiveBetween(-180,180) when not null, Latitude required when Longitude provided and vice versa, Notes MaxLength(2000), Address MaxLength(500) in `src/Application/Locations/Commands/CreateLocation/CreateLocationCommandValidator.cs`
- [x] T013 [P] [US1] Create UpdateLocationCommand (record with Id, Name, InstagramHandle, Latitude, Longitude, Notes, Address), handler that finds location by Id+UserId, guards NotFound, strips "@" from InstagramHandle, updates all fields. Create in `src/Application/Locations/Commands/UpdateLocation/UpdateLocation.cs`
- [x] T014 [P] [US1] Create UpdateLocationCommandValidator with same rules as CreateLocationCommandValidator plus Id validation in `src/Application/Locations/Commands/UpdateLocation/UpdateLocationCommandValidator.cs`
- [x] T015 [P] [US1] Create DeleteLocationCommand (record with Id), handler that finds location by Id+UserId, guards NotFound, sets IsDeleted=true and DeletedAt=DateTimeOffset.UtcNow in `src/Application/Locations/Commands/DeleteLocation/DeleteLocation.cs`
- [x] T016 [P] [US1] Create LocationBriefDto with Id, Name, InstagramHandle, Latitude, Longitude, Notes, Address, Created, LastModified and AutoMapper Profile mapping from Location in `src/Application/Locations/Queries/GetLocations/LocationBriefDto.cs`
- [x] T017 [P] [US1] Create GetLocationsQuery (record, no params), handler that returns List<LocationBriefDto> filtered by UserId, ordered by Name, using ProjectTo in `src/Application/Locations/Queries/GetLocations/GetLocations.cs`
- [x] T018 [P] [US1] Create GetLocationQuery (record with Id), handler that finds by Id+UserId, guards NotFound, returns LocationBriefDto in `src/Application/Locations/Queries/GetLocation/GetLocation.cs`

### Backend — Locations Endpoint

- [x] T019 [US1] Create Locations endpoint group extending EndpointGroupBase in `src/Web/Endpoints/Locations.cs`. Map routes: GET → GetLocations, GET {id} → GetLocation, POST → CreateLocation (returns Created), PUT {id} → UpdateLocation (returns NoContent, validates id match), DELETE {id} → DeleteLocation (returns NoContent). All routes require authorization.

### Mobile — Locations API Service

- [x] T020 [US1] Create locations API service with types (LocationDto, CreateLocationRequest, UpdateLocationRequest) and functions (getLocations, getLocation, createLocation, updateLocation, deleteLocation) in `src/App/services/locations.ts`

### Mobile — Settings & Location Screens

- [x] T021 [US1] Create settings index screen with a "My Locations" navigation link in `src/App/app/(app)/settings/index.tsx`. Use theme tokens for styling.
- [x] T022 [US1] Add settings route to the app layout Stack in `src/App/app/(app)/_layout.tsx` — add Stack.Screen entries for settings/index, settings/locations/index, settings/locations/create, settings/locations/[id]
- [x] T023 [US1] Add a settings gear icon button to the landing page header that navigates to settings in `src/App/app/(app)/index.tsx`
- [x] T024 [US1] Create My Locations list screen: fetch locations via API, display in FlatList with name and optional details, empty state when no locations, "Add Location" button, tap row to navigate to edit screen, swipe-to-delete with confirmation alert that calls deleteLocation API in `src/App/app/(app)/settings/locations/index.tsx`
- [x] T025 [US1] Create location creation screen with form fields: Name (required TextInput), Instagram Handle (optional TextInput), Latitude (optional numeric TextInput), Longitude (optional numeric TextInput), Notes (optional multiline TextInput), Address (optional TextInput). Validate name not empty and coordinates in range before submit. Strip "@" from Instagram handle client-side. On save call createLocation API and navigate back in `src/App/app/(app)/settings/locations/create.tsx`
- [x] T026 [US1] Create location edit screen: load location by id param, pre-fill form with same fields as create, on save call updateLocation API and navigate back in `src/App/app/(app)/settings/locations/[id].tsx`

**Checkpoint**: Users can fully manage locations via Settings → My Locations. CRUD works end-to-end.

---

## Phase 4: User Story 2 — Associate Locations with Workout Templates (Priority: P1)

**Goal**: Users can select a location when creating/editing a workout template. The location name is displayed on the template. Includes inline location creation from the picker.

**Independent Test**: Edit a workout template, select a location from the picker, save, verify the location name appears on the template.

### Backend — Update WorkoutTemplate Commands & Queries

- [x] T027 [P] [US2] Update CreateWorkoutTemplateCommand: replace `Location` (string?) with `LocationId` (int?), update handler to set entity.LocationId. Update validator to remove Location MaxLength rule, add LocationId validation (if provided, must exist and belong to user) in `src/Application/WorkoutTemplates/Commands/CreateWorkoutTemplate/CreateWorkoutTemplate.cs` and `src/Application/WorkoutTemplates/Commands/CreateWorkoutTemplate/CreateWorkoutTemplateCommandValidator.cs`
- [x] T028 [P] [US2] Update UpdateWorkoutTemplateCommand: replace `Location` (string?) with `LocationId` (int?), update handler to set entity.LocationId. Update validator similarly in `src/Application/WorkoutTemplates/Commands/UpdateWorkoutTemplate/UpdateWorkoutTemplate.cs` and `src/Application/WorkoutTemplates/Commands/UpdateWorkoutTemplate/UpdateWorkoutTemplateCommandValidator.cs`
- [x] T029 [US2] Update WorkoutTemplateBriefDto: replace `Location` (string?) with `LocationId` (int?) and `LocationName` (string?). Update AutoMapper mapping to map LocationName from Location.Name (use IgnoreQueryFilters or include soft-deleted location for name display). Update any WorkoutTemplateDetailDto similarly in `src/Application/WorkoutTemplates/Queries/`

### Mobile — Location Picker Component

- [x] T030 [US2] Create LocationPickerModal component: receives visible, onClose, onSelect(locationId, locationName) props. Fetches user locations via getLocations(). Displays list with search. Includes "None" option to clear selection. Includes "Create New Location" option that navigates to inline creation form (embedded in modal or navigates to create screen). On select calls onSelect and closes in `src/App/components/LocationPickerModal.tsx`

### Mobile — Integrate Picker into Workout Template Screens

- [x] T031 [US2] Update workout template create screen: replace free-text Location TextInput with a tappable location field that opens LocationPickerModal. Display selected location name. Pass locationId to createWorkoutTemplate API call in `src/App/app/(app)/workout-templates/create.tsx`
- [x] T032 [US2] Update workout template edit screen: replace free-text Location TextInput with tappable location field + LocationPickerModal. Pre-select current location. Pass locationId to updateWorkoutTemplate API call in `src/App/app/(app)/workout-templates/[id].tsx`
- [x] T033 [US2] Update workout-templates service types: replace `location` (string?) with `locationId` (int?) and `locationName` (string?) in response DTO, replace `location` with `locationId` in request types in `src/App/services/workout-templates.ts`
- [x] T034 [US2] Update landing page workout template list items to display locationName when present in `src/App/app/(app)/index.tsx`

**Checkpoint**: Workout templates can be associated with locations. Location name displays on templates.

---

## Phase 5: User Story 3 — Associate Locations with Exercise Templates (Priority: P2)

**Goal**: Users can select a location when creating/editing an exercise template. Same picker pattern as workout templates.

**Independent Test**: Edit an exercise template, select a location, save, verify the location name appears.

### Backend — Update ExerciseTemplate Commands & Queries

- [x] T035 [P] [US3] Update CreateExerciseTemplateCommand: add `LocationId` (int?) property, update handler to set entity.LocationId. Update validator to add LocationId validation (if provided, must exist and belong to user) in `src/Application/ExerciseTemplates/Commands/CreateExerciseTemplate/`
- [x] T036 [P] [US3] Update UpdateExerciseTemplateCommand: add `LocationId` (int?) property, update handler and validator similarly in `src/Application/ExerciseTemplates/Commands/UpdateExerciseTemplate/`
- [x] T037 [US3] Update ExerciseTemplateBriefDto: add `LocationId` (int?) and `LocationName` (string?). Update AutoMapper mapping to map LocationName from Location.Name (handle soft-deleted locations). Update any ExerciseTemplateDetailDto similarly in `src/Application/ExerciseTemplates/Queries/`

### Mobile — Integrate Picker into Exercise Template Screens

- [x] T038 [US3] Update exercise template create screen: add tappable location field that opens LocationPickerModal, pass locationId to createExerciseTemplate API call in `src/App/app/(app)/exercise-templates/create.tsx`
- [x] T039 [US3] Update exercise template edit screen: add tappable location field + LocationPickerModal, pre-select current location, pass locationId to updateExerciseTemplate API call in `src/App/app/(app)/exercise-templates/[id].tsx`
- [x] T040 [US3] Update exercise-templates service types: add `locationId` (int?) and `locationName` (string?) to response DTO, add `locationId` to request types in `src/App/services/exercise-templates.ts`

**Checkpoint**: Exercise templates can be associated with locations. Location name displays on templates.

---

## Phase 6: User Story 4 — Filter Workout Templates by Location (Priority: P1)

**Goal**: Users can filter workout templates on the landing page by location. Only matching templates are shown.

**Independent Test**: Have workout templates at different locations, apply a location filter, verify only matching templates appear.

### Backend — Add Location Filter to GetWorkoutTemplates

- [x] T041 [US4] Add `LocationId` (int?) property to GetWorkoutTemplatesQuery. Update handler to add `.Where(x => x.LocationId == request.LocationId.Value)` when LocationId is provided. Update endpoint to accept locationId as query parameter via [AsParameters] in `src/Application/WorkoutTemplates/Queries/GetWorkoutTemplates/GetWorkoutTemplates.cs` and `src/Web/Endpoints/WorkoutTemplates.cs`

### Mobile — Location Filter on Landing Page

- [x] T042 [US4] Add location filter UI to the workout templates section of the landing page: fetch user's locations for filter options, display as horizontal scrollable chip selector (matching existing implement/exercise type filter pattern), "All Locations" chip to clear filter, pass selected locationId as query param when fetching workout templates in `src/App/app/(app)/index.tsx`

**Checkpoint**: Workout templates can be filtered by location on the landing page.

---

## Phase 7: User Story 5 — Filter Exercise Templates by Location (Priority: P2)

**Goal**: Users can filter exercise templates by location in both the exercise library on the landing page and the exercise picker modal.

**Independent Test**: Have exercises at different locations, apply a location filter in the exercise library, verify only matching exercises appear.

### Backend — Add Location Filter to GetExerciseTemplates

- [x] T043 [US5] Add `LocationId` (int?) property to GetExerciseTemplatesQuery. Update handler to add `.Where(x => x.LocationId == request.LocationId.Value)` when LocationId is provided. Update endpoint to accept locationId query parameter in `src/Application/ExerciseTemplates/Queries/GetExerciseTemplates/GetExerciseTemplates.cs` and `src/Web/Endpoints/ExerciseTemplates.cs`

### Mobile — Location Filter in Exercise Library & Picker

- [x] T044 [US5] Add location filter chip to the exercise library section of the landing page: same horizontal chip pattern as workout template filter, pass locationId when fetching exercises in `src/App/app/(app)/index.tsx`
- [x] T045 [US5] Add location filter chip to ExercisePickerModal: add location chip row alongside existing implement type and exercise type chips, filter exercises by selected location (either client-side from already-fetched data or via API param) in `src/App/components/ExercisePickerModal.tsx`

**Checkpoint**: Exercise templates can be filtered by location in both the library and exercise picker.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup across all user stories.

- [x] T046 Verify full solution builds cleanly with `dotnet build -tl`
- [x] T047 Run existing test suite with `dotnet test` to confirm no regressions from entity changes
- [x] T048 Run mobile app linter with `npx expo lint` from `src/App/` and fix any issues
- [x] T049 Manual end-to-end validation (see validation steps below) following quickstart.md test flow: log in as test@test.com, navigate to Settings → My Locations, create "The Refinery", associate with a workout template, filter workouts by location on landing page

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately. BLOCKS all user stories.
- **US1 (Phase 3)**: Depends on Phase 2 completion
- **US2 (Phase 4)**: Depends on Phase 2 + Phase 3 (needs Location CRUD API and LocationPickerModal)
- **US3 (Phase 5)**: Depends on Phase 2 + Phase 3 (reuses LocationPickerModal from US2, but only needs the Location API from US1)
- **US4 (Phase 6)**: Depends on Phase 2 + Phase 4 (needs LocationId on WorkoutTemplate and locations to filter by)
- **US5 (Phase 7)**: Depends on Phase 2 + Phase 5 (needs LocationId on ExerciseTemplate and locations to filter by)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

```
Phase 2 (Foundation)
  └── US1 (Location CRUD) ← MVP
        ├── US2 (Workout Template + Location) ← depends on US1 for picker
        │     └── US4 (Filter Workouts by Location) ← depends on US2 for LocationId on WorkoutTemplate
        └── US3 (Exercise Template + Location) ← depends on US1 for picker
              └── US5 (Filter Exercises by Location) ← depends on US3 for LocationId on ExerciseTemplate
```

### Parallel Opportunities

Within Phase 2:
- T002 and T003 (entity modifications) can run in parallel
- T004 and T005 (DbContext changes) can run in parallel
- T007 and T008 (configuration updates) can run in parallel

Within US1 (Phase 3):
- T011–T018 (all backend commands/queries) can run in parallel — different files
- T024, T025, T026 (mobile screens) can run in parallel — different files

Within US2 (Phase 4):
- T027 and T028 (create/update command changes) can run in parallel

Within US3 (Phase 5):
- T035 and T036 (create/update command changes) can run in parallel

US2 + US3 can run in parallel after US1 completes (different entity files).
US4 + US5 can run in parallel after their respective dependencies complete.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (Location entity + FK changes)
2. Complete Phase 3: US1 (Location CRUD — backend + mobile)
3. **STOP and VALIDATE**: Create and manage locations via Settings → My Locations
4. Deploy/demo if ready

### Incremental Delivery

1. Phase 2 → Foundation ready
2. US1 → Location CRUD works → MVP
3. US2 → Workout templates have location association → Core value
4. US4 → Filter workouts by location → Primary user request fulfilled
5. US3 → Exercise templates have location association
6. US5 → Filter exercises by location → Feature complete
7. Phase 8 → Polish and verify

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Commit after each completed phase
- The LocationPickerModal (T030) is created in US2 and reused in US3 — no duplication
- Instagram handle "@" stripping happens in both backend handler and mobile client for consistency
- Soft-deleted locations must still show their name on associated templates — use IgnoreQueryFilters or eager-load navigation property before filtering
