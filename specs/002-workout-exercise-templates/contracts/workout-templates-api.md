# API Contract: Workout Templates

**Base Path**: `/api/WorkoutTemplates`
**Auth**: All endpoints require Bearer token authentication

---

## GET /api/WorkoutTemplates

List the current user's workout templates.

**Query Parameters**: None (returns all for current user)

**Response 200**:
```json
[
  {
    "id": 1,
    "name": "Push Day",
    "notes": "Chest, shoulders, triceps",
    "location": "Home Gym",
    "created": "2026-02-14T10:00:00Z",
    "lastModified": "2026-02-14T12:30:00Z",
    "exerciseCount": 5
  }
]
```

**Response DTOs**:
- `WorkoutTemplateBriefDto`: id, name, notes, location, created, lastModified, exerciseCount

---

## GET /api/WorkoutTemplates/{id}

Get a single workout template with its exercises.

**Path Parameters**:
- `id` (int, required): Workout template ID

**Response 200**:
```json
{
  "id": 1,
  "name": "Push Day",
  "notes": "Chest, shoulders, triceps",
  "location": "Home Gym",
  "created": "2026-02-14T10:00:00Z",
  "lastModified": "2026-02-14T12:30:00Z",
  "exercises": [
    {
      "id": 10,
      "exerciseTemplateId": 5,
      "exerciseName": "Bench Press",
      "implementType": "Barbell",
      "exerciseType": "Reps",
      "position": 1
    },
    {
      "id": 11,
      "exerciseTemplateId": 3,
      "exerciseName": "Overhead Press",
      "implementType": "Dumbbell",
      "exerciseType": "Reps",
      "position": 2
    }
  ]
}
```

**Response DTOs**:
- `WorkoutTemplateDetailDto`: id, name, notes, location, created, lastModified, exercises[]
- `WorkoutTemplateExerciseDto`: id, exerciseTemplateId, exerciseName, implementType, exerciseType, position

**Response 404**: Workout template not found or not owned by user

---

## POST /api/WorkoutTemplates

Create a new workout template.

**Request Body**:
```json
{
  "name": "Push Day",
  "notes": "Chest, shoulders, triceps",
  "location": "Home Gym"
}
```

**Validation**:
- `name`: required, max 200 chars
- `notes`: optional, max 2000 chars
- `location`: optional, max 200 chars

**Response 201**: Returns created ID
```json
1
```

**Response 400**: Validation errors

---

## PUT /api/WorkoutTemplates/{id}

Update an existing workout template.

**Path Parameters**:
- `id` (int, required): Workout template ID

**Request Body**:
```json
{
  "id": 1,
  "name": "Push Day (Updated)",
  "notes": "Chest, shoulders, triceps focus",
  "location": "Planet Fitness"
}
```

**Validation**:
- `id`: must match path parameter
- `name`: required, max 200 chars
- `notes`: optional, max 2000 chars
- `location`: optional, max 200 chars

**Response 204**: Success (no content)
**Response 400**: Validation errors or ID mismatch
**Response 404**: Not found or not owned by user

---

## DELETE /api/WorkoutTemplates/{id}

Delete a workout template and its exercise associations.

**Path Parameters**:
- `id` (int, required): Workout template ID

**Response 204**: Success (no content)
**Response 404**: Not found or not owned by user

---

## PUT /api/WorkoutTemplates/{id}/Exercises

Replace the full exercise list for a workout template (add, remove, reorder in a single operation).

**Path Parameters**:
- `id` (int, required): Workout template ID

**Request Body**:
```json
{
  "workoutTemplateId": 1,
  "exercises": [
    { "exerciseTemplateId": 5 },
    { "exerciseTemplateId": 3 },
    { "exerciseTemplateId": 5 }
  ]
}
```

The order of items in the array determines the Position values (1-based).

**Validation**:
- `workoutTemplateId`: must match path parameter
- `exercises`: array, each item must have a valid `exerciseTemplateId` owned by the current user
- Duplicate `exerciseTemplateId` values are allowed

**Response 204**: Success (no content)
**Response 400**: Validation errors
**Response 404**: Workout template not found or not owned by user
