# Implementation Plan: Gym Locations

**Branch**: `003-gym-locations` | **Date**: 2026-02-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-gym-locations/spec.md`

## Summary

Add a Location entity that users can create, edit, and soft-delete to represent gyms and training spaces. Workout templates and exercise templates gain an optional foreign key to Location (replacing the existing free-text location column on WorkoutTemplate). Both template types can be filtered by location on the landing page, exercise library, and exercise picker.

## Technical Context

**Language/Version**: C# / .NET 10.0 (backend), TypeScript 5.9 / React Native 0.81 (mobile)
**Primary Dependencies**: ASP.NET Core Identity, MediatR, FluentValidation, EF Core 10, AutoMapper (backend); Expo SDK 54, expo-router 6, React 19 (mobile)
**Storage**: SQL Server via EF Core (Aspire-orchestrated container in dev, Testcontainers in tests)
**Testing**: NUnit, Shouldly, Moq, Respawn, Testcontainers (backend); manual/Expo for mobile
**Target Platform**: iOS, Android, Web (Expo); .NET backend API
**Project Type**: Mobile + API
**Performance Goals**: Filter results display within 1 second; lists responsive with up to 50 locations
**Constraints**: Bearer token auth required for all endpoints; user data isolation (multi-tenant by UserId)
**Scale/Scope**: Up to 50 locations per user, referenced by up to 500 workout templates and 500 exercise templates per user

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Architecture | PASS | Location follows existing entity → Application CQRS → Infrastructure EF Core → Web endpoint flow |
| II. CQRS via MediatR | PASS | All operations expressed as Commands/Queries with FluentValidation |
| III. Domain-Driven Design | PASS | Location extends BaseAuditableEntity; soft-delete follows ExerciseTemplate pattern |
| IV. API-First with Minimal APIs | PASS | New Locations endpoint group via EndpointGroupBase |
| V. Test-Driven Quality | PASS | Unit tests for handlers, functional tests for API flows |
| VI. Fitness Domain Integrity | PASS | Locations serve the core user journey of organizing workouts by training venue |
| VII. Simplicity and YAGNI | PASS | Direct FK reference, no over-engineered location hierarchy or geolocation |
| VIII. Mobile-First with Expo React Native | PASS | Screens via expo-router, theme tokens, no business logic in mobile |

No gate violations. No complexity tracking needed.

## Project Structure

### Documentation (this feature)

```text
specs/003-gym-locations/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── locations-api.md # REST API contracts
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   └── Entities/
│       └── Location.cs                          # NEW entity
├── Application/
│   └── Locations/
│       ├── Commands/
│       │   ├── CreateLocation/
│       │   │   ├── CreateLocation.cs            # Command + Handler
│       │   │   └── CreateLocationCommandValidator.cs
│       │   ├── UpdateLocation/
│       │   │   ├── UpdateLocation.cs
│       │   │   └── UpdateLocationCommandValidator.cs
│       │   └── DeleteLocation/
│       │       └── DeleteLocation.cs            # Soft delete
│       └── Queries/
│           ├── GetLocations/
│           │   ├── GetLocations.cs              # List query with user filter
│           │   └── LocationBriefDto.cs
│           └── GetLocation/
│               ├── GetLocation.cs               # Detail query
│               └── LocationDetailDto.cs
├── Infrastructure/
│   └── Data/
│       └── Configurations/
│           └── LocationConfiguration.cs         # EF config + soft-delete filter
├── Web/
│   └── Endpoints/
│       └── Locations.cs                         # Minimal API endpoint group
└── App/
    ├── services/
    │   └── locations.ts                         # NEW API service
    ├── app/(app)/
    │   ├── settings/
    │   │   ├── index.tsx                        # NEW settings screen
    │   │   └── locations/
    │   │       ├── index.tsx                    # My Locations list
    │   │       ├── create.tsx                   # Create location
    │   │       └── [id].tsx                     # Edit location
    │   └── (existing screens modified for location picker/filter)
    └── components/
        └── LocationPickerModal.tsx              # NEW reusable picker

# Modified files:
# src/Domain/Entities/WorkoutTemplate.cs         → Replace Location string with LocationId FK
# src/Domain/Entities/ExerciseTemplate.cs        → Add LocationId FK
# src/Infrastructure/Data/ApplicationDbContext.cs → Add DbSet<Location>
# src/Application/Common/Interfaces/IApplicationDbContext.cs → Add DbSet<Location>
# src/Infrastructure/Data/Configurations/WorkoutTemplateConfiguration.cs → FK config
# src/Infrastructure/Data/Configurations/ExerciseTemplateConfiguration.cs → FK config
# src/Application/WorkoutTemplates/Commands/CreateWorkoutTemplate/* → LocationId
# src/Application/WorkoutTemplates/Commands/UpdateWorkoutTemplate/* → LocationId
# src/Application/WorkoutTemplates/Queries/GetWorkoutTemplates/* → LocationId filter + DTO
# src/Application/ExerciseTemplates/Commands/CreateExerciseTemplate/* → LocationId
# src/Application/ExerciseTemplates/Commands/UpdateExerciseTemplate/* → LocationId
# src/Application/ExerciseTemplates/Queries/GetExerciseTemplates/* → LocationId filter + DTO
# src/Web/Endpoints/WorkoutTemplates.cs → Pass filter param
# src/Web/Endpoints/ExerciseTemplates.cs → Pass filter param
# src/App/services/workout-templates.ts → LocationId field + filter param
# src/App/services/exercise-templates.ts → LocationId field + filter param
# src/App/app/(app)/index.tsx → Location filter chips
# src/App/app/(app)/workout-templates/create.tsx → Location picker
# src/App/app/(app)/workout-templates/[id].tsx → Location picker
# src/App/app/(app)/exercise-templates/create.tsx → Location picker
# src/App/app/(app)/exercise-templates/[id].tsx → Location picker
# src/App/components/ExercisePickerModal.tsx → Location filter chip
# src/Infrastructure/Data/ApplicationDbContextInitialiser.cs → Seed locations
# src/App/app/(app)/_layout.tsx → Add settings routes
```

**Structure Decision**: Follows the existing Mobile + API architecture. Location is a first-class domain entity with full CQRS stack. Mobile app gains a settings section for location management and a reusable LocationPickerModal for association.
