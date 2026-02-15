# API Contracts: Gym Locations

**Feature**: 003-gym-locations
**Base Path**: `/api/Locations`
**Auth**: Bearer token required on all endpoints

---

## Endpoints

### GET /api/Locations

List all non-deleted locations for the authenticated user.

**Response**: `200 OK`
```json
[
  {
    "id": 1,
    "name": "The Refinery",
    "instagramHandle": "therefinerysc",
    "latitude": 34.012345,
    "longitude": -81.034567,
    "notes": "Main gym",
    "address": "123 Main St, Columbia, SC",
    "created": "2026-02-14T10:00:00+00:00",
    "lastModified": "2026-02-14T10:00:00+00:00"
  }
]
```

---

### GET /api/Locations/{id}

Get a single location by ID (must belong to authenticated user, must not be deleted).

**Response**: `200 OK`
```json
{
  "id": 1,
  "name": "The Refinery",
  "instagramHandle": "therefinerysc",
  "latitude": 34.012345,
  "longitude": -81.034567,
  "notes": "Main gym",
  "address": "123 Main St, Columbia, SC",
  "created": "2026-02-14T10:00:00+00:00",
  "lastModified": "2026-02-14T10:00:00+00:00"
}
```

**Error Responses**:
- `404 Not Found` — location does not exist or does not belong to user

---

### POST /api/Locations

Create a new location.

**Request**:
```json
{
  "name": "The Refinery",
  "instagramHandle": "@therefinerysc",
  "latitude": 34.012345,
  "longitude": -81.034567,
  "notes": "Main gym",
  "address": "123 Main St, Columbia, SC"
}
```

**Validation**:
- `name`: required, non-empty, max 200 characters
- `instagramHandle`: optional, max 30 characters (leading "@" stripped on save)
- `latitude`: optional, -90 to 90 (if provided, longitude required)
- `longitude`: optional, -180 to 180 (if provided, latitude required)
- `notes`: optional, max 2000 characters
- `address`: optional, max 500 characters

**Response**: `201 Created`
```json
1
```
Header: `Location: /Locations/1`

**Error Responses**:
- `400 Bad Request` — validation errors

---

### PUT /api/Locations/{id}

Update an existing location.

**Request**:
```json
{
  "id": 1,
  "name": "The Refinery - Downtown",
  "instagramHandle": "therefinerysc",
  "latitude": 34.012345,
  "longitude": -81.034567,
  "notes": "Main gym, updated hours",
  "address": "123 Main St, Columbia, SC"
}
```

**Validation**: Same rules as POST, plus `id` in URL must match body.

**Response**: `204 No Content`

**Error Responses**:
- `400 Bad Request` — validation errors or ID mismatch
- `404 Not Found` — location does not exist or does not belong to user

---

### DELETE /api/Locations/{id}

Soft-delete a location. Sets `IsDeleted = true` and `DeletedAt = now`. Location remains in database and its name is still visible on associated templates.

**Response**: `204 No Content`

**Error Responses**:
- `404 Not Found` — location does not exist or does not belong to user

---

## Modified Endpoints

### GET /api/WorkoutTemplates

**New query parameter**: `?locationId={int}`

When provided, filters workout templates to only those associated with the given location.

**Updated response DTO**:
```json
{
  "id": 1,
  "name": "Push Day",
  "notes": "Upper body push",
  "locationId": 1,
  "locationName": "The Refinery",
  "created": "2026-02-14T10:00:00+00:00",
  "lastModified": "2026-02-14T10:00:00+00:00",
  "exerciseCount": 3
}
```

Changes:
- Removed: `location` (string?)
- Added: `locationId` (int?), `locationName` (string?)

---

### POST /api/WorkoutTemplates

**Updated request**:
```json
{
  "name": "Push Day",
  "notes": "Upper body push",
  "locationId": 1
}
```

Changes:
- Removed: `location` (string?)
- Added: `locationId` (int?, optional, must reference a valid non-deleted location belonging to user)

---

### PUT /api/WorkoutTemplates/{id}

**Updated request**:
```json
{
  "id": 1,
  "name": "Push Day",
  "notes": "Upper body push",
  "locationId": 1
}
```

Changes:
- Removed: `location` (string?)
- Added: `locationId` (int?, optional, null to clear association)

---

### GET /api/ExerciseTemplates

**New query parameter**: `?locationId={int}`

When provided, filters exercise templates to only those associated with the given location.

**Updated response DTO**:
```json
{
  "id": 1,
  "name": "Bench Press",
  "implementType": "Barbell",
  "exerciseType": "Reps",
  "locationId": 1,
  "locationName": "The Refinery",
  "imagePath": null,
  "model": null
}
```

Changes:
- Added: `locationId` (int?), `locationName` (string?)

---

### POST /api/ExerciseTemplates

**Updated request** — added `locationId`:
```json
{
  "name": "Bench Press",
  "implementType": 0,
  "exerciseType": 0,
  "locationId": 1
}
```

---

### PUT /api/ExerciseTemplates/{id}

**Updated request** — added `locationId`:
```json
{
  "id": 1,
  "name": "Bench Press",
  "implementType": 0,
  "exerciseType": 0,
  "locationId": null
}
```
