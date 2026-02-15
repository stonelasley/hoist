# Quickstart: Workout Tracking

**Feature Branch**: `004-workout-tracking`
**Date**: 2026-02-15

## Prerequisites

- Docker running (for SQL Server via Aspire/Testcontainers)
- .NET 10 SDK
- Node.js 22+ with npm
- Expo CLI (`npx expo`)

## Backend Development

### Run the backend

```bash
dotnet run --project src/AppHost
```

This starts the Aspire orchestrator with SQL Server container + Web API. The database is auto-initialized with seed data on startup (dev mode drops and recreates).

### Scaffold new use cases

From `src/Application/`:

```bash
# Workout commands
dotnet new ca-usecase --name StartWorkout --feature-name Workouts --usecase-type command --return-type int
dotnet new ca-usecase --name CompleteWorkout --feature-name Workouts --usecase-type command
dotnet new ca-usecase --name UpdateWorkout --feature-name Workouts --usecase-type command
dotnet new ca-usecase --name DiscardWorkout --feature-name Workouts --usecase-type command
dotnet new ca-usecase --name UpdateWorkoutExercises --feature-name Workouts --usecase-type command
dotnet new ca-usecase --name CreateWorkoutSet --feature-name Workouts --usecase-type command --return-type int
dotnet new ca-usecase --name UpdateWorkoutSet --feature-name Workouts --usecase-type command
dotnet new ca-usecase --name DeleteWorkoutSet --feature-name Workouts --usecase-type command

# Workout queries
dotnet new ca-usecase -n GetInProgressWorkout -fn Workouts -ut query -rt WorkoutDetailDto
dotnet new ca-usecase -n GetRecentWorkouts -fn Workouts -ut query -rt "List<WorkoutBriefDto>"
dotnet new ca-usecase -n GetWorkoutHistory -fn Workouts -ut query -rt "PaginatedWorkoutList"
dotnet new ca-usecase -n GetWorkout -fn Workouts -ut query -rt WorkoutDetailDto

# User Preferences commands
dotnet new ca-usecase --name UpsertUserPreferences --feature-name UserPreferences --usecase-type command

# User Preferences queries
dotnet new ca-usecase -n GetUserPreferences -fn UserPreferences -ut query -rt UserPreferencesDto
```

### Run tests

```bash
dotnet test                                          # All tests
dotnet test tests/Domain.UnitTests                   # Domain unit tests
dotnet test tests/Application.FunctionalTests        # Functional tests (needs Docker)
dotnet test --filter "FullyQualifiedName~Workouts"   # Feature-specific
```

### API docs

After running the backend, Swagger UI is available at `https://localhost:5001/api`.

## Mobile Development

### Run the mobile app

```bash
cd src/App
npx expo start
```

### Key files to create/modify

**New screens** (file-based routing):
- `app/(app)/workouts/active.tsx` — Active workout session screen
- `app/(app)/workouts/complete.tsx` — Workout completion screen
- `app/(app)/workouts/[id].tsx` — View/edit completed workout
- `app/(app)/workouts/history.tsx` — Full workout history with sort/filter/search
- `app/(app)/settings/preferences.tsx` — Unit preferences and bodyweight settings

**Modify existing screens**:
- `app/(app)/index.tsx` — Landing page: add recent workouts section + in-progress workout banner
- `app/(app)/settings/index.tsx` — Add navigation to preferences screen

**New services**:
- `services/workouts.ts` — API calls for workouts, exercises within workouts, and sets
- `services/user-preferences.ts` — API calls for user preferences

**New components**:
- `components/RecentWorkoutsList.tsx` — Recent workouts card list for landing page
- `components/InProgressWorkoutBanner.tsx` — Banner for resuming in-progress workout
- `components/WorkoutExerciseCard.tsx` — Exercise card with set entry during active workout
- `components/SetEntryRow.tsx` — Row for entering/displaying a single set
- `components/WorkoutCompletionForm.tsx` — Notes, rating, time editing for completion
- `components/WorkoutHistoryList.tsx` — History list with sort/filter/search controls
- `components/RatingPicker.tsx` — 1-5 star rating selector
- `components/DurationPicker.tsx` — Duration input (mm:ss or hh:mm:ss)
- `components/BandColorPicker.tsx` — Band color selector using Colour values

### Run mobile tests

```bash
cd src/App
npm test
```

## Key Implementation Order

1. **Domain enums** (WorkoutStatus, WeightUnit, DistanceUnit)
2. **Domain entities** (UserPreferences, Workout, WorkoutExercise, WorkoutSet)
3. **Infrastructure** (DbContext DbSets, entity configurations, IApplicationDbContext interface updates)
4. **Application layer — UserPreferences** (get/upsert commands, queries, validators, DTOs)
5. **Application layer — Workouts** (start, complete, update, discard commands; get queries; validators, DTOs)
6. **Application layer — WorkoutSets** (create, update, delete set commands; validators)
7. **Web endpoints** (Workouts, UserPreferences endpoint groups)
8. **Backend tests** (unit + functional)
9. **Mobile services** (API client functions for workouts and preferences)
10. **Mobile screens — Settings** (preferences screen for units and bodyweight)
11. **Mobile screens — Active workout** (start, record sets, manage exercises)
12. **Mobile screens — Completion** (notes, rating, time editing)
13. **Mobile screens — Landing page** (recent workouts, in-progress banner)
14. **Mobile screens — History** (full history with sort/filter/search)
