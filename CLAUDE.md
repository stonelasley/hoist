# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
dotnet build -tl                        # Build the solution
dotnet test                             # Run all tests (unit, integration, functional)
dotnet test tests/Domain.UnitTests      # Run a single test project
dotnet test --filter "FullyQualifiedName~TodoItems"  # Run tests matching a name
dotnet run --project src/AppHost        # Run via Aspire (starts SQL Server container + Web app)
dotnet watch run --project src/Web      # Run Web directly with hot reload (needs DB available)
```

**NSwag note:** The post-build NSwag step generates `src/Web/wwwroot/api/specification.json` by booting the ASP.NET host. It produces warnings when no SQL Server is available — this is expected and does not fail the build.

## Code Scaffolding

From `src/Application/`:
```bash
dotnet new ca-usecase --name CreateFoo --feature-name Foos --usecase-type command --return-type int
dotnet new ca-usecase -n GetFoos -fn Foos -ut query -rt FoosVm
```

## Architecture

This is a **Clean Architecture** solution (based on [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture) v10.0.0-preview) with CQRS via MediatR.

**Dependency flow:** `Web → Application + Infrastructure → Domain`

### Layers

- **Domain** — Entities (`TodoList`, `TodoItem`), value objects (`Colour`), domain events, constants. No framework dependencies except MediatR.Contracts.
- **Application** — Commands and queries organized by feature folder (e.g., `TodoItems/Commands/CreateTodoItem/`). Each use case has a handler, command/query record, and optional validator. Contains interfaces (`IApplicationDbContext`, `IUser`, `IIdentityService`), DTOs, AutoMapper mappings.
- **Infrastructure** — EF Core `ApplicationDbContext`, entity configurations, ASP.NET Core Identity (`ApplicationUser`), `IdentityService`. Reads connection string key `HoistDb`.
- **Web** — Minimal API endpoints, DI composition root, NSwag OpenAPI generation.
- **AppHost** — .NET Aspire orchestrator that starts a SQL Server container and the Web project together.

### Key Patterns

**Endpoints:** Minimal APIs via `EndpointGroupBase` subclasses in `src/Web/Endpoints/`. Each class maps routes in `Map(RouteGroupBuilder)` and delegates to MediatR. Endpoints are auto-discovered by reflection and mounted at `/api/{GroupName}`.

**MediatR Pipeline Behaviors** (registered in `src/Application/DependencyInjection.cs`, executed in order):
1. `LoggingBehaviour` — Logs request name/user
2. `UnhandledExceptionBehaviour` — Catches and logs exceptions
3. `AuthorizationBehaviour` — Checks `[Authorize]` attributes for roles/policies
4. `ValidationBehaviour` — Runs FluentValidation validators
5. `PerformanceBehaviour` — Warns on requests >500ms

**Domain Events:** Entities raise events (e.g., `TodoItemCreatedEvent`) via `BaseEntity.AddDomainEvent()`. The `DispatchDomainEventsInterceptor` publishes them after `SaveChanges`.

**Auditing:** `AuditableEntityInterceptor` auto-populates `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy` on `BaseAuditableEntity` subclasses.

**Auth:** Bearer token auth via ASP.NET Core Identity. Identity endpoints auto-mapped via `AddApiEndpoints()`. Authorization policy `CanPurge` requires `Administrator` role.

**Exception Handling:** `CustomExceptionHandler` maps `ValidationException` → 400, `NotFoundException` → 404, `UnauthorizedAccessException` → 401, `ForbiddenAccessException` → 403.

## Testing

Tests require Docker (for Testcontainers SQL Server).

- **Domain.UnitTests** — Pure domain logic tests
- **Application.UnitTests** — Command/query handler tests with mocked `IApplicationDbContext`
- **Application.FunctionalTests** — End-to-end tests via `CustomWebApplicationFactory` with real SQL Server (Testcontainers). Uses Respawn for DB cleanup between tests. Key helpers in `Testing.cs`: `SendAsync()`, `RunAsDefaultUserAsync()`, `RunAsAdministratorAsync()`, `ResetState()`, `FindAsync<T>()`, `AddAsync<T>()`
- **Infrastructure.IntegrationTests** — Direct database tests against Testcontainers

## Tech Stack

- **.NET 10.0**, C# with nullable reference types, implicit usings
- **SQL Server** via EF Core 10.0 (Aspire container for dev, Testcontainers for tests)
- **Central Package Management** — all versions in `Directory.Packages.props`
- **TreatWarningsAsErrors** is enabled globally in `Directory.Build.props`
- **Build artifacts** output to `artifacts/` (configured via `ArtifactsPath`)

## Connection String

The app reads `ConnectionStrings:HoistDb`. When run via Aspire (`src/AppHost`), the connection string is injected automatically. The fallback in `appsettings.Development.json` points to `localhost,1433`.

## Database Initialization

In development, `ApplicationDbContextInitialiser` drops and recreates the database on startup, then seeds default roles, an admin user (`administrator@localhost` / `Administrator1!`), and sample data. Init failures are logged but don't crash the app (allows NSwag and non-DB workflows to proceed).
