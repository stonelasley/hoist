# API Contract: User Preferences

**Base Path**: `/api/UserPreferences`
**Auth**: All endpoints require Bearer token authentication

---

## GET /api/UserPreferences

Get the current user's preferences. Returns defaults if no preferences have been set yet.

**Response 200**:
```json
{
  "weightUnit": "Lbs",
  "distanceUnit": "Miles",
  "bodyweight": null
}
```

**Response DTOs**:
- `UserPreferencesDto`: weightUnit (string), distanceUnit (string), bodyweight (decimal?)

**Behavior**:
- If no UserPreferences entity exists for the user, returns default values (Lbs, Miles, null) without creating a record
- Created lazily on first PUT

---

## PUT /api/UserPreferences

Create or update the current user's preferences.

**Request Body**:
```json
{
  "weightUnit": "Kg",
  "distanceUnit": "Kilometers",
  "bodyweight": 81.6
}
```

**Validation**:
- `weightUnit`: required, must be "Lbs" or "Kg"
- `distanceUnit`: required, must be "Miles", "Kilometers", "Meters", or "Yards"
- `bodyweight`: optional, positive if provided

**Response 204**: Success (upsert — creates if not exists, updates if exists)
**Response 400**: Validation errors
