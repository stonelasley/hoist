# Implementation Plan: Workout Tracking

**Branch**: `004-workout-tracking` | **Date**: 2026-02-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-workout-tracking/spec.md`

## Summary

Implement workout tracking as executions of workout templates. Users start a workout from a template, record sets with measurements appropriate to each exercise's type and implement (weight+reps, bodyweight+reps, bodyweight+duration, band+reps, band+duration, distance+duration), complete the workout with notes and rating, and view workout history. The backend adds 4 new entities (Workout, WorkoutExercise, WorkoutSet, UserPreferences), 3 new enums, CQRS commands/queries for the full workout lifecycle, and Minimal API endpoints. The mobile app adds an active workout session screen, completion flow, settings for unit preferences, recent workouts on the landing page, and a full history screen with sort/filter/search.

## Technical Context

**Language/Version**: C# / .NET 10.0 (backend), TypeScript 5.9 / React Native 0.81 (mobile)
**Primary Dependencies**: ASP.NET Core Identity, MediatR, FluentValidation, EF Core 10, AutoMapper (backend); Expo SDK 54, expo-router 6, React 19 (mobile)
**Storage**: SQL Server via EF Core (Aspire-orchestrated container in dev, Testcontainers in tests)
**Testing**: NUnit + Shouldly + Moq + Respawn + Testcontainers (backend)
**Target Platform**: iOS + Android (mobile), Linux/Windows server (backend API)
**Project Type**: Mobile + API
**Performance Goals**: Landing page with recent workouts <2s, set creation <1s, history search/filter <1s
**Constraints**: One in-progress workout per user, sets persisted immediately on creation, completed workouts fully editable, historical data survives template/exercise/location deletion via snapshots
**Scale/Scope**: Up to 1000 workouts per user with 10+ exercises and 50+ sets each; 5 new screens, ~16 new use cases, 4 new entities, 3 new enums

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Architecture | PASS | Entities in Domain, use cases in Application feature folders (Workouts/, UserPreferences/), EF config in Infrastructure, endpoints in Web |
| II. CQRS via MediatR | PASS | All operations as Commands (StartWorkout, CompleteWorkout, CreateWorkoutSet, UpsertUserPreferences, etc.) and Queries (GetInProgressWorkout, GetRecentWorkouts, GetWorkoutHistory). FluentValidation validators for all. |
| III. Domain-Driven Design | PASS | All entities extend BaseAuditableEntity. WorkoutStatus, WeightUnit, DistanceUnit as domain enums. Snapshot fields preserve domain integrity across deletions. |
| IV. API-First with Minimal APIs | PASS | Two new EndpointGroupBase subclasses: Workouts, UserPreferences. All require authorization. Nested routes for sets (Workouts/{id}/Exercises/{exerciseId}/Sets). |
| V. Test-Driven Quality | PASS | Unit tests for validators/handlers, functional tests for workout lifecycle flows with Testcontainers. |
| VI. Fitness Domain Integrity | PASS | Workouts serve the core track journey. Historical data is preserved. Completed workouts are editable (spec clarification aligns with constitution's edit policy). |
| VII. Simplicity and YAGNI | PASS | Single flat WorkoutSet entity (no polymorphic hierarchy). Simple enum for workout status. Lazy UserPreferences creation. No offline sync. |
| VIII. Mobile-First with Expo RN | PASS | File-based routing under app/(app)/workouts/. Functional components with hooks. Theme tokens from constants/theme.ts. Business logic delegated to API. |

**Post-Phase 1 Re-check**: All principles remain satisfied. No violations detected.

## Project Structure

### Documentation (this feature)

```text
specs/004-workout-tracking/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── workouts-api.md
│   └── user-preferences-api.md
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Entities/
│   │   ├── Workout.cs                  # New entity
│   │   ├── WorkoutExercise.cs          # New entity
│   │   ├── WorkoutSet.cs               # New entity
│   │   └── UserPreferences.cs          # New entity
│   └── Enums/
│       ├── WorkoutStatus.cs            # New enum
│       ├── WeightUnit.cs               # New enum
│       └── DistanceUnit.cs             # New enum
├── Application/
│   ├── Common/Interfaces/
│   │   └── IApplicationDbContext.cs     # Add new DbSets
│   ├── Workouts/
│   │   ├── Commands/
│   │   │   ├── StartWorkout/
│   │   │   ├── CompleteWorkout/
│   │   │   ├── UpdateWorkout/
│   │   │   ├── DiscardWorkout/
│   │   │   ├── UpdateWorkoutExercises/
│   │   │   ├── CreateWorkoutSet/
│   │   │   ├── UpdateWorkoutSet/
│   │   │   └── DeleteWorkoutSet/
│   │   └── Queries/
│   │       ├── GetInProgressWorkout/
│   │       ├── GetRecentWorkouts/
│   │       ├── GetWorkoutHistory/
│   │       └── GetWorkout/
│   └── UserPreferences/
│       ├── Commands/
│       │   └── UpsertUserPreferences/
│       └── Queries/
│           └── GetUserPreferences/
├── Infrastructure/
│   └── Data/
│       ├── ApplicationDbContext.cs           # Add new DbSets
│       ├── Configurations/
│       │   ├── WorkoutConfiguration.cs
│       │   ├── WorkoutExerciseConfiguration.cs
│       │   ├── WorkoutSetConfiguration.cs
│       │   └── UserPreferencesConfiguration.cs
│       └── ApplicationDbContextInitialiser.cs  # Add seed data (optional)
├── Web/
│   └── Endpoints/
│       ├── Workouts.cs                  # New endpoint group
│       └── UserPreferences.cs           # New endpoint group
└── App/
    ├── app/(app)/
    │   ├── index.tsx                     # Modify: add recent workouts + in-progress banner
    │   ├── workouts/
    │   │   ├── active.tsx               # New: active workout session
    │   │   ├── complete.tsx             # New: workout completion
    │   │   ├── [id].tsx                 # New: view/edit completed workout
    │   │   └── history.tsx              # New: full workout history
    │   └── settings/
    │       ├── index.tsx                # Modify: add preferences navigation
    │       └── preferences.tsx          # New: unit preferences + bodyweight
    ├── services/
    │   ├── workouts.ts                  # New: API client
    │   └── user-preferences.ts          # New: API client
    └── components/
        ├── RecentWorkoutsList.tsx        # New: recent workouts for landing page
        ├── InProgressWorkoutBanner.tsx   # New: resume in-progress workout
        ├── WorkoutExerciseCard.tsx       # New: exercise card with set entry
        ├── SetEntryRow.tsx              # New: single set input row
        ├── WorkoutCompletionForm.tsx     # New: completion form (notes, rating, times)
        ├── WorkoutHistoryList.tsx        # New: history with sort/filter/search
        ├── RatingPicker.tsx             # New: 1-5 star rating
        ├── DurationPicker.tsx           # New: duration input
        └── BandColorPicker.tsx          # New: band color selector

