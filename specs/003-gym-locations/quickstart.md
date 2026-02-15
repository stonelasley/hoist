# Quickstart: Gym Locations

**Feature**: 003-gym-locations
**Branch**: `003-gym-locations`

## Prerequisites

- .NET 10.0 SDK
- Docker (for SQL Server via Aspire/Testcontainers)
- Node.js 20+ and npm (for Expo mobile app)
- Expo CLI (`npx expo`)

## Backend Development

### Start the backend

```bash
dotnet run --project src/AppHost
```

This starts the SQL Server container via Aspire and the Web API. The database is dropped and recreated on startup with seed data (including new Location seed data once implemented).

### Build and verify

```bash
dotnet build -tl
```

### Run tests

```bash
# All tests
dotnet test

# Only unit tests
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests

# Only functional tests (requires Docker)
dotnet test tests/Application.FunctionalTests

# Specific test by name
dotnet test --filter "FullyQualifiedName~Location"
```

### Scaffold a new use case

```bash
# From src/Application/
dotnet new ca-usecase --name CreateLocation --feature-name Locations --usecase-type command --return-type int
dotnet new ca-usecase -n GetLocations -fn Locations -ut query -rt "List<LocationBriefDto>"
```

### API docs

Swagger UI available at `http://localhost:<port>/api` when running via Aspire.

## Mobile Development

### Start the mobile app

```bash
cd src/App
npx expo start
```

### Key directories for this feature

```
src/App/
├── services/locations.ts           # API service (create this)
├── app/(app)/settings/             # Settings screens (create this)
│   ├── index.tsx                   # Settings hub
│   └── locations/                  # Location CRUD screens
│       ├── index.tsx
│       ├── create.tsx
│       └── [id].tsx
└── components/
    └── LocationPickerModal.tsx     # Reusable picker (create this)
```

## Implementation Order

1. **Domain entity** — `Location.cs`
2. **EF config + DbContext** — `LocationConfiguration.cs`, update `ApplicationDbContext`, `IApplicationDbContext`
3. **Modify WorkoutTemplate/ExerciseTemplate** — replace free-text Location, add LocationId FK
4. **Application layer** — CQRS commands/queries for Location CRUD
5. **Update existing commands/queries** — add LocationId to WorkoutTemplate and ExerciseTemplate operations
6. **Web endpoints** — `Locations.cs` endpoint group, update existing endpoints for filter param
7. **Seed data** — add locations to `ApplicationDbContextInitialiser`
8. **Backend tests** — unit tests for handlers, functional tests for API
9. **Mobile service** — `locations.ts` API service
10. **Mobile screens** — settings, location CRUD, location picker modal
11. **Integrate location picker** — into workout template and exercise template create/edit screens
12. **Location filter** — add filter chips to landing page and exercise picker

## Verification

After implementation, verify:

```bash
# Backend compiles clean
dotnet build -tl

# All tests pass
dotnet test

# Start full stack and manually test:
dotnet run --project src/AppHost
# In another terminal:
cd src/App && npx expo start
```

Manual test flow:
1. Log in as `test@test.com` / `Test1234!`
2. Navigate to Settings → My Locations
3. Create "The Refinery" with optional details
4. Go to a workout template → set location to "The Refinery"
5. Return to landing page → filter workouts by "The Refinery"
6. Verify only matching workouts appear
