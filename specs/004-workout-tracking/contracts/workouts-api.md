# API Contract: Workouts

**Base Path**: `/api/Workouts`
**Auth**: All endpoints require Bearer token authentication

---

## POST /api/Workouts

Start a new workout from a workout template.

**Request Body**:
```json
{
  "workoutTemplateId": 1
}
```

**Validation**:
- `workoutTemplateId`: required, must reference a valid workout template owned by the current user
- User must not have another workout in InProgress status

**Response 201**: Returns created workout ID
```json
1
```

**Response 400**: Validation errors (e.g., another workout already in progress)
**Response 404**: Workout template not found or not owned by user

**Behavior**:
- Creates a Workout with Status=InProgress, StartedAt=now
- Snapshots TemplateName from the template
- Snapshots LocationId and LocationName from the template (if set)
- Creates WorkoutExercise records for each template exercise, snapshotting ExerciseName, ImplementType, ExerciseType

---

## GET /api/Workouts/InProgress

Get the current user's in-progress workout (if any).

**Response 200**:
```json
{
  "id": 1,
  "templateName": "Push Day",
  "status": "InProgress",
  "startedAt": "2026-02-15T10:00:00Z",
  "endedAt": null,
  "notes": null,
  "rating": null,
  "locationId": 5,
  "locationName": "Iron Works Gym",
  "exercises": [
    {
      "id": 10,
      "exerciseTemplateId": 5,
      "exerciseName": "Bench Press",
      "implementType": "Barbell",
      "exerciseType": "Reps",
      "position": 1,
      "sets": [
        {
          "id": 100,
          "position": 1,
          "weight": 135.0,
          "reps": 10,
          "duration": null,
          "distance": null,
          "bodyweight": null,
          "bandColor": null,
          "weightUnit": "Lbs",
          "distanceUnit": null
        }
      ]
    }
  ]
}
```

**Response 204**: No in-progress workout

**Response DTOs**:
- `WorkoutDetailDto`: id, templateName, status, startedAt, endedAt, notes, rating, locationId, locationName, exercises[]
- `WorkoutExerciseDto`: id, exerciseTemplateId, exerciseName, implementType, exerciseType, position, sets[]
- `WorkoutSetDto`: id, position, weight, reps, duration, distance, bodyweight, bandColor, weightUnit, distanceUnit

---

## GET /api/Workouts/Recent

Get the current user's 3 most recently completed workouts.

**Response 200**:
```json
[
  {
    "id": 3,
    "templateName": "Push Day",
    "startedAt": "2026-02-15T10:00:00Z",
    "endedAt": "2026-02-15T11:15:00Z",
    "rating": 4,
    "locationName": "Iron Works Gym"
  }
]
```

**Response DTOs**:
- `WorkoutBriefDto`: id, templateName, startedAt, endedAt, rating, locationName

---

## GET /api/Workouts

List the current user's completed workout history with sorting, filtering, and search.

**Query Parameters**:
- `sortBy` (string, optional): `date` (default) or `rating`
- `sortDirection` (string, optional): `desc` (default) or `asc`
- `locationId` (int, optional): Filter by location
- `minRating` (int, optional): Filter by minimum rating (1-5)
- `search` (string, optional): Search notes text (case-insensitive LIKE)
- `cursor` (string, optional): Pagination cursor (opaque token encoding EndedAt + Id)
- `pageSize` (int, optional): Number of results per page (default 20, max 50)

**Response 200**:
```json
{
  "items": [
    {
      "id": 3,
      "templateName": "Push Day",
      "startedAt": "2026-02-15T10:00:00Z",
      "endedAt": "2026-02-15T11:15:00Z",
      "rating": 4,
      "locationName": "Iron Works Gym"
    }
  ],
  "nextCursor": "eyJlIjoiMjAyNi0wMi0xNVQxMToxNTowMFoiLCJpIjozfQ=="
}
```

**Response DTOs**:
- `PaginatedList<WorkoutBriefDto>`: items[], nextCursor

---

## GET /api/Workouts/{id}

Get a single workout with all exercises and sets.

**Path Parameters**:
- `id` (int, required): Workout ID

**Response 200**: Full `WorkoutDetailDto` (same structure as InProgress response)

**Response 404**: Workout not found or not owned by user

---

## PUT /api/Workouts/{id}

