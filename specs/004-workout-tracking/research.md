# Research: Workout Tracking

**Feature Branch**: `004-workout-tracking`
**Date**: 2026-02-15

## R-001: Workout Set Data Model Strategy

**Decision**: Use a single `WorkoutSet` entity with nullable fields for all measurement types (weight, reps, duration, distance, bandColor, bodyweight, weightUnit, distanceUnit). The exercise's `ImplementType` and `ExerciseType` determine which fields are relevant for a given set.

**Rationale**: The existing codebase favors simplicity (Constitution Principle VII — YAGNI). A single flat table avoids polymorphic complexity (table-per-hierarchy, table-per-type) and keeps queries simple. The number of nullable fields is manageable (8 measurement fields). EF Core handles nullable columns efficiently, and the exercise type/implement combination already encodes which fields to expect. This mirrors the flat entity pattern used by WorkoutTemplateExercise.

**Alternatives considered**:
- Table-per-hierarchy with discriminator column: Rejected — adds ORM complexity for no query benefit, since all set types are queried together within a workout
- JSON payload column: Rejected — loses strong typing and query filterability at the DB level
- Separate tables per measurement type: Rejected — would require 6+ tables and union queries for workout summary views

## R-002: User Preferences Storage

**Decision**: Add a `UserPreferences` entity (one-per-user) with fields for WeightUnit, DistanceUnit, and Bodyweight. Create it lazily on first access with default values (lbs, miles, null bodyweight).

**Rationale**: The existing `ApplicationUser` entity extends ASP.NET Core Identity's `IdentityUser` and currently has only profile fields (FirstName, LastName, Age). Adding workout-specific preferences directly to ApplicationUser would mix concerns. A separate entity keeps preferences self-contained, follows Clean Architecture (preferences are a domain concept, not an identity concern), and can grow with future settings without touching the identity table.

**Alternatives considered**:
- Add columns to ApplicationUser: Rejected — couples workout preferences to identity; requires modifying Identity migrations
- JSON column on ApplicationUser: Rejected — same coupling issue plus loses strong typing
- Key-value settings table: Rejected — over-engineered for 3 fields; YAGNI applies

## R-003: Workout Snapshot Strategy for Historical Data Preservation

**Decision**: Store snapshot fields directly on the Workout and WorkoutExercise entities — `TemplateName` on Workout, `ExerciseName`/`ImplementType`/`ExerciseType` on WorkoutExercise. Maintain nullable FKs to the source template/exercise for linking when they still exist.

**Rationale**: The spec requires that historical workout data survives template/exercise deletion (FR-023). Snapshot fields are the simplest approach — they're set once at workout creation and never updated. The nullable FK approach (SetNull on delete for templates, no cascade from soft-deleted exercises) means the app can still navigate to a template if it exists, but displays the snapshot name if it doesn't. This is consistent with the existing Location FK pattern (SetNull on delete).

**Alternatives considered**:
- Temporal/versioned templates: Rejected — massive complexity for a simple snapshot need
- Denormalized JSON blob: Rejected — loses queryability
- Prevent template deletion if workouts exist: Rejected — poor UX, spec explicitly requires preservation

## R-004: Immediate Set Persistence Strategy

**Decision**: Each set is saved via an individual API call (POST /api/Workouts/{id}/Exercises/{exerciseId}/Sets) immediately when the user confirms it. The mobile client fires a POST for each set and handles offline/failure with a retry queue.

**Rationale**: The clarification established that sets must be saved immediately to the server (not batched). Individual set creation is the simplest API design — one POST creates one set and returns the ID. This ensures no data loss on app crash. The mobile client can optimistically update the UI and retry failed POSTs. Server-side, each POST is a simple insert — no conflict resolution needed since sets are append-only during a workout.

**Alternatives considered**:
- Batch save on workout completion: Rejected — contradicts clarification that sets save immediately
- WebSocket push: Over-engineered — HTTP POST per set is sufficient for the expected frequency (one set every 30-120 seconds)
- Local-first with background sync: Rejected — adds offline-first complexity; not required by spec

## R-005: Workout Status and Lifecycle

**Decision**: Use a `WorkoutStatus` enum with two values: `InProgress` (0) and `Completed` (1). The status is set to InProgress on creation and transitions to Completed when the user finalizes. Completed workouts can be edited (per clarification) but remain in Completed status — there is no "reopen" workflow.

**Rationale**: The spec defines exactly two states. A simple enum is sufficient. Discarding an in-progress workout is a hard delete (since the data has no historical value). The constitution's Principle VI mentions "edits create new versions, never silently overwrite" — however, for this iteration, direct field edits on completed workouts are acceptable since there's no collaboration or audit trail requirement on individual set changes.

**Alternatives considered**:
- Additional states (Paused, Cancelled): Rejected — YAGNI, spec defines only in-progress and completed
- Soft-delete for discarded workouts: Rejected — no value in keeping discarded workout data

## R-006: Workout History Pagination and Search

**Decision**: Use cursor-based pagination (keyset pagination) for workout history, with `EndedAt` as the primary cursor and `Id` as the tiebreaker. Search uses SQL Server's `LIKE` operator on the Notes column. Filtering uses WHERE clauses on LocationId and Rating.

**Rationale**: Cursor-based pagination performs better than offset-based for large datasets and avoids the "shifting page" problem when new workouts are added. The existing codebase uses `OrderByDescending` patterns that translate cleanly to keyset pagination. For the expected scale (hundreds of workouts per user), `LIKE` search on Notes is sufficient without full-text indexing.

**Alternatives considered**:
- Offset-based pagination: Rejected — less performant for large datasets, inconsistent with modern mobile infinite scroll
- Full-text search: Over-engineered for free-text notes search on per-user data
- ElasticSearch: Rejected — external dependency for a simple search requirement

## R-007: Unit Storage on Sets

**Decision**: Store the weight unit and distance unit as enum values directly on each WorkoutSet record. This captures the unit at recording time, making historical data self-describing regardless of future preference changes.

**Rationale**: The spec requires that changing unit preferences does not retroactively convert existing data (FR-024). Storing the unit per-set is the simplest way to satisfy this — each set is a complete record with its own unit context. The enums (WeightUnit, DistanceUnit) are small and add minimal storage overhead.

**Alternatives considered**:
- Store all values in a canonical unit (e.g., kg/meters) and convert for display: Rejected — spec says values are stored as entered, not converted
- Store unit on the Workout (parent) level: Rejected — edge case where user changes preference mid-workout (per spec, this is handled by locking to the unit at workout start, but per-set storage is more robust)
