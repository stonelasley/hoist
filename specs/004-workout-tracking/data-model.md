# Data Model: Workout Tracking

**Feature Branch**: `004-workout-tracking`
**Date**: 2026-02-15

## New Enums

### WorkoutStatus

```
InProgress = 0
Completed = 1
```

### WeightUnit

```
Lbs = 0
Kg = 1
```

### DistanceUnit

```
Miles = 0
Kilometers = 1
Meters = 2
Yards = 3
```

## New Entities

### UserPreferences

Stores per-user workout tracking settings. Created lazily on first access with defaults.

| Field        | Type                 | Required | Constraints                            |
|--------------|----------------------|----------|----------------------------------------|
| Id           | int (PK)             | Yes      | Auto-generated                         |
| UserId       | string (FK)          | Yes      | References ApplicationUser.Id, unique  |
| WeightUnit   | WeightUnit (enum)    | Yes      | Default: Lbs                           |
| DistanceUnit | DistanceUnit (enum)  | Yes      | Default: Miles                         |
| Bodyweight   | decimal?             | No       | Precision 6,2. Stored in user's preferred weight unit |
| Created      | DateTimeOffset       | Yes      | Auto-set (BaseAuditableEntity)         |
| CreatedBy    | string?              | Yes      | Auto-set (BaseAuditableEntity)         |
| LastModified | DateTimeOffset       | Yes      | Auto-set (BaseAuditableEntity)         |
| LastModifiedBy | string?            | Yes      | Auto-set (BaseAuditableEntity)         |

**Relationships**:
- Belongs to one User (via UserId, unique constraint)

### Workout

An execution instance of a workout template.

| Field              | Type                    | Required | Constraints                                |
|--------------------|-------------------------|----------|--------------------------------------------|
| Id                 | int (PK)                | Yes      | Auto-generated                             |
| WorkoutTemplateId  | int? (FK)               | No       | References WorkoutTemplate.Id, SetNull on delete |
| TemplateName       | string                  | Yes      | Max 200 chars. Snapshot of template name at creation |
| Status             | WorkoutStatus (enum)    | Yes      | Default: InProgress                        |
| StartedAt          | DateTimeOffset          | Yes      | Set to current time on creation            |
| EndedAt            | DateTimeOffset?         | No       | Set when workout is completed              |
| Notes              | string?                 | No       | Max 2000 chars                             |
| Rating             | int?                    | No       | 1-5 inclusive, null if not rated            |
| LocationId         | int? (FK)               | No       | References Location.Id, SetNull on delete  |
| LocationName       | string?                 | No       | Max 200 chars. Snapshot of location name at creation |
| UserId             | string (FK)             | Yes      | References ApplicationUser.Id              |
| Created            | DateTimeOffset          | Yes      | Auto-set (BaseAuditableEntity)             |
| CreatedBy          | string?                 | Yes      | Auto-set (BaseAuditableEntity)             |
| LastModified       | DateTimeOffset          | Yes      | Auto-set (BaseAuditableEntity)             |
| LastModifiedBy     | string?                 | Yes      | Auto-set (BaseAuditableEntity)             |

**Relationships**:
- Belongs to one User (via UserId)
- Optionally references one WorkoutTemplate (nullable FK, SetNull on template delete)
- Optionally references one Location (nullable FK, SetNull on location delete)
- Has many WorkoutExercise records (ordered by Position)

### WorkoutExercise

Represents a specific exercise performed in a workout. Maintains its own position and snapshots exercise metadata.

| Field              | Type                    | Required | Constraints                                |
|--------------------|-------------------------|----------|--------------------------------------------|
| Id                 | int (PK)                | Yes      | Auto-generated                             |
| WorkoutId          | int (FK)                | Yes      | References Workout.Id, cascade delete      |
| ExerciseTemplateId | int? (FK)               | No       | References ExerciseTemplate.Id, SetNull on delete |
| ExerciseName       | string                  | Yes      | Max 200 chars. Snapshot of exercise name   |
| ImplementType      | ImplementType (enum)    | Yes      | Snapshot of exercise implement type        |
| ExerciseType       | ExerciseType (enum)     | Yes      | Snapshot of exercise type                  |
| Position           | int                     | Yes      | 1-based ordering within workout            |
| Created            | DateTimeOffset          | Yes      | Auto-set (BaseAuditableEntity)             |
| CreatedBy          | string?                 | Yes      | Auto-set (BaseAuditableEntity)             |
| LastModified       | DateTimeOffset          | Yes      | Auto-set (BaseAuditableEntity)             |
| LastModifiedBy     | string?                 | Yes      | Auto-set (BaseAuditableEntity)             |

**Relationships**:
- Belongs to one Workout (cascade delete)
- Optionally references one ExerciseTemplate (nullable FK, SetNull on exercise soft-delete)
- Has many WorkoutSet records (ordered by Position)

### WorkoutSet

A single set performed for an exercise. Uses nullable fields — the exercise's ImplementType and ExerciseType determine which fields are populated.

