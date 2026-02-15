# Feature Specification: Workout Tracking

**Feature Branch**: `004-workout-tracking`
**Created**: 2026-02-15
**Status**: Draft
**Input**: User description: "Track workouts as executions of workout templates with sets, measurements based on exercise/implement type, user unit preferences, workout lifecycle (start/complete/rate/notes), recent workouts on landing page, and full workout history with sorting/filtering/search."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start and Record a Workout (Priority: P1)

As a user, I want to start a workout from one of my workout templates so that I can track what I do during a training session. When I tap "Start Workout" on a template, the system captures the current date/time as the start time and pre-populates the workout with the exercises from that template. I can then record sets for each exercise — entering weight and reps, bodyweight and reps, bodyweight and duration, band color and reps, band color and duration, or distance and optional duration — depending on the exercise's type and implement. The location from the template is pre-filled but I can change it or clear it.

**Why this priority**: This is the core value proposition — without the ability to record a workout, no other feature matters.

**Independent Test**: Can be fully tested by starting a workout from a template, recording sets for different exercise types, and verifying the data persists.

**Acceptance Scenarios**:

1. **Given** a user has a workout template with 3 exercises (barbell, bodyweight-reps, distance), **When** they tap "Start Workout", **Then** a new workout is created with the current date/time as start time, the template's location pre-filled, and all 3 exercises listed ready for set entry.
2. **Given** a user is recording a barbell exercise, **When** they add a set, **Then** they are prompted to enter weight (in their preferred unit) and rep count.
3. **Given** a user is recording a bodyweight exercise with reps, **When** they add a set, **Then** their current bodyweight is pre-filled (in their preferred unit) and they enter rep count.
4. **Given** a user is recording a bodyweight exercise with duration, **When** they add a set, **Then** their current bodyweight is pre-filled and they enter a duration time.
5. **Given** a user is recording a band exercise with reps, **When** they add a set, **Then** they select a band color and enter rep count.
6. **Given** a user is recording a band exercise with duration, **When** they add a set, **Then** they select a band color and enter a duration time.
7. **Given** a user is recording a distance exercise, **When** they add a set, **Then** they enter a distance (in their preferred unit) and optionally a time duration.
8. **Given** a workout was started from a template with a location, **When** the user views the workout, **Then** they can change the location to a different one, or clear it entirely.

---

### User Story 2 - Complete a Workout (Priority: P1)

As a user, when I finish my training session I want to complete the workout. Before finalizing, I can edit the start/end times (to correct mistakes), add notes about how the session went, and rate the workout on a subjective scale. When I confirm completion, the system captures the current date/time as the end time (which I can then adjust if needed).

**Why this priority**: Completing a workout with notes and rating is essential to the tracking lifecycle and gives the data long-term value.

**Independent Test**: Can be tested by completing an in-progress workout, verifying end time capture, notes, and rating are saved.

**Acceptance Scenarios**:

1. **Given** a user has an in-progress workout with recorded sets, **When** they tap "Complete Workout", **Then** the current date/time is captured as the end time and the user is presented with a completion screen.
2. **Given** a user is on the completion screen, **When** they edit the start time or end time, **Then** the workout duration updates accordingly.
3. **Given** a user is on the completion screen, **When** they add notes (free-text), **Then** the notes are saved with the workout.
4. **Given** a user is on the completion screen, **When** they select a rating (1-5 scale), **Then** the rating is saved with the workout.
5. **Given** a user confirms completion, **When** the workout is saved, **Then** it appears as a completed workout in their history with all sets, notes, rating, and duration.

---

### User Story 3 - Configure Unit Preferences (Priority: P2)

As a user, I want to set my preferred units for weight and distance so that all my workout data is recorded in units I'm familiar with. I can choose between weight units (lbs, kg) and distance units (miles, kilometers, meters, yards).

**Why this priority**: Unit preferences must be in place before workout data entry is meaningful, but the preference itself is a simple one-time setup.

**Independent Test**: Can be tested by setting unit preferences and verifying they appear correctly when recording sets.

**Acceptance Scenarios**:

1. **Given** a new user who has not set preferences, **When** they navigate to settings, **Then** they see default unit preferences (lbs for weight, miles for distance).
2. **Given** a user is in settings, **When** they change weight unit from lbs to kg, **Then** all subsequent workout set entries display and accept values in kg.
3. **Given** a user is in settings, **When** they change distance unit from miles to kilometers, **Then** all subsequent distance entries display and accept values in kilometers.
4. **Given** a user has existing workouts recorded in lbs, **When** they change their preference to kg, **Then** existing workout data is stored as-is (original values preserved) but new entries use the new unit.

---

### User Story 4 - View Recent Workouts on Landing Page (Priority: P2)

As a user, when I open the app I want to see my last 3 completed workouts on the landing page so I can quickly review recent activity and have a shortcut to start a similar workout again.

