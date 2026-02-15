# Research: Workout & Exercise Templates

**Feature Branch**: `002-workout-exercise-templates`
**Date**: 2026-02-14

## R-001: Image Upload Strategy for Exercise Templates

**Decision**: Store images as files on the server filesystem (or cloud blob storage in production), with the file path/URL stored in the database as a string column on the ExerciseTemplate entity. Upload via a dedicated multipart/form-data endpoint that returns the stored URL.

**Rationale**: The existing codebase uses ASP.NET Core Minimal APIs. Storing images as binary blobs in SQL Server is discouraged for performance and scalability. A file-based approach keeps the database lean, is simple to implement, and allows future migration to Azure Blob Storage. For the development environment, local file storage under `wwwroot/uploads/exercises/` is sufficient.

**Alternatives considered**:
- Base64 in DB column: Rejected — bloats database, inefficient for large images, complicates queries
- Azure Blob Storage directly: Deferred — adds external dependency; can migrate later with the same URL-in-DB pattern
- Third-party service (Cloudinary, S3): Deferred — unnecessary complexity for MVP

## R-002: Soft Delete Pattern for Exercise Templates

**Decision**: Add an `IsDeleted` boolean and `DeletedAt` DateTimeOffset? to the ExerciseTemplate entity. Apply a global query filter in EF Core to exclude soft-deleted records by default. Provide an `IgnoreQueryFilters()` escape hatch for admin/reporting scenarios if needed.

**Rationale**: EF Core global query filters are the established pattern for soft delete. This keeps the domain simple (just a flag) while transparently filtering soft-deleted records from all standard queries. Workout template exercise associations continue to reference the exercise by ID regardless of soft-delete status — the foreign key is preserved.

**Alternatives considered**:
- Separate "archived" table: Rejected — adds migration complexity, breaks foreign keys
- Status enum (Active/Deleted/Archived): Over-engineered for current requirement — YAGNI applies; can expand later if archive feature is needed

## R-003: Exercise Order Within Workout Templates

**Decision**: Use an integer `Position` column on the `WorkoutTemplateExercise` join entity. Position values are assigned starting at 1 and increment sequentially. Reorder operations reassign positions for all affected rows in a single transaction.

**Rationale**: Simple integer ordering is the most straightforward approach for EF Core. Gap-based ordering (e.g., positions 10, 20, 30) adds unnecessary complexity for a client-driven reorder where the full list is submitted. The mobile client will submit the complete ordered list on save, so server-side gap management is unnecessary.

**Alternatives considered**:
- Linked list (prev/next pointers): Rejected — complex queries, harder to maintain consistency
- Fractional ordering (0.5, 1.5): Rejected — precision issues, eventual need for rebalancing
- Gap-based integers (10, 20, 30): Rejected — unnecessary for full-list reorder pattern

## R-004: Exercise Name Uniqueness Enforcement

**Decision**: Enforce unique exercise names per user via a unique composite index on `(UserId, Name)` in the database, plus a FluentValidation rule that checks for existing names before save.

**Rationale**: Database-level constraint prevents race conditions. Application-level validation provides user-friendly error messages. The composite index ensures names are unique per user (not globally). Soft-deleted exercises are excluded from the uniqueness check via the global query filter.

**Alternatives considered**:
- Application-only check: Rejected — race condition allows duplicates under concurrent requests
- Database-only constraint: Insufficient — raw SQL exception is not user-friendly

## R-005: Search and Filter Implementation

**Decision**: Implement server-side search (case-insensitive `LIKE` on name) and filtering (WHERE clauses on ImplementType and ExerciseType) in the API query handlers. The mobile client sends search/filter parameters as query string parameters.

**Rationale**: Server-side filtering keeps the API pattern consistent with the existing `GetTodoItemsWithPagination` query. For the expected scale (up to 500 exercises per user), this is efficient without additional indexing. If scale increases, a full-text index can be added later.

**Alternatives considered**:
- Client-side filtering (fetch all, filter in JS): Rejected — wastes bandwidth, inconsistent with existing API patterns
- Full-text search (SQL Server FTS): Over-engineered for name-only search on small datasets

## R-006: Landing Page Data Loading Strategy

**Decision**: Two separate API calls from the mobile client — one for workout templates and one for exercise templates. Both return lightweight list DTOs. The landing page loads both in parallel.

**Rationale**: Separate endpoints follow CQRS principles (one query per concern), are independently cacheable, and allow the UI to render each section as it arrives. A combined "dashboard" endpoint would couple unrelated data.

**Alternatives considered**:
- Single "dashboard" endpoint: Rejected — violates CQRS, couples workout and exercise data
- GraphQL: Over-engineered for this use case; project is committed to REST Minimal APIs