| Field        | Type                 | Required | Constraints                                     |
|--------------|----------------------|----------|-------------------------------------------------|
| Id           | int (PK)             | Yes      | Auto-generated                                  |
| WorkoutExerciseId | int (FK)        | Yes      | References WorkoutExercise.Id, cascade delete   |
| Position     | int                  | Yes      | 1-based ordering within exercise                |
| Weight       | decimal?             | No       | Precision 8,2. For weighted implements          |
| Reps         | int?                 | No       | For reps-type exercises                         |
| Duration     | int?                 | No       | In seconds. For duration-type exercises and optional distance time |
| Distance     | decimal?             | No       | Precision 10,4. For distance-type exercises     |
| Bodyweight   | decimal?             | No       | Precision 6,2. For bodyweight implements        |
| BandColor    | string?              | No       | Colour code (e.g., "#FF5733"). For band implements |
| WeightUnit   | WeightUnit? (enum)   | No       | Unit used for weight/bodyweight at recording time |
| DistanceUnit | DistanceUnit? (enum) | No       | Unit used for distance at recording time        |
| Created      | DateTimeOffset       | Yes      | Auto-set (BaseAuditableEntity)                  |
| CreatedBy    | string?              | Yes      | Auto-set (BaseAuditableEntity)                  |
| LastModified | DateTimeOffset       | Yes      | Auto-set (BaseAuditableEntity)                  |
| LastModifiedBy | string?            | Yes      | Auto-set (BaseAuditableEntity)                  |

**Relationships**:
- Belongs to one WorkoutExercise (cascade delete)

**Field Population Rules** (by ImplementType + ExerciseType):

| ImplementType              | ExerciseType | Fields Populated                          |
|----------------------------|-------------|-------------------------------------------|
| Barbell, Dumbbell, SelectorizedMachine, PlateLoadedMachine, Plate, MedicineBall, Kettlebell | Reps | Weight, Reps, WeightUnit |
| Bodyweight                 | Reps         | Bodyweight, Reps, WeightUnit              |
| Bodyweight                 | Duration     | Bodyweight, Duration, WeightUnit          |
| Band                       | Reps         | BandColor, Reps                           |
| Band                       | Duration     | BandColor, Duration                       |
| Any                        | Distance     | Distance, Duration (optional), DistanceUnit |

## Indexes

| Table             | Columns                           | Type   | Notes                                          |
|-------------------|-----------------------------------|--------|-------------------------------------------------|
| UserPreferences   | (UserId)                          | Unique | One preferences record per user                 |
| Workout           | (UserId, Status)                  | Index  | Supports finding in-progress workout            |
| Workout           | (UserId, EndedAt)                 | Index  | Supports recent workouts and history pagination |
| Workout           | (UserId, Rating)                  | Index  | Supports sorting/filtering by rating            |
| Workout           | (LocationId)                      | Index  | Supports filtering by location                  |
| WorkoutExercise   | (WorkoutId, Position)             | Index  | Supports ordered exercise retrieval             |
| WorkoutSet        | (WorkoutExerciseId, Position)     | Index  | Supports ordered set retrieval                  |

## Entity Relationship Diagram (textual)

```
ApplicationUser (1) ──── (*) Workout
ApplicationUser (1) ──── (0..1) UserPreferences

WorkoutTemplate (1) ──?── (*) Workout (nullable FK, snapshot name preserved)
Location (1) ──?── (*) Workout (nullable FK, snapshot name preserved)

Workout (1) ──── (*) WorkoutExercise (*) ──?── (1) ExerciseTemplate (nullable FK, snapshot fields preserved)
WorkoutExercise (1) ──── (*) WorkoutSet
```

## State Transitions

### Workout Lifecycle

```
[Start Workout] → InProgress (StartedAt = now)
InProgress → Completed (EndedAt = now, user adds notes/rating)
InProgress → [Discarded] (hard delete)
Completed → Completed (edits allowed: sets, notes, rating, times)
```

- Only one InProgress workout per user at a time (enforced at application level)
- Discarding an InProgress workout hard-deletes it and all associated exercises/sets (cascade)
- Completed workouts are fully editable but remain in Completed status

## Validation Rules

| Entity           | Field          | Rule                                                        |
|------------------|----------------|-------------------------------------------------------------|
| Workout          | TemplateName   | Not empty, max 200 characters                               |
| Workout          | Notes          | Max 2000 characters (if provided)                           |
| Workout          | Rating         | 1-5 inclusive (if provided)                                 |
| Workout          | StartedAt      | Required, must be in the past or present                    |
| Workout          | EndedAt        | Must be after StartedAt (if provided)                       |
| Workout          | LocationName   | Max 200 characters (if provided)                            |
| WorkoutExercise  | ExerciseName   | Not empty, max 200 characters                               |
| WorkoutExercise  | Position       | Positive integer, sequential within workout                 |
| WorkoutSet       | Position       | Positive integer, sequential within exercise                |
| WorkoutSet       | Weight         | Non-negative (if provided)                                  |
| WorkoutSet       | Reps           | Non-negative (if provided)                                  |
| WorkoutSet       | Duration       | Non-negative (if provided)                                  |
| WorkoutSet       | Distance       | Non-negative (if provided)                                  |
| WorkoutSet       | Bodyweight     | Non-negative (if provided)                                  |
| UserPreferences  | Bodyweight     | Positive (if provided)                                      |
| UserPreferences  | WeightUnit     | Must be a valid WeightUnit enum value                       |
| UserPreferences  | DistanceUnit   | Must be a valid DistanceUnit enum value                     |