tests/
├── Domain.UnitTests/
│   └── (enum validation, entity defaults)
├── Application.FunctionalTests/
│   ├── Workouts/
│   │   ├── Commands/ (start, complete, update, discard, update exercises, create/update/delete set)
│   │   └── Queries/ (in-progress, recent, history with pagination/sort/filter/search, get detail)
│   └── UserPreferences/
│       ├── Commands/ (upsert)
│       └── Queries/ (get with defaults)
└── Application.UnitTests/
    └── (validator tests for all commands)
```

**Structure Decision**: Mobile + API pattern following the existing project structure. Backend code follows Clean Architecture layers already established. Mobile screens use file-based routing under the `(app)` group (authenticated). Workout screens nested under `workouts/` directory. No new projects are created — all code fits within existing solution structure.

## Complexity Tracking

| Decision | Principle Tension | Justification | Rejected Alternative |
|----------|------------------|---------------|---------------------|
| No domain events for workout lifecycle | III (DDD events) vs VII (YAGNI) | No event handlers exist — raising events with no consumers adds dead code. Audit trail is handled by BaseAuditableEntity interceptor. Events can be added when a consumer emerges (e.g., streak tracking, notifications). | Adding WorkoutCreatedEvent, WorkoutCompletedEvent with no handlers |
