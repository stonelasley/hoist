# Feature Specification: Workout & Exercise Templates

**Feature Branch**: `002-workout-exercise-templates`
**Created**: 2026-02-14
**Status**: Draft
**Input**: User description: "Upon logging in I should see a list of my workout templates. The templates should be distinguishable by name. I should be able to create a new workout template or edit an existing. A workout template consists of name, created date, modified date, notes (optional), Location (optional). A workout template has list of exercise templates associated with it. While editing a workout template I can add a new exercise template to my workout template or I can edit an existing. When adding an exercise template to a workout template I can select from a list of existing exercise templates or create a new one. An exercise template has a Name, Implement [Barbell, Dumbbell, Selectorized Machine, Plate Loaded Machine, Bodyweight, Band, Kettlebell, Plate, Medicine Ball], Type [Reps, Duration, Distance], Image (picture of machine, optional), Model (optional). On my landing page along with my list of workouts I should see a list of my exercises in my exercise library."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Landing Page with Workout Templates and Exercise Library (Priority: P1)

After logging in, the user sees a landing page displaying two distinct sections: a list of their workout templates and their exercise library. Workout templates are displayed by name, allowing the user to quickly identify each one. The exercise library shows all exercises the user has created or added.

**Why this priority**: This is the foundational view that gives users immediate access to their core data. Without this, no other feature is usable.

**Independent Test**: Can be fully tested by logging in and verifying both the workout template list and exercise library are visible and populated with data.

**Acceptance Scenarios**:

1. **Given** a logged-in user with existing workout templates, **When** they arrive at the landing page, **Then** they see a list of their workout templates displayed by name
2. **Given** a logged-in user with exercises in their library, **When** they arrive at the landing page, **Then** they see a list of their exercises in the exercise library section
3. **Given** a logged-in user with no workout templates or exercises, **When** they arrive at the landing page, **Then** they see empty states with prompts to create their first workout template and exercise
4. **Given** a logged-in user, **When** they view the landing page, **Then** workout templates and exercises belonging to other users are not visible
5. **Given** a logged-in user with exercises in their library, **When** they type a search term in the exercise library, **Then** the list is filtered to show only exercises whose name contains the search term
6. **Given** a logged-in user with exercises in their library, **When** they filter by implement type or exercise type, **Then** the list shows only exercises matching the selected filter(s)
7. **Given** a logged-in user, **When** they combine a search term with filters, **Then** the results match both the search term and the selected filters

---

### User Story 2 - Create and Edit Workout Templates (Priority: P1)

The user can create a new workout template by providing a name and optionally adding notes and a location. The system automatically records the created and modified dates. The user can also edit an existing workout template to change its name, notes, or location. The modified date updates automatically on each edit.

**Why this priority**: Creating and editing workout templates is the core write operation. Users cannot build their workout library without this.

**Independent Test**: Can be fully tested by creating a new workout template with a name, verifying it appears in the list, then editing it and confirming changes persist.

**Acceptance Scenarios**:

1. **Given** a logged-in user on the landing page, **When** they choose to create a new workout template, **Then** they are prompted to enter a name (required), notes (optional), and location (optional)
2. **Given** a user creating a workout template, **When** they provide a valid name and submit, **Then** the workout template is saved with the current date as both created and modified dates
3. **Given** a user creating a workout template, **When** they submit without a name, **Then** the system displays a validation error indicating the name is required
4. **Given** a user with an existing workout template, **When** they choose to edit it, **Then** they can modify the name, notes, and location
5. **Given** a user editing a workout template, **When** they save changes, **Then** the modified date is updated to the current date and time
6. **Given** a user editing a workout template, **When** they clear the name field and try to save, **Then** the system displays a validation error

---

### User Story 3 - Manage Exercise Templates in Workout Templates (Priority: P1)

While editing a workout template, the user can add exercise templates to it. When adding an exercise, the user can select from their existing exercise library or create a new exercise template on the spot. The user can also edit exercise templates that are already associated with the workout template. Exercises within a workout template maintain their order.

**Why this priority**: Associating exercises with workouts is the core relationship that makes workout templates meaningful. Without this, templates are just empty shells.

**Independent Test**: Can be fully tested by opening a workout template in edit mode, adding an exercise from the library, creating a new exercise inline, and verifying both appear in the workout template.

**Acceptance Scenarios**:

