# Data Model: Workout & Exercise Templates

**Feature Branch**: `002-workout-exercise-templates`
**Date**: 2026-02-14

## Entities

### ExerciseTemplate

Represents a specific exercise in a user's library.

| Field         | Type                    | Required | Constraints                                     |
|---------------|-------------------------|----------|-------------------------------------------------|
| Id            | int (PK)                | Yes      | Auto-generated                                  |
| Name          | string                  | Yes      | Max 200 chars, unique per user (composite index with UserId, excludes soft-deleted) |
| ImplementType | ImplementType (enum)    | Yes      | One of 9 predefined values                      |
| ExerciseType  | ExerciseType (enum)     | Yes      | One of 3 predefined values                      |
| ImagePath     | string?                 | No       | Relative URL to uploaded image, max 500 chars    |
| Model         | string?                 | No       | Free-text equipment model description, max 500 chars |
| IsDeleted     | bool                    | Yes      | Default false, used for soft delete              |
| DeletedAt     | DateTimeOffset?         | No       | Set when soft-deleted                            |
| UserId        | string (FK)             | Yes      | References ApplicationUser.Id                    |
| Created       | DateTimeOffset          | Yes      | Auto-set (BaseAuditableEntity)                   |
| CreatedBy     | string?                 | Yes      | Auto-set (BaseAuditableEntity)                   |
| LastModified  | DateTimeOffset          | Yes      | Auto-set (BaseAuditableEntity)                   |
| LastModifiedBy| string?                 | Yes      | Auto-set (BaseAuditableEntity)                   |

**Relationships**:
- Belongs to one User (via UserId)
- Referenced by many WorkoutTemplateExercise records

**Global Query Filter**: `e => !e.IsDeleted` (EF Core)

### WorkoutTemplate

Represents a reusable workout plan.

| Field    | Type           | Required | Constraints                             |
|----------|----------------|----------|-----------------------------------------|
| Id       | int (PK)       | Yes      | Auto-generated                          |
| Name     | string         | Yes      | Max 200 chars, not empty                |
| Notes    | string?        | No       | Max 2000 chars                          |
| Location | string?        | No       | Max 200 chars, free-text                |
| UserId   | string (FK)    | Yes      | References ApplicationUser.Id           |
| Created       | DateTimeOffset | Yes | Auto-set (BaseAuditableEntity)         |
| CreatedBy     | string?        | Yes | Auto-set (BaseAuditableEntity)         |
| LastModified  | DateTimeOffset | Yes | Auto-set (BaseAuditableEntity)         |
| LastModifiedBy| string?        | Yes | Auto-set (BaseAuditableEntity)         |

**Relationships**:
- Belongs to one User (via UserId)
- Has many WorkoutTemplateExercise records (ordered by Position)

### WorkoutTemplateExercise

Join entity between WorkoutTemplate and ExerciseTemplate. Preserves order and allows duplicates.

| Field              | Type     | Required | Constraints                                  |
|--------------------|----------|----------|----------------------------------------------|
| Id                 | int (PK) | Yes      | Auto-generated                               |
| WorkoutTemplateId  | int (FK) | Yes      | References WorkoutTemplate.Id, cascade delete |
| ExerciseTemplateId | int (FK) | Yes      | References ExerciseTemplate.Id, restrict delete |
| Position           | int      | Yes      | 1-based sequential ordering                  |

**Relationships**:
- Belongs to one WorkoutTemplate (cascade delete — when workout template is deleted, associations are removed)
- References one ExerciseTemplate (restrict delete — cannot hard-delete an exercise that is referenced; soft-delete is used instead)

## Enums

### ImplementType

```
Barbell = 0
Dumbbell = 1
SelectorizedMachine = 2
PlateLoadedMachine = 3
Bodyweight = 4
Band = 5
Kettlebell = 6
Plate = 7
MedicineBall = 8
```

### ExerciseType

```
Reps = 0
Duration = 1
Distance = 2
```

## Indexes

| Table                    | Columns                        | Type   | Notes                                           |
|--------------------------|--------------------------------|--------|-------------------------------------------------|
| ExerciseTemplate         | (UserId, Name)                 | Unique | Filtered: WHERE IsDeleted = 0                   |
| ExerciseTemplate         | (UserId, IsDeleted)            | Index  | Supports listing user's active exercises         |
| WorkoutTemplate          | (UserId)                       | Index  | Supports listing user's workout templates        |
| WorkoutTemplateExercise  | (WorkoutTemplateId, Position)  | Index  | Supports ordered exercise retrieval              |

## Entity Relationship Diagram (textual)

```
ApplicationUser (1) ──── (*) WorkoutTemplate
ApplicationUser (1) ──── (*) ExerciseTemplate

WorkoutTemplate (1) ──── (*) WorkoutTemplateExercise (*) ──── (1) ExerciseTemplate
```

## State Transitions

### ExerciseTemplate Lifecycle

```
[Created] → Active (IsDeleted=false)
Active → Soft-Deleted (IsDeleted=true, DeletedAt=now)
```

- Active exercises appear in library and exercise picker
- Soft-deleted exercises are hidden from library and picker but remain visible in existing workout template associations

### WorkoutTemplate Lifecycle

```
[Created] → Active
Active → Deleted (hard delete, cascade removes WorkoutTemplateExercise rows)
```

## Validation Rules

| Entity                   | Field         | Rule                                                        |
|--------------------------|---------------|-------------------------------------------------------------|
| WorkoutTemplate          | Name          | Not empty, max 200 characters                               |
| WorkoutTemplate          | Notes         | Max 2000 characters (if provided)                           |
| WorkoutTemplate          | Location      | Max 200 characters (if provided)                            |
| ExerciseTemplate         | Name          | Not empty, max 200 characters, unique per user              |
| ExerciseTemplate         | ImplementType | Must be a valid ImplementType enum value                    |
| ExerciseTemplate         | ExerciseType  | Must be a valid ExerciseType enum value                     |
| ExerciseTemplate         | ImagePath     | Max 500 characters (if provided)                            |
| ExerciseTemplate         | Model         | Max 500 characters (if provided)                            |
| Image Upload             | File          | Max 5 MB, JPEG/PNG/WebP only                               |
| WorkoutTemplateExercise  | Position      | Positive integer, sequential within workout template        |