**Why this priority**: Quick access to recent workouts is a key daily-use feature that drives engagement and makes the app feel personal.

**Independent Test**: Can be tested by completing workouts and verifying the landing page shows the 3 most recent.

**Acceptance Scenarios**:

1. **Given** a user has completed 5 workouts, **When** they view the landing page, **Then** the 3 most recent completed workouts are displayed with template name, date, duration, and rating.
2. **Given** a user has completed fewer than 3 workouts, **When** they view the landing page, **Then** only the completed workouts are shown (no empty placeholders).
3. **Given** a user has no completed workouts, **When** they view the landing page, **Then** an appropriate empty state message is shown encouraging them to start their first workout.
4. **Given** a user has an in-progress workout, **When** they view the landing page, **Then** the in-progress workout is shown prominently (above or separate from the recent completed workouts) with an option to resume it.

---

### User Story 5 - View Full Workout History (Priority: P3)

As a user, I want to view my complete workout history with the ability to sort, filter, and search so I can review my progress over time.

**Why this priority**: Full history with filtering is valuable for long-term users but not required for the initial tracking experience.

**Independent Test**: Can be tested by navigating to the history screen and verifying sort, filter, and search functionality.

**Acceptance Scenarios**:

1. **Given** a user has completed many workouts, **When** they navigate to workout history, **Then** they see a scrollable list of all completed workouts sorted by date (most recent first) by default.
2. **Given** a user is viewing workout history, **When** they change the sort to "by rating", **Then** workouts are reordered by rating (highest first).
3. **Given** a user is viewing workout history, **When** they apply a location filter, **Then** only workouts performed at that location are shown.
4. **Given** a user is viewing workout history, **When** they apply a rating filter (e.g., 4+ stars), **Then** only workouts with that rating or higher are shown.
5. **Given** a user is viewing workout history, **When** they type a search term, **Then** workouts whose notes contain that term are displayed.
6. **Given** a user is on the landing page, **When** they tap the "View All" or "Workout History" navigation link, **Then** they are taken to the full workout history screen.

---

### User Story 6 - Set Current Bodyweight (Priority: P2)

As a user, I want to record my current bodyweight so that when I perform bodyweight exercises, my weight is automatically pre-filled in my workout sets.

**Why this priority**: Bodyweight exercises need a weight reference, and this is a simple but necessary input for accurate tracking.

**Independent Test**: Can be tested by setting bodyweight and verifying it appears in bodyweight exercise sets.

**Acceptance Scenarios**:

1. **Given** a user has not set their bodyweight, **When** they record a bodyweight exercise set, **Then** the weight field is empty and they must enter it manually.
2. **Given** a user has set their bodyweight to 180 lbs, **When** they record a bodyweight exercise set, **Then** 180 lbs is pre-filled in the weight field (using their preferred unit).
3. **Given** a user updates their bodyweight, **When** they start a new workout with bodyweight exercises, **Then** the new bodyweight is used for pre-filling.

---

### Edge Cases

- What happens when a user starts a workout but never completes it? The workout remains in "in-progress" state and is shown on the landing page with an option to resume or discard.
- What happens when a user tries to start a second workout while one is already in progress? The system alerts the user and offers to resume the existing workout or discard it before starting a new one.
- What happens when the app closes unexpectedly during a workout? Set data entered so far is persisted; the user can resume the workout when they reopen the app.
- What happens when a workout template is deleted after a workout has been recorded from it? The historical workout data is preserved; the template name is retained as a snapshot.
- What happens when an exercise template is soft-deleted after sets have been recorded? The historical set data is preserved with the exercise name retained.
- What happens when a user records a set with zero reps or zero weight? The system allows zero values (e.g., failed attempt or bodyweight-only) — these are valid tracking entries.
- What happens when a location used in a workout is soft-deleted? The workout retains the location name as a snapshot; the location filter in history still shows it as a past-used location.
- What happens when a user changes unit preferences mid-workout? The in-progress workout continues using the unit that was active when the workout started; the new preference applies to future workouts.

## Clarifications

### Session 2026-02-15

- Q: When are individual sets persisted during an active workout? → A: Each set is saved to the server immediately when the user confirms it.
- Q: Can users edit completed workouts after finalization? → A: Completed workouts are fully editable (sets, notes, rating, times).
- Q: How should the workout set data model handle the 6 different measurement combinations? → A: Single set entity with nullable fields (weight, reps, duration, distance, bandColor, bodyweight, unit); the exercise type/implement determines which fields are populated.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to start a workout from an existing workout template, creating a new workout instance with the current date/time as the start time.
- **FR-002**: System MUST pre-populate a new workout with exercises from the selected template in their defined order.
- **FR-003**: System MUST pre-fill the workout location from the template's location (if set), and allow the user to change or clear it.
- **FR-004**: System MUST allow users to add sets to each exercise in a workout, where each set captures measurements appropriate to the exercise's type and implement:
  - Weight + Reps: for barbell, dumbbell, selectorized machine, plate loaded machine, plate, medicine ball, and kettlebell implements with reps-type exercises
  - Bodyweight + Reps: for bodyweight implements with reps-type exercises
  - Bodyweight + Duration: for bodyweight implements with duration-type exercises
  - Band Color + Reps: for band implements with reps-type exercises
  - Band Color + Duration: for band implements with duration-type exercises
  - Distance + optional Duration: for any exercise with distance-type