Update a workout (location, notes, rating, start/end times). Works for both in-progress and completed workouts.

**Path Parameters**:
- `id` (int, required): Workout ID

**Request Body**:
```json
{
  "id": 1,
  "locationId": 5,
  "notes": "Great session, hit a new PR on bench",
  "rating": 5,
  "startedAt": "2026-02-15T10:00:00Z",
  "endedAt": "2026-02-15T11:15:00Z"
}
```

**Validation**:
- `id`: must match path parameter
- `notes`: max 2000 chars
- `rating`: 1-5 (if provided)
- `endedAt`: must be after startedAt (if provided)

**Response 204**: Success
**Response 400**: Validation errors
**Response 404**: Not found or not owned by user

---

## PUT /api/Workouts/{id}/Complete

Complete an in-progress workout.

**Path Parameters**:
- `id` (int, required): Workout ID

**Request Body**:
```json
{
  "id": 1,
  "notes": "Good workout",
  "rating": 4,
  "startedAt": "2026-02-15T10:00:00Z",
  "endedAt": "2026-02-15T11:15:00Z"
}
```

**Validation**:
- `id`: must match path parameter
- Workout must be in InProgress status
- `endedAt`: defaults to current time if not provided, must be after startedAt
- `notes`: max 2000 chars
- `rating`: 1-5 (optional)

**Response 204**: Success
**Response 400**: Validation errors or workout not in-progress
**Response 404**: Not found or not owned by user

---

## DELETE /api/Workouts/{id}

Discard an in-progress workout (hard delete). Only works for InProgress workouts.

**Path Parameters**:
- `id` (int, required): Workout ID

**Validation**:
- Workout must be in InProgress status

**Response 204**: Success
**Response 400**: Cannot delete completed workouts
**Response 404**: Not found or not owned by user

---

## PUT /api/Workouts/{id}/Exercises

Replace/reorder exercises in an in-progress workout. Allows adding new exercises (by exerciseTemplateId), removing exercises, and reordering.

**Path Parameters**:
- `id` (int, required): Workout ID

**Request Body**:
```json
{
  "workoutId": 1,
  "exercises": [
    { "exerciseTemplateId": 5 },
    { "exerciseTemplateId": 3 },
    { "exerciseTemplateId": 8 }
  ]
}
```

**Validation**:
- `workoutId`: must match path parameter
- Workout must be in InProgress status
- Each `exerciseTemplateId` must reference a valid exercise template owned by the current user

**Behavior**:
- Removes exercises not in the new list (and their sets via cascade)
- Adds new exercises with snapshot fields
- Reorders based on array position
- Preserves existing sets for exercises that remain

**Response 204**: Success
**Response 400**: Validation errors
**Response 404**: Not found or not owned by user

---

## POST /api/Workouts/{workoutId}/Exercises/{exerciseId}/Sets

Add a set to an exercise in a workout.

**Path Parameters**:
- `workoutId` (int, required): Workout ID
- `exerciseId` (int, required): WorkoutExercise ID

**Request Body**:
```json
{
  "weight": 135.0,
  "reps": 10,
  "duration": null,
  "distance": null,
  "bodyweight": null,
  "bandColor": null,
  "weightUnit": "Lbs",
  "distanceUnit": null
}
```

**Validation**:
- Workout must be owned by current user
- WorkoutExercise must belong to the specified workout
- Numeric fields: non-negative (if provided)
- At least one measurement field must be populated

**Response 201**: Returns created set ID
```json
100
```

**Response 400**: Validation errors
**Response 404**: Workout or exercise not found

---

## PUT /api/Workouts/{workoutId}/Exercises/{exerciseId}/Sets/{setId}

Update an existing set.

**Path Parameters**:
- `workoutId` (int, required): Workout ID
- `exerciseId` (int, required): WorkoutExercise ID
- `setId` (int, required): WorkoutSet ID

**Request Body**: Same structure as POST

**Response 204**: Success
**Response 400**: Validation errors
**Response 404**: Not found or not owned by user

---

## DELETE /api/Workouts/{workoutId}/Exercises/{exerciseId}/Sets/{setId}

Delete a set from an exercise.

**Path Parameters**:
- `workoutId` (int, required): Workout ID
- `exerciseId` (int, required): WorkoutExercise ID
- `setId` (int, required): WorkoutSet ID

**Response 204**: Success
**Response 404**: Not found or not owned by user
