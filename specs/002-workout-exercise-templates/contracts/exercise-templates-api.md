# API Contract: Exercise Templates

**Base Path**: `/api/ExerciseTemplates`
**Auth**: All endpoints require Bearer token authentication

---

## GET /api/ExerciseTemplates

List the current user's exercise templates (active only, excludes soft-deleted).

**Query Parameters**:
- `search` (string, optional): Case-insensitive name search (LIKE %search%)
- `implementType` (int, optional): Filter by ImplementType enum value
- `exerciseType` (int, optional): Filter by ExerciseType enum value

**Response 200**:
```json
[
  {
    "id": 5,
    "name": "Bench Press",
    "implementType": "Barbell",
    "exerciseType": "Reps",
    "imagePath": "/uploads/exercises/bench-press-abc123.jpg",
    "model": "Rogue Monster Bench"
  },
  {
    "id": 3,
    "name": "Running",
    "implementType": "Bodyweight",
    "exerciseType": "Distance",
    "imagePath": null,
    "model": null
  }
]
```

**Response DTOs**:
- `ExerciseTemplateBriefDto`: id, name, implementType (string), exerciseType (string), imagePath, model

**Notes**:
- Enum values are serialized as strings in responses (e.g., "Barbell", not 0)
- Filters can be combined (AND logic)

---

## GET /api/ExerciseTemplates/{id}

Get a single exercise template.

**Path Parameters**:
- `id` (int, required): Exercise template ID

**Response 200**:
```json
{
  "id": 5,
  "name": "Bench Press",
  "implementType": "Barbell",
  "exerciseType": "Reps",
  "imagePath": "/uploads/exercises/bench-press-abc123.jpg",
  "model": "Rogue Monster Bench",
  "created": "2026-02-14T10:00:00Z",
  "lastModified": "2026-02-14T12:30:00Z"
}
```

**Response DTOs**:
- `ExerciseTemplateDetailDto`: id, name, implementType, exerciseType, imagePath, model, created, lastModified

**Response 404**: Not found, soft-deleted, or not owned by user

---

## POST /api/ExerciseTemplates

Create a new exercise template.

**Request Body**:
```json
{
  "name": "Bench Press",
  "implementType": 0,
  "exerciseType": 0,
  "model": "Rogue Monster Bench"
}
```

**Validation**:
- `name`: required, max 200 chars, must be unique among user's active exercises
- `implementType`: required, valid ImplementType enum value (0-8)
- `exerciseType`: required, valid ExerciseType enum value (0-2)
- `model`: optional, max 500 chars

**Response 201**: Returns created ID
```json
5
```

**Response 400**: Validation errors (including duplicate name)

**Note**: Image is uploaded separately via the image upload endpoint after creation.

---

## PUT /api/ExerciseTemplates/{id}

Update an existing exercise template.

**Path Parameters**:
- `id` (int, required): Exercise template ID

**Request Body**:
```json
{
  "id": 5,
  "name": "Flat Bench Press",
  "implementType": 0,
  "exerciseType": 0,
  "model": "Rogue Monster Bench 2.0"
}
```

**Validation**:
- `id`: must match path parameter
- `name`: required, max 200 chars, must be unique among user's active exercises (excluding self)
- `implementType`: required, valid ImplementType enum value
- `exerciseType`: required, valid ExerciseType enum value
- `model`: optional, max 500 chars

**Response 204**: Success (no content)
**Response 400**: Validation errors or ID mismatch
**Response 404**: Not found, soft-deleted, or not owned by user

---

## DELETE /api/ExerciseTemplates/{id}

Soft-delete an exercise template. Hides from library but preserves in existing workout templates.

**Path Parameters**:
- `id` (int, required): Exercise template ID

**Response 204**: Success (no content)
**Response 404**: Not found, already soft-deleted, or not owned by user

---

## POST /api/ExerciseTemplates/{id}/Image

Upload or replace the image for an exercise template.

**Path Parameters**:
- `id` (int, required): Exercise template ID

**Request**: `multipart/form-data`
- `file` (file, required): Image file (JPEG, PNG, or WebP)

**Validation**:
- File must be present
- Max size: 5 MB
- Allowed content types: image/jpeg, image/png, image/webp

**Response 200**: Returns the image path
```json
{
  "imagePath": "/uploads/exercises/bench-press-abc123.jpg"
}
```

**Response 400**: Validation errors (file too large, invalid type)
**Response 404**: Exercise template not found or not owned by user

---

## DELETE /api/ExerciseTemplates/{id}/Image

Remove the image from an exercise template.

**Path Parameters**:
- `id` (int, required): Exercise template ID

**Response 204**: Success (no content)
**Response 404**: Exercise template not found or not owned by user

---

## Enum Reference

### ImplementType
| Value | Name               |
|-------|--------------------|
| 0     | Barbell            |
| 1     | Dumbbell           |
| 2     | SelectorizedMachine|
| 3     | PlateLoadedMachine |
| 4     | Bodyweight         |
| 5     | Band               |
| 6     | Kettlebell         |
| 7     | Plate              |
| 8     | MedicineBall       |

### ExerciseType
| Value | Name     |
|-------|----------|
| 0     | Reps     |
| 1     | Duration |
| 2     | Distance |