- **FR-005**: System MUST display and accept weight values in the user's preferred weight unit (lbs or kg).
- **FR-006**: System MUST display and accept distance values in the user's preferred distance unit (miles, kilometers, meters, or yards).
- **FR-007**: System MUST capture the current date/time as the end time when a user completes a workout.
- **FR-008**: System MUST allow users to edit the start time and end time before finalizing a completed workout.
- **FR-009**: System MUST allow users to add free-text notes to a workout before completion.
- **FR-010**: System MUST allow users to rate a workout on a 1-5 scale before completion.
- **FR-011**: System MUST persist each set to the server immediately when the user confirms it during an active workout. Notes, rating, times, and location are persisted upon completion.
- **FR-012**: System MUST display the 3 most recent completed workouts on the landing page, showing template name, date, duration, and rating.
- **FR-013**: System MUST display any in-progress workout prominently on the landing page with a resume option.
- **FR-014**: System MUST provide a full workout history screen accessible from the landing page.
- **FR-015**: System MUST support sorting workout history by date (ascending/descending) and by rating (ascending/descending).
- **FR-016**: System MUST support filtering workout history by location and by minimum rating.
- **FR-017**: System MUST support searching workout history by notes text.
- **FR-018**: System MUST allow users to set and update weight unit preference (lbs or kg) in their account settings.
- **FR-019**: System MUST allow users to set and update distance unit preference (miles, kilometers, meters, or yards) in their account settings.
- **FR-020**: System MUST allow users to set and update their current bodyweight in their account settings.
- **FR-021**: System MUST pre-fill bodyweight in sets for bodyweight-implement exercises using the user's current recorded bodyweight.
- **FR-022**: System MUST prevent a user from having more than one workout in-progress at a time.
- **FR-023**: System MUST preserve historical workout data when a template, exercise, or location is subsequently deleted.
- **FR-024**: System MUST store set values with their unit at the time of recording (changing unit preference does not retroactively convert existing data).
- **FR-025**: System MUST allow users to discard an in-progress workout.
- **FR-027**: System MUST allow users to edit any field of a completed workout (sets, notes, rating, start time, end time) after finalization.
- **FR-026**: System MUST allow users to reorder, add, or remove exercises from a workout during an active session (the template is a starting point, not a constraint).

### Key Entities

- **Workout**: An instance/execution of a workout template. Has a start time, end time, location (optional), notes, rating (1-5), status (in-progress or completed), and references the source template. Belongs to a user.
- **WorkoutSet**: A single set performed within a workout for a specific exercise. Modeled as a single entity with nullable fields: weight, reps, duration (seconds), distance, band color, bodyweight, and unit. The exercise's type and implement determine which fields are populated. Tracks position/order within the exercise.
- **WorkoutExercise**: A join between a workout and an exercise, representing a specific exercise being performed in a workout. Maintains position/order and references the source exercise template. Allows exercises to be added/removed/reordered independently of the original template.
- **UserPreferences**: Stores per-user settings including preferred weight unit, preferred distance unit, and current bodyweight.

### Assumptions

- Default weight unit is lbs; default distance unit is miles (US-centric defaults matching expected initial user base).
- Rating scale is 1-5 (whole numbers only); rating is optional — users can complete a workout without rating it.
- Notes have a reasonable maximum length (2000 characters, consistent with existing template notes).
- Band color options reuse the existing Colour value object (White, Red, Orange, Yellow, Green, Blue, Purple, Grey).
- Duration is captured in seconds (displayed as mm:ss or hh:mm:ss as appropriate).
- A workout can only be started from an existing template (no ad-hoc workouts without a template in this iteration).
- Workout history pagination uses infinite scroll / cursor-based loading for performance.
- The "template name" displayed on historical workouts is a snapshot taken at workout creation time, not a live reference.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can start a workout from a template and record their first set within 10 seconds.
- **SC-002**: Users can complete a full workout (start, record sets, add notes, rate, and finish) in under 2 minutes of app interaction time (excluding actual exercise time).
- **SC-003**: The landing page loads and displays the 3 most recent workouts within 2 seconds.
- **SC-004**: Workout history search and filter results appear within 1 second of user input.
- **SC-005**: 100% of workout data is preserved when source templates, exercises, or locations are subsequently deleted.
- **SC-006**: All measurement inputs respect the user's configured unit preferences with no manual conversion needed.
- **SC-007**: Users can configure their unit preferences and bodyweight in a single settings interaction.
