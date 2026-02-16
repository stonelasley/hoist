# Feature Specification: Auth Route Guard

**Feature Branch**: `006-auth-route-guard`
**Created**: 2026-02-15
**Status**: Draft
**Input**: User description: "Auth-based route protection and redirect for the Expo/React Native mobile app."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Unauthenticated User Redirected to Login (Priority: P1)

An unauthenticated user opens the app or navigates to any protected screen (e.g., workout list, exercise templates, settings). Instead of seeing an empty or broken screen, they are automatically redirected to the login page so they can sign in before accessing protected content.

**Why this priority**: Without this guard, unauthenticated users see empty screens with failed API calls, creating a confusing and broken experience. This is the core problem being solved.

**Independent Test**: Can be tested by clearing auth tokens and navigating to any protected route — the user should always land on the login screen.

**Acceptance Scenarios**:

1. **Given** the user has no stored auth tokens, **When** the app launches, **Then** the user sees the login screen.
2. **Given** the user has no stored auth tokens, **When** the user attempts to navigate to a protected route (e.g., workout templates), **Then** the user is redirected to the login screen.
3. **Given** the user has expired or invalid tokens, **When** the app launches, **Then** the user sees the login screen.

---

### User Story 2 - Authenticated User Bypasses Login (Priority: P1)

An authenticated user who already has valid tokens stored opens the app. Instead of seeing the login screen, they are automatically directed to the main landing page so they can immediately start using the app.

**Why this priority**: Equally critical to Story 1 — authenticated users should not be forced through the login flow on every app launch. This directly impacts daily usability.

**Independent Test**: Can be tested by logging in, closing the app, and reopening — the user should land directly on the landing page.

**Acceptance Scenarios**:

1. **Given** the user has valid stored auth tokens, **When** the app launches, **Then** the user sees the main landing page (not the login screen).
2. **Given** the user has valid stored auth tokens, **When** the user navigates to a login or registration URL, **Then** the user is redirected to the main landing page.

---

### User Story 3 - Loading State While Auth Initializes (Priority: P2)

When the app first opens, it takes a moment to load stored authentication tokens. During this brief period, the user sees a loading indicator rather than a flash of the wrong screen (e.g., briefly seeing login before being redirected to the app, or vice versa).

**Why this priority**: Prevents a visually jarring experience during app startup. Lower priority than the core redirect logic but important for polish.

**Independent Test**: Can be tested by observing app startup — a loading indicator should appear briefly before the correct screen is shown, with no flash of an incorrect screen.

**Acceptance Scenarios**:

1. **Given** the app is launching, **When** auth state is still being determined, **Then** the user sees a loading indicator.
2. **Given** the app is launching, **When** auth state finishes loading, **Then** the user is navigated to the appropriate screen without any visible flash of another screen.

---

### User Story 4 - Cleanup of Unused Default Screens (Priority: P3)

The app contains leftover boilerplate screens from the Expo template (tabs layout with "Home" and "Explore" tabs) that are not part of the actual app. These should be removed to prevent confusion and keep the codebase clean.

**Why this priority**: Housekeeping that reduces confusion for developers but does not affect end-user functionality.

**Independent Test**: Can be tested by verifying that no routes resolve to the old tab-based screens and that navigating to the root URL properly routes through the auth guard.

**Acceptance Scenarios**:

1. **Given** the app is running, **When** a user or developer navigates to the root URL, **Then** the auth guard handles routing (no default tab layout is shown).

---

### Edge Cases

- What happens when tokens exist in storage but are expired or revoked? The app treats the user as authenticated initially (tokens present), and handles 401 responses from the API gracefully at the screen level.
- What happens on web where tokens are stored in memory? After a page refresh, tokens are lost, so the user is redirected to login.
- What happens if the auth loading state takes an unusually long time? The loading indicator remains visible until auth state is resolved.
- What happens when a user logs out? After clearing tokens, the user is redirected to the login screen.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST check authentication state on launch before displaying any application screen.
- **FR-002**: The app MUST redirect unauthenticated users to the login screen when they attempt to access any protected route.
- **FR-003**: The app MUST redirect authenticated users to the main landing page when they attempt to access auth screens (login, register, etc.).
- **FR-004**: The app MUST display a loading indicator while authentication state is being determined on startup.
- **FR-005**: The app MUST NOT flash an incorrect screen (e.g., briefly showing login before redirecting to app, or vice versa) during the auth check.
- **FR-006**: The app MUST redirect users to the login screen after they log out and their tokens are cleared.
- **FR-007**: Unused boilerplate screens (tabs layout) MUST be removed from the app.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Unauthenticated users are redirected to the login screen within 1 second of app launch, with no protected content visible at any point.
- **SC-002**: Authenticated users see the main landing page within 1 second of app launch, without passing through the login screen.
- **SC-003**: No screen "flash" occurs during app startup — the user sees only a loading indicator followed by the correct destination screen.
- **SC-004**: All existing login, registration, and app navigation flows continue to work correctly after the auth guard is added.

## Assumptions

- The existing `useAuth` hook and `AuthProvider` correctly track authentication state (`isAuthenticated`, `isLoading`, tokens).
- Token validation happens at the API level (401 responses) rather than client-side token inspection. The auth guard only checks for token presence, not validity.
- On web, tokens are stored in memory and are lost on page refresh — this is expected behavior and the guard should redirect to login in this case.
- The `(auth)` route group contains all public/unauthenticated screens (login, register, forgot-password, etc.).
- The `(app)` route group contains all protected/authenticated screens.
