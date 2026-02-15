# Quickstart: Workout & Exercise Templates

**Feature Branch**: `002-workout-exercise-templates`
**Date**: 2026-02-14

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
# Exercise Template commands
dotnet new ca-usecase --name CreateExerciseTemplate --feature-name ExerciseTemplates --usecase-type command --return-type int
dotnet new ca-usecase --name UpdateExerciseTemplate --feature-name ExerciseTemplates --usecase-type command
dotnet new ca-usecase --name DeleteExerciseTemplate --feature-name ExerciseTemplates --usecase-type command

# Exercise Template queries
dotnet new ca-usecase -n GetExerciseTemplates -fn ExerciseTemplates -ut query -rt "List<ExerciseTemplateBriefDto>"
dotnet new ca-usecase -n GetExerciseTemplate -fn ExerciseTemplates -ut query -rt ExerciseTemplateDetailDto

# Workout Template commands
dotnet new ca-usecase --name CreateWorkoutTemplate --feature-name WorkoutTemplates --usecase-type command --return-type int
dotnet new ca-usecase --name UpdateWorkoutTemplate --feature-name WorkoutTemplates --usecase-type command
dotnet new ca-usecase --name DeleteWorkoutTemplate --feature-name WorkoutTemplates --usecase-type command
dotnet new ca-usecase --name UpdateWorkoutTemplateExercises --feature-name WorkoutTemplates --usecase-type command

# Workout Template queries
dotnet new ca-usecase -n GetWorkoutTemplates -fn WorkoutTemplates -ut query -rt "List<WorkoutTemplateBriefDto>"
dotnet new ca-usecase -n GetWorkoutTemplate -fn WorkoutTemplates -ut query -rt WorkoutTemplateDetailDto
```

### Run tests

```bash
dotnet test                                          # All tests
dotnet test tests/Domain.UnitTests                   # Domain unit tests
dotnet test tests/Application.FunctionalTests        # Functional tests (needs Docker)
dotnet test --filter "FullyQualifiedName~ExerciseTemplates"  # Feature-specific
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
- `app/(app)/index.tsx` — Landing page (modify existing)
- `app/(app)/workout-templates/[id].tsx` — Workout template detail/edit
- `app/(app)/workout-templates/create.tsx` — Create workout template
- `app/(app)/exercise-templates/[id].tsx` — Exercise template detail/edit
- `app/(app)/exercise-templates/create.tsx` — Create exercise template

**New services**:
- `services/workout-templates.ts` — API calls for workout templates
- `services/exercise-templates.ts` — API calls for exercise templates

**New components**:
- `components/WorkoutTemplateList.tsx` — List view for landing page
- `components/ExerciseTemplateList.tsx` — List view with search/filter
- `components/ExercisePickerModal.tsx` — Modal for selecting/adding exercises

### Run mobile tests

```bash
cd src/App
npm test
```

## Key Implementation Order

1. **Domain entities & enums** (ExerciseTemplate, WorkoutTemplate, WorkoutTemplateExercise, ImplementType, ExerciseType)
2. **Infrastructure** (DbContext, entity configurations, seed data)
3. **Application layer** (commands, queries, validators, DTOs)
4. **Web endpoints** (ExerciseTemplates, WorkoutTemplates endpoint groups)
5. **Backend tests** (unit + functional)
6. **Mobile services** (API client functions)
7. **Mobile screens** (landing page, create/edit screens, exercise picker)
8. **Mobile tests**