1. **Given** a user editing a workout template, **When** they choose to add an exercise, **Then** they see a list of exercises from their exercise library to select from, with search and filter capabilities
2. **Given** a user adding an exercise to a workout template, **When** they select an existing exercise from the library, **Then** it is added to the workout template's exercise list
3. **Given** a user adding an exercise to a workout template, **When** the desired exercise does not exist, **Then** they can create a new exercise template directly from that context
4. **Given** a user editing a workout template, **When** they view the associated exercises, **Then** the exercises are displayed in the order they were added
5. **Given** a user editing a workout template with exercises, **When** they choose to edit an associated exercise, **Then** they can modify the exercise template's details
6. **Given** a user editing a workout template, **When** they add multiple exercises, **Then** all exercises are saved and associated with the workout template
7. **Given** a user editing a workout template with exercises, **When** they remove an exercise, **Then** the exercise is removed from the workout template but remains in their exercise library

---

### User Story 4 - Create and Edit Exercise Templates (Priority: P2)

The user can create exercise templates that become part of their exercise library. Each exercise template requires a name, an implement type (Barbell, Dumbbell, Selectorized Machine, Plate Loaded Machine, Bodyweight, Band, Kettlebell, Plate, or Medicine Ball), and a type (Reps, Duration, or Distance). Optionally, the user can attach an image (picture of the machine) and a model description. Exercise templates can be edited after creation.

**Why this priority**: While users can create exercises inline during workout template editing (Story 3), standalone exercise management provides direct library management and is essential for a complete experience.

**Independent Test**: Can be fully tested by navigating to the exercise library, creating a new exercise with all required fields, verifying it appears in the library, and editing its details.

**Acceptance Scenarios**:

1. **Given** a user on the landing page, **When** they choose to create a new exercise template, **Then** they are prompted to enter a name (required), implement type (required), and exercise type (required)
2. **Given** a user creating an exercise template, **When** they select an implement type, **Then** only the valid options are available: Barbell, Dumbbell, Selectorized Machine, Plate Loaded Machine, Bodyweight, Band, Kettlebell, Plate, Medicine Ball
3. **Given** a user creating an exercise template, **When** they select an exercise type, **Then** only the valid options are available: Reps, Duration, Distance
4. **Given** a user creating an exercise template, **When** they optionally add an image, **Then** the image is stored and displayed with the exercise
5. **Given** a user creating an exercise template, **When** they optionally add a model description, **Then** the model is saved with the exercise
6. **Given** a user creating an exercise template, **When** they submit without a name, implement, or type, **Then** the system displays validation errors for each missing required field
7. **Given** a user with an existing exercise template, **When** they choose to edit it, **Then** they can modify any of its fields (name, implement, type, image, model)

---

### User Story 5 - Reorder Exercises Within a Workout Template (Priority: P3)

While editing a workout template, the user can reorder the exercise templates to reflect their preferred workout sequence. The order is preserved when the workout template is saved and displayed.

**Why this priority**: Ordering exercises is important for usability but is an enhancement over the basic add/edit functionality. Users can still use templates without custom ordering.

**Independent Test**: Can be fully tested by opening a workout template with multiple exercises, reordering them, saving, and verifying the new order persists.

**Acceptance Scenarios**:

1. **Given** a user editing a workout template with multiple exercises, **When** they reorder the exercises, **Then** the new order is reflected immediately in the list
2. **Given** a user who has reordered exercises, **When** they save the workout template, **Then** the order is preserved for future views

---

### Edge Cases

