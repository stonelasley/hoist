# Research: Gym Locations

**Feature**: 003-gym-locations
**Date**: 2026-02-14

## R1: Soft-Delete Pattern for Location Entity

**Decision**: Follow the existing ExerciseTemplate soft-delete pattern — `IsDeleted` boolean + `DeletedAt` timestamp + EF Core global query filter.

**Rationale**: The project already has a proven soft-delete pattern on ExerciseTemplate. Reusing it ensures consistency, avoids introducing new abstractions, and the EF Core query filter automatically excludes deleted records from all queries without manual filtering.

**Alternatives considered**:
- Hard delete with cascade to null on FKs — rejected because spec requires deleted location names to remain visible on associated templates
- Soft-delete via a separate "archived" status enum — rejected as over-engineered for a boolean state

## R2: Replacing Free-Text Location Column

**Decision**: Drop the `Location` (string?) column from WorkoutTemplate and replace it with `LocationId` (int?) FK. Since the free-text field was never surfaced in the mobile app UI, no data migration is needed — the column is simply removed.

**Rationale**: Clarified during `/speckit.clarify` that the free-text location was never user-facing in the mobile app. The column exists in the schema and backend DTOs but was only a placeholder. Dropping it cleanly and replacing with a proper FK is the simplest approach.

**Alternatives considered**:
- Keep both columns (legacy + new FK) — rejected as unnecessary complexity since no user data exists
- Auto-migrate text values to Location entities — rejected since no user-entered data exists

## R3: FK Relationship Design (Location → Templates)

**Decision**: One-to-many from Location to WorkoutTemplate and ExerciseTemplate. Each template has an optional `LocationId` (int?) FK. The FK uses `DeleteBehavior.SetNull` so that if a Location is hard-deleted (bypassing soft-delete), the FK is safely nulled.

**Rationale**: The spec states "a workout template or exercise template can be associated with at most one location." A simple nullable FK is the most straightforward implementation. `SetNull` on delete is a safety net; normal flow uses soft-delete.

**Alternatives considered**:
- Many-to-many (a template at multiple locations) — rejected per spec; one location per template
- Junction table — unnecessary overhead for a simple optional FK

## R4: Location Filter Implementation Strategy

**Decision**: Server-side filtering via query parameter on existing GetWorkoutTemplates and GetExerciseTemplates queries. Add `int? LocationId` parameter. Client passes the selected location ID; backend appends `.Where(x => x.LocationId == locationId)`.

**Rationale**: Follows the existing pattern where GetExerciseTemplates already accepts optional `Search`, `ImplementType`, and `ExerciseType` parameters. Adding `LocationId` is consistent and efficient. Server-side filtering is preferred since template lists can be large (up to 500 items).

**Alternatives considered**:
- Client-side filtering only (fetch all, filter in-memory) — rejected for consistency with existing server-side filtering pattern and to handle larger lists efficiently
- Separate filtered endpoint — rejected as unnecessary; adding a query param to existing endpoints is simpler

## R5: Instagram Handle Storage

**Decision**: Store Instagram handle as a plain string without the "@" prefix. Strip leading "@" on save if present. Max length 30 characters (Instagram's limit).

**Rationale**: Instagram usernames are max 30 characters. Stripping "@" normalizes storage so queries and display are consistent. No Instagram API integration needed — this is purely a reference field.

**Alternatives considered**:
- Store with "@" — rejected because it complicates comparison and the "@" is a display concern
- Validate against Instagram username rules (alphanumeric + underscores + periods) — deferred as low value; users may enter display names rather than exact handles

## R6: GPS Coordinates Storage

**Decision**: Store latitude and longitude as separate `decimal?` fields with precision sufficient for ~1 meter accuracy (6 decimal places). Use `decimal(9,6)` for latitude (-90.000000 to 90.000000) and `decimal(10,6)` for longitude (-180.000000 to 180.000000).

**Rationale**: SQL Server's `decimal` type provides exact storage for geographic coordinates. Separate fields are simpler than a geography type and the feature doesn't require spatial queries (just storage and display). The precision is standard for fitness/gym location use.

**Alternatives considered**:
- SQL Server `geography` type — over-engineered since no spatial queries (nearest gym, distance) are needed
- Single string field for coordinates — rejected because validation and individual access become harder

## R7: Settings/Profile Navigation Structure

**Decision**: Create a new settings route group under `src/App/app/(app)/settings/`. The settings screen links to "My Locations" as a sub-screen. The settings entry point is accessible from the landing page header (gear/profile icon).

**Rationale**: Clarified during `/speckit.clarify` that location management belongs in settings, not as a top-level tab. This keeps the main navigation focused on workouts and exercises. The settings area is a natural home for user configuration.

**Alternatives considered**:
- Bottom tab for locations — rejected per clarification; settings is more appropriate for setup tasks
- Landing page section — rejected per clarification; clutters the primary view
