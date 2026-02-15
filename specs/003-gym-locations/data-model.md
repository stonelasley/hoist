# Data Model: Gym Locations

**Feature**: 003-gym-locations
**Date**: 2026-02-14

## New Entity: Location

```
Location : BaseAuditableEntity
├── Id              : int (PK, auto-increment) [inherited]
├── Name            : string (required, max 200)
├── InstagramHandle : string? (max 30, stored without "@")
├── Latitude        : decimal? (precision 9, scale 6, range -90 to 90)
├── Longitude       : decimal? (precision 10, scale 6, range -180 to 180)
├── Notes           : string? (max 2000)
├── Address         : string? (max 500)
├── UserId          : string (required, FK → AspNetUsers)
├── IsDeleted       : bool (default false)
├── DeletedAt       : DateTimeOffset? (set on soft-delete)
├── Created         : DateTimeOffset [inherited]
├── CreatedBy       : string? [inherited]
├── LastModified    : DateTimeOffset [inherited]
└── LastModifiedBy  : string? [inherited]
```

### Indexes
- `IX_Location_UserId_IsDeleted` → composite index on (UserId, IsDeleted) for list queries
- Global query filter: `e => !e.IsDeleted` (soft-delete exclusion)

### Relationships
- Location → ApplicationUser: many-to-one (FK: UserId, DeleteBehavior.Cascade)
- Location → WorkoutTemplate: one-to-many (inverse FK on WorkoutTemplate)
- Location → ExerciseTemplate: one-to-many (inverse FK on ExerciseTemplate)

### Validation Rules
- Name: required, non-empty, max 200 characters
- InstagramHandle: if provided, strip leading "@", max 30 characters
- Latitude: if provided, must be between -90 and 90
- Longitude: if provided, must be between -180 and 180
- Notes: max 2000 characters
- Address: max 500 characters
- Coordinates: if one of latitude/longitude is provided, both must be provided

---

## Modified Entity: WorkoutTemplate

### Changes
```diff
  WorkoutTemplate : BaseAuditableEntity
    Name            : string (required, max 200)
    Notes           : string? (max 2000)
-   Location        : string? (max 200)          # REMOVED: free-text field
+   LocationId      : int? (FK → Location)       # NEW: optional FK to Location
+   Location        : Location? (navigation)     # NEW: navigation property
    UserId          : string (required)
    Exercises       : IList<WorkoutTemplateExercise>
```

### New Index
- FK index on `LocationId`

### FK Configuration
- WorkoutTemplate.LocationId → Location.Id
- DeleteBehavior.SetNull (safety net; normal deletion is soft-delete on Location)

---

## Modified Entity: ExerciseTemplate

### Changes
```diff
  ExerciseTemplate : BaseAuditableEntity
    Name            : string (required, max 200)
    ImplementType   : ImplementType (required)
    ExerciseType    : ExerciseType (required)
    ImagePath       : string? (max 500)
    Model           : string? (max 500)
    IsDeleted       : bool (default false)
    DeletedAt       : DateTimeOffset?
+   LocationId      : int? (FK → Location)       # NEW: optional FK to Location
+   Location        : Location? (navigation)     # NEW: navigation property
    UserId          : string (required)
    WorkoutTemplateExercises : IList<WorkoutTemplateExercise>
```

### New Index
- FK index on `LocationId`

### FK Configuration
- ExerciseTemplate.LocationId → Location.Id
- DeleteBehavior.SetNull

---

## Entity Relationship Diagram

```
┌──────────────────┐
│ ApplicationUser  │
│ (AspNetUsers)    │
└──────┬───────────┘
       │ 1
       │
       ├──────────────────┐──────────────────┐
       │ *                │ *                │ *
┌──────┴──────┐   ┌───────┴───────┐   ┌─────┴───────┐
│  Location   │   │WorkoutTemplate│   │ExerciseTempl│
│             │   │               │   │             │
│ Id (PK)     │   │ Id (PK)       │   │ Id (PK)     │
│ Name        │   │ Name          │   │ Name        │
│ Instagram   │   │ Notes         │   │ ImplementTyp│
│ Lat/Lng     │   │ LocationId(FK)│──▶│ ExerciseType│
│ Notes       │   │ UserId (FK)   │   │ LocationId? │
│ Address     │   │ IsDeleted=N/A │   │ IsDeleted   │
│ UserId (FK) │   └───────┬───────┘   │ UserId (FK) │
│ IsDeleted   │           │ 1         └──────┬──────┘
└─────────────┘           │                  │ *
       ▲                  │ *                │
       │ 0..1      ┌──────┴──────────────────┘
       │           │
       └───────────┤  WorkoutTemplateExercise
                   │  (Join table)
                   │  WorkoutTemplateId (FK)
                   │  ExerciseTemplateId (FK)
                   │  Position
                   └─────────────────────────
```

Location ◀── 0..1 ──── WorkoutTemplate (optional FK)
Location ◀── 0..1 ──── ExerciseTemplate (optional FK)

---

## Seed Data Updates

Add to `ApplicationDbContextInitialiser.TrySeedAsync()` for the test user:

```
Location: "The Refinery"
  InstagramHandle: "therefinerysc"
  Latitude: null
  Longitude: null
  Notes: "Main gym"
  Address: null

Location: "Home Gym"
  InstagramHandle: null
  Latitude: null
  Longitude: null
  Notes: "Garage setup"
  Address: null
```

Associate existing seed workout template "Push Day" with "The Refinery" location.
Associate existing seed exercise "Bench Press" and "Squat" with "The Refinery" location.