- What happens when a user tries to create a workout template with a name that already exists? The system allows duplicate names but displays both, differentiated by their created date.
- What happens when a user deletes an exercise template that is used in multiple workout templates? The exercise is soft-deleted — it is hidden from the exercise library and cannot be added to new workout templates, but it remains visible in any existing workout templates that reference it. This preserves workout integrity while allowing library cleanup.
- What happens when a user uploads an image that exceeds size limits? The system rejects the upload and displays a message indicating the maximum allowed file size (5 MB).
- What happens when a user has a very large number of workout templates or exercises? The lists support scrolling with smooth performance for up to 500 items.
- What happens when a user tries to add the same exercise to a workout template more than once? The system allows it, as users may want to perform the same exercise at different points in a workout.
- What happens when a user deletes a workout template? The system shows a confirmation prompt. Upon confirmation, the workout template and its exercise associations are permanently removed. The exercise templates themselves remain in the user's library.
- What happens when a user tries to create an exercise template with a name that already exists in their library? The system displays a validation error indicating the name is already in use.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the user's workout templates on the landing page after login, listed by name
- **FR-002**: System MUST display the user's exercise library on the landing page after login
- **FR-003**: System MUST allow users to create a new workout template with a name (required), notes (optional), and location (optional)
- **FR-004**: System MUST automatically record the created date when a workout template is first saved
- **FR-005**: System MUST automatically update the modified date each time a workout template is edited and saved
- **FR-006**: System MUST allow users to edit an existing workout template's name, notes, and location
- **FR-007**: System MUST validate that workout template name is not empty before saving
- **FR-008**: System MUST allow users to add exercise templates to a workout template while editing it
- **FR-009**: System MUST present the user's exercise library for selection when adding an exercise to a workout template
- **FR-010**: System MUST allow users to create a new exercise template inline when adding exercises to a workout template
- **FR-011**: System MUST allow users to edit exercise templates associated with a workout template
- **FR-012**: System MUST preserve the order of exercises within a workout template
- **FR-013**: System MUST allow users to create a new exercise template with name (required), implement type (required), and exercise type (required)
- **FR-014**: System MUST restrict implement type to: Barbell, Dumbbell, Selectorized Machine, Plate Loaded Machine, Bodyweight, Band, Kettlebell, Plate, Medicine Ball
- **FR-015**: System MUST restrict exercise type to: Reps, Duration, Distance
- **FR-016**: System MUST allow users to optionally attach an image to an exercise template
- **FR-017**: System MUST allow users to optionally add a model description to an exercise template
- **FR-018**: System MUST allow users to edit existing exercise templates in their library
- **FR-019**: System MUST allow users to reorder exercises within a workout template
- **FR-020**: System MUST ensure users can only view and manage their own workout templates and exercise library
- **FR-021**: System MUST allow the same exercise to be added multiple times to a single workout template
- **FR-022**: System MUST reject image uploads exceeding 5 MB and display an appropriate error message
- **FR-023**: System MUST display appropriate empty states when a user has no workout templates or exercises
- **FR-024**: System MUST soft-delete exercise templates — hiding them from the exercise library and preventing them from being added to new workout templates, while preserving them in any existing workout templates that reference them
- **FR-025**: System MUST allow users to delete a workout template after confirming via a confirmation prompt
- **FR-026**: System MUST permanently remove a deleted workout template and its exercise associations (the exercise templates themselves are not affected)
- **FR-027**: System MUST allow users to remove individual exercises from a workout template while editing, without requiring confirmation
- **FR-028**: Removing an exercise from a workout template MUST NOT affect the exercise template in the user's library
- **FR-029**: System MUST enforce unique exercise template names per user — preventing creation of an exercise with the same name as an existing (non-deleted) exercise in the user's library
- **FR-030**: System MUST allow users to search exercises by name in both the exercise library on the landing page and the exercise picker when adding exercises to a workout template
- **FR-031**: System MUST allow users to filter exercises by implement type and exercise type in both the exercise library and the exercise picker

### Key Entities

- **Workout Template**: Represents a reusable workout plan. Has a name (required), notes (optional), location (optional), created date, and modified date. Contains an ordered list of exercise template references. Belongs to a single user.
- **Exercise Template**: Represents a specific exercise in the user's library. Has a name (required, unique per user), implement type (required, one of nine predefined options), exercise type (required, one of three predefined options), image (optional), and model (optional). Belongs to a single user. Can be referenced by multiple workout templates.
- **Workout Template Exercise**: Represents the association between a workout template and an exercise template, including the position/order of the exercise within the workout. Allows the same exercise template to appear multiple times in a single workout template.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new workout template in under 30 seconds
- **SC-002**: Users can add an existing exercise to a workout template in under 10 seconds
- **SC-003**: Users can create a new exercise template in under 45 seconds
- **SC-004**: Landing page loads and displays both workout templates and exercise library within 2 seconds of login
- **SC-005**: 90% of users can create their first workout template with exercises on first attempt without assistance
- **SC-006**: The landing page remains responsive with up to 500 workout templates and 500 exercises in a user's library

## Clarifications

### Session 2026-02-14

- Q: Can users delete workout templates? → A: Yes, hard delete with confirmation prompt
- Q: Can users remove an exercise from a workout template without deleting it from their library? → A: Yes, remove directly while editing (no confirmation needed)
- Q: Should exercise template names be unique per user? → A: Yes, enforce unique exercise names per user (prevent duplicates)
- Q: Should the exercise library support searching or filtering? → A: Search by name + filter by implement type and exercise type

## Assumptions

- Users are authenticated before accessing any workout or exercise template features (login is handled by the existing auth system from feature 001)
- Duplicate workout template names are allowed; users differentiate by context and recency
- Exercise templates are shared references; adding an exercise to a workout template creates a reference, not a copy
- Image uploads are limited to 5 MB and common image formats (JPEG, PNG, WebP)
- The landing page is the default screen shown after login
- "Model" on an exercise template refers to a free-text field for describing the specific equipment model (e.g., "Life Fitness Signature Series Leg Press")
- Location on a workout template is a free-text field (e.g., "Home Gym", "Planet Fitness Downtown")
- Exercise order within a workout template starts at position 1 and increments sequentially
