# Feature Specification: Gym Locations

**Feature Branch**: `003-gym-locations`
**Created**: 2026-02-14
**Status**: Draft
**Input**: User description: "I want to be able to set my locations in my app so that if I go to my normal gym 'The Refinery' I can filter my workout templates by that location. Similarly I want to associate an exercise template with a location. Both workout templates and exercise templates can have an optional location association. A location is a name, instagram handle, coordinates, notes, address. I want to be able to filter exercise templates and workout templates by location."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Manage Locations (Priority: P1)

The user can create locations representing gyms or training spaces they frequent. Each location has a name (required), and optionally an Instagram handle, GPS coordinates, notes, and address. The user can view all their locations, edit any location's details, and delete locations they no longer use.

**Why this priority**: Locations are the foundational entity for this feature. Without the ability to create and manage locations, no filtering or association is possible.

**Independent Test**: Can be fully tested by creating a new location with a name and optional details, verifying it appears in the locations list, editing it, and deleting it.

**Acceptance Scenarios**:

1. **Given** a logged-in user, **When** they navigate to "My Locations" in the settings/profile area, **Then** they see a list of all their saved locations
2. **Given** a logged-in user, **When** they choose to create a new location, **Then** they are prompted to enter a name (required), Instagram handle (optional), coordinates (optional), notes (optional), and address (optional)
3. **Given** a user creating a location, **When** they provide a valid name and submit, **Then** the location is saved and appears in their locations list
4. **Given** a user creating a location, **When** they submit without a name, **Then** the system displays a validation error indicating the name is required
5. **Given** a user with an existing location, **When** they choose to edit it, **Then** they can modify any of its fields
6. **Given** a user with an existing location, **When** they choose to delete it, **Then** the system shows a confirmation prompt
7. **Given** a user who confirms deletion of a location, **When** the deletion completes, **Then** the location is hidden from the locations list and all pickers, but its name remains visible on any previously associated workout templates and exercise templates
8. **Given** a logged-in user, **When** they view their locations list, **Then** they cannot see locations belonging to other users

---

### User Story 2 - Associate Locations with Workout Templates (Priority: P1)

While creating or editing a workout template, the user can optionally associate it with one of their saved locations. This replaces the existing free-text location field with a proper location reference. The user can also clear the location association.

**Why this priority**: Associating locations with workout templates is a core interaction that enables the filtering feature and directly addresses the user's primary use case (filtering workouts by gym).

**Independent Test**: Can be fully tested by editing a workout template, selecting a location from the user's saved locations, saving, and verifying the association persists.

**Acceptance Scenarios**:

1. **Given** a user creating or editing a workout template, **When** they tap the location field, **Then** they see a list of their saved locations to choose from
2. **Given** a user selecting a location for a workout template, **When** they pick a location, **Then** the location is associated with the workout template
3. **Given** a user editing a workout template with an existing location, **When** they clear the location, **Then** the workout template's location is set to none
4. **Given** a user creating or editing a workout template, **When** they want to use a location that doesn't exist yet, **Then** they can create a new location inline and it is immediately available for selection
5. **Given** a user who has associated a location with a workout template, **When** they view the workout template, **Then** the location name is displayed

---

### User Story 3 - Associate Locations with Exercise Templates (Priority: P2)

While creating or editing an exercise template, the user can optionally associate it with one of their saved locations. This allows exercises to be tagged with the gym where specific equipment is available. The user can also clear the location association.

**Why this priority**: While valuable for filtering and organization, exercise-location association is secondary to workout-location association since users typically think of their workout routine in terms of where they go, not individual exercises.

**Independent Test**: Can be fully tested by editing an exercise template, selecting a location, saving, and verifying the association persists.

**Acceptance Scenarios**:

1. **Given** a user creating or editing an exercise template, **When** they tap the location field, **Then** they see a list of their saved locations to choose from
2. **Given** a user selecting a location for an exercise template, **When** they pick a location, **Then** the location is associated with the exercise template
3. **Given** a user editing an exercise template with an existing location, **When** they clear the location, **Then** the exercise template's location is set to none
4. **Given** a user creating or editing an exercise template, **When** they want to use a location that doesn't exist yet, **Then** they can create a new location inline
5. **Given** a user who has associated a location with an exercise template, **When** they view the exercise template, **Then** the location name is displayed

---

### User Story 4 - Filter Workout Templates by Location (Priority: P1)

On the landing page, the user can filter their workout templates by location. When a location filter is active, only workout templates associated with that location are shown. The user can clear the filter to see all workout templates again.

**Why this priority**: This is the primary motivation for the feature — the user wants to quickly see which workout templates are relevant to the gym they are currently at.

**Independent Test**: Can be fully tested by having multiple workout templates associated with different locations, applying a location filter, and verifying only matching templates are shown.

**Acceptance Scenarios**:

1. **Given** a logged-in user on the landing page with workout templates associated with different locations, **When** they select a location filter, **Then** only workout templates associated with that location are displayed
2. **Given** a user with a location filter active, **When** they clear the filter, **Then** all workout templates are displayed again
3. **Given** a user filtering by a location, **When** no workout templates are associated with that location, **Then** an empty state is displayed indicating no workout templates match
4. **Given** a user with workout templates that have no location set, **When** they filter by a specific location, **Then** unassociated workout templates are not shown
5. **Given** a user filtering workout templates by location, **When** other filters or search are also active, **Then** all filters are applied together (AND logic)

---

### User Story 5 - Filter Exercise Templates by Location (Priority: P2)

