# Implementation Plan: Workout & Exercise Templates

**Branch**: `002-workout-exercise-templates` | **Date**: 2026-02-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-workout-exercise-templates/spec.md`

## Summary

Implement workout and exercise template management for the Hoist workout tracking app. Users can create, edit, delete, and view workout templates on the landing page, each containing an ordered list of exercise templates. Exercise templates live in a per-user library with search and filter capabilities. The backend follows Clean Architecture with CQRS (MediatR commands/queries), EF Core entities with soft-delete for exercises, and Minimal API endpoints. The mobile app uses Expo Router screens for CRUD flows and an exercise picker modal.

## Technical Context

**Language/Version**: C# / .NET 10.0 (backend), TypeScript 5.9 / React Native 0.81 (mobile)
**Primary Dependencies**: ASP.NET Core Identity, MediatR, FluentValidation, EF Core 10, AutoMapper (backend); Expo SDK 54, expo-router 6, React 19 (mobile)
**Storage**: SQL Server via EF Core (Aspire-orchestrated container in dev, Testcontainers in tests); local filesystem for exercise images
**Testing**: NUnit + Shouldly + Moq + Respawn + Testcontainers (backend); Jest (mobile)
**Target Platform**: iOS + Android (mobile), Linux/Windows server (backend API)
**Project Type**: Mobile + API
**Performance Goals**: Landing page data load <2s, create/edit operations <1s response time, smooth scrolling for up to 500 items
**Constraints**: Image uploads max 5 MB (JPEG/PNG/WebP), exercise names unique per user
**Scale/Scope**: Up to 500 workout templates and 500 exercises per user; 6 new screens, ~12 new use cases, 3 new entities

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Architecture | PASS | Entities in Domain, use cases in Application feature folders, EF config in Infrastructure, endpoints in Web |
| II. CQRS via MediatR | PASS | All operations expressed as Commands (create, update, delete) and Queries (get list, get detail). Validators use FluentValidation. |
| III. Domain-Driven Design | PASS | Entities extend BaseAuditableEntity. ImplementType/ExerciseType as domain enums. Soft-delete is domain behavior on ExerciseTemplate. |
| IV. API-First with Minimal APIs | PASS | Two new EndpointGroupBase subclasses: WorkoutTemplates, ExerciseTemplates. All require authorization. |
| V. Test-Driven Quality | PASS | Unit tests for validators/handlers, functional tests for API flows with Testcontainers. |
| VI. Fitness Domain Integrity | PASS | Exercise data uses established equipment types. Workout templates serve the core plan-track-share journey. |
| VII. Simplicity and YAGNI | PASS | No speculative abstractions. Direct EF Core access via IApplicationDbContext. Simple integer ordering. Local file storage for images. |
| VIII. Mobile-First with Expo RN | PASS | File-based routing under app/(app)/. Functional components with hooks. Theme tokens from constants/theme.ts. No business logic in client. |

**Post-Phase 1 Re-check**: All principles remain satisfied. No violations detected.

## Project Structure

### Documentation (this feature)

```text
specs/002-workout-exercise-templates/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── workout-templates-api.md
│   └── exercise-templates-api.md
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Entities/
│   │   ├── WorkoutTemplate.cs          # New entity
│   │   ├── ExerciseTemplate.cs         # New entity
│   │   └── WorkoutTemplateExercise.cs  # New join entity
│   ├── Enums/
│   │   ├── ImplementType.cs            # New enum
│   │   └── ExerciseType.cs             # New enum
│   └── Events/
│       ├── WorkoutTemplateCreatedEvent.cs
│       └── ExerciseTemplateCreatedEvent.cs
├── Application/
│   ├── Common/Interfaces/
│   │   └── IApplicationDbContext.cs     # Add new DbSets
│   ├── WorkoutTemplates/
│   │   ├── Commands/
│   │   │   ├── CreateWorkoutTemplate/
│   │   │   ├── UpdateWorkoutTemplate/
│   │   │   ├── DeleteWorkoutTemplate/
│   │   │   └── UpdateWorkoutTemplateExercises/
│   │   └── Queries/
│   │       ├── GetWorkoutTemplates/
│   │       └── GetWorkoutTemplate/
│   └── ExerciseTemplates/
│       ├── Commands/
│       │   ├── CreateExerciseTemplate/
│       │   ├── UpdateExerciseTemplate/
│       │   ├── DeleteExerciseTemplate/
│       │   └── UploadExerciseImage/
│       └── Queries/
│           ├── GetExerciseTemplates/
│           └── GetExerciseTemplate/
├── Infrastructure/
│   └── Data/
│       ├── ApplicationDbContext.cs           # Add new DbSets
│       ├── Configurations/
│       │   ├── WorkoutTemplateConfiguration.cs
│       │   ├── ExerciseTemplateConfiguration.cs
│       │   └── WorkoutTemplateExerciseConfiguration.cs
│       └── ApplicationDbContextInitialiser.cs  # Add seed data
├── Web/
│   ├── Endpoints/
│   │   ├── WorkoutTemplates.cs          # New endpoint group
│   │   └── ExerciseTemplates.cs         # New endpoint group
│   └── wwwroot/uploads/exercises/       # Image upload directory
└── App/
    ├── app/(app)/
    │   ├── index.tsx                     # Modify: landing page with template lists
    │   ├── workout-templates/
    │   │   ├── [id].tsx                  # New: view/edit workout template
    │   │   └── create.tsx               # New: create workout template
    │   └── exercise-templates/
    │       ├── [id].tsx                  # New: view/edit exercise template
    │       └── create.tsx               # New: create exercise template
    ├── services/
    │   ├── workout-templates.ts         # New: API client
    │   └── exercise-templates.ts        # New: API client
    └── components/
        ├── WorkoutTemplateList.tsx       # New: workout list for landing page
        ├── ExerciseTemplateList.tsx      # New: exercise list with search/filter
        └── ExercisePickerModal.tsx       # New: modal for adding exercises to workout

tests/
├── Domain.UnitTests/
│   └── (exercise template soft-delete, enum validation)
├── Application.FunctionalTests/
│   ├── WorkoutTemplates/
│   │   ├── Commands/ (create, update, delete, update exercises)
│   │   └── Queries/ (get list, get detail)
│   └── ExerciseTemplates/
│       ├── Commands/ (create, update, delete, image upload)
│       └── Queries/ (get list with search/filter, get detail)
└── Application.UnitTests/
    └── (validator tests)
```

**Structure Decision**: Mobile + API pattern following the existing project structure. Backend code follows Clean Architecture layers already established. Mobile screens use file-based routing under the `(app)` group (authenticated). No new projects are created — all code fits within existing solution structure.

## Complexity Tracking

> No constitution violations detected. No complexity justifications needed.