In the exercise library on the landing page and in the exercise picker (when adding exercises to a workout template), the user can filter exercises by location. This helps the user find exercises available at a specific gym.

**Why this priority**: Filtering exercises by location is useful but secondary to filtering workouts, which is the user's primary described use case.

**Independent Test**: Can be fully tested by having exercises associated with different locations, applying a location filter in the exercise library, and verifying only matching exercises are shown.

**Acceptance Scenarios**:

1. **Given** a logged-in user viewing their exercise library, **When** they select a location filter, **Then** only exercise templates associated with that location are displayed
2. **Given** a user adding exercises to a workout template, **When** they select a location filter in the exercise picker, **Then** only exercises associated with that location are shown
3. **Given** a user with a location filter active in the exercise library, **When** they clear the filter, **Then** all exercises are displayed again
4. **Given** a user filtering exercises by location, **When** other filters (implement type, exercise type) or search are also active, **Then** all filters are applied together (AND logic)

---

### Edge Cases

- What happens when a user deletes a location that is associated with workout templates or exercise templates? The location is soft-deleted — hidden from the locations list and location pickers, but its name remains visible on any previously associated workout templates and exercise templates. The templates themselves are not affected.
- What happens when a user tries to create a location with a name that already exists? The system allows duplicate location names, as a user may have different entries for branches of the same gym chain.
- What happens when a user enters an Instagram handle without the "@" prefix? The system accepts it with or without the "@" prefix and stores it consistently (without "@").
- What happens when a user enters invalid GPS coordinates? The system validates that latitude is between -90 and 90 and longitude is between -180 and 180, and displays a validation error for invalid values.
- What happens to the existing free-text location column on workout templates? The free-text location field was never surfaced in the mobile app UI, so the column is dropped entirely during migration. No data loss occurs.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to create a location with a name (required), Instagram handle (optional), GPS coordinates (optional), notes (optional), and address (optional)
- **FR-002**: System MUST validate that location name is not empty before saving
- **FR-003**: System MUST allow users to view a list of all their saved locations via a "My Locations" screen in the settings/profile area
- **FR-004**: System MUST allow users to edit any field of an existing location
- **FR-005**: System MUST allow users to soft-delete a location after confirming via a confirmation prompt
- **FR-006**: System MUST hide soft-deleted locations from the locations list and all location pickers, while preserving the location name on any previously associated workout templates and exercise templates
- **FR-007**: System MUST allow users to optionally associate a workout template with one of their saved locations
- **FR-008**: System MUST allow users to optionally associate an exercise template with one of their saved locations
- **FR-009**: System MUST allow users to clear the location association from a workout template or exercise template
- **FR-010**: System MUST allow users to create a new location inline when selecting a location for a workout template or exercise template
- **FR-011**: System MUST allow users to filter workout templates on the landing page by location
- **FR-012**: System MUST allow users to filter exercise templates in the exercise library by location
- **FR-013**: System MUST allow users to filter exercises in the exercise picker (when adding to a workout template) by location
- **FR-014**: System MUST apply location filters in combination with existing filters (search, implement type, exercise type) using AND logic
- **FR-015**: System MUST ensure users can only view and manage their own locations
- **FR-016**: System MUST validate GPS coordinates when provided: latitude between -90 and 90, longitude between -180 and 180
- **FR-017**: System MUST store Instagram handles consistently without the "@" prefix, regardless of how the user enters them
- **FR-018**: System MUST display the associated location name on workout templates and exercise templates that have one

### Key Entities

- **Location**: Represents a gym, studio, or training space the user frequents. Has a name (required), Instagram handle (optional), GPS coordinates as latitude and longitude (optional), notes (optional), and address (optional). Belongs to a single user. Can be referenced by multiple workout templates and exercise templates. Supports soft-delete: when deleted, hidden from lists and pickers but name remains visible on associated templates.
- **Workout Template** (modified): Gains an optional reference to a Location entity. The previous free-text location column (never user-facing) is dropped.
- **Exercise Template** (modified): Gains an optional reference to a Location entity.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new location in under 30 seconds
- **SC-002**: Users can associate a location with a workout template in under 5 seconds (two taps: open picker, select location)
- **SC-003**: Users can filter workout templates by location in under 3 seconds (single tap on filter)
- **SC-004**: Location filter results display within 1 second of selection
- **SC-005**: 90% of users can successfully create a location and filter their workouts by it on first attempt
- **SC-006**: The locations list remains responsive with up to 50 saved locations

## Clarifications

### Session 2026-02-14

- Q: How should the existing free-text location field on workout templates be handled? → A: Drop the column entirely — it was never surfaced in the mobile app UI, so no data to migrate.
- Q: Where does location management live in app navigation? → A: Settings/profile area — a "My Locations" screen under user settings. Location management is a setup task, not a daily action.
- Q: Location deletion — hard delete or soft delete? → A: Soft delete — location hidden from UI and pickers, but name still visible on previously associated templates.

## Assumptions

- Users are authenticated before accessing any location features (login handled by existing auth system from feature 001)
- Duplicate location names are allowed to accommodate different branches or interpretations
- A workout template or exercise template can be associated with at most one location (one-to-many: one location to many templates)
- GPS coordinates are entered manually by the user (no automatic geolocation in this iteration)
- Instagram handle is stored as a text field for reference purposes; no Instagram API integration
- The location filter appears as a dropdown or chip selector alongside existing filter controls
- When a location is soft-deleted, it is hidden from UI and pickers but its name remains visible on previously associated templates; the templates themselves are unaffected
- The existing free-text "Location" column on workout templates (from feature 002) was never exposed in the mobile app and will be dropped entirely — no migration needed
