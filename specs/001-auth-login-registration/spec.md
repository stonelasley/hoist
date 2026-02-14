# Feature Specification: Authentication, Login & Registration

**Feature Branch**: `001-auth-login-registration`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Create a login screen that is modern and clean. The login screen should support logging in with email/password, and google open auth. The login screen should give the option to register. Registration can be done with username/password or google oauth. Registration should require firstname, lastname, age (optional). The login screen should also have a forgot password functionality. Upon registration an email will be sent for account validation. A new account must validate email before they are able to login. Upon logging in they are taken to a landing page with the words 'Landing Page'. Emails are sent via SendGrid/Twilio. The login page should support password reset which dispatches a password reset link in an email."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Email/Password Login (Priority: P1)

A returning user visits the application and sees a modern, clean login screen. They enter their email address and password, then submit the form. The system verifies their credentials and that their email has been validated. Upon successful authentication, they are redirected to a landing page displaying the text "Landing Page".

**Why this priority**: Login is the gateway to the entire application. Without it, no other feature is accessible. This is the minimum viable slice that proves the authentication flow works end-to-end.

**Independent Test**: Can be fully tested by creating a verified user account directly, then logging in with email/password and confirming the landing page appears.

**Acceptance Scenarios**:

1. **Given** a user with a verified email account, **When** they enter valid email and password and submit, **Then** they are authenticated and redirected to the landing page showing "Landing Page".
2. **Given** a user with a verified email account, **When** they enter an incorrect password, **Then** they see an error message "Invalid email or password" and remain on the login screen.
3. **Given** a user whose email is not yet verified, **When** they enter valid credentials, **Then** they see a message indicating they must verify their email before logging in, along with an option to resend the verification email.
4. **Given** a user whose email is not yet verified, **When** they click "Resend verification email" after a blocked login attempt, **Then** a new verification email is sent and they see a confirmation message.
5. **Given** a visitor, **When** they view the login screen, **Then** they see email and password fields, a login button, a "Sign in with Google" option, a "Register" link, and a "Forgot password?" link.

---

### User Story 2 - Email/Password Registration with Email Verification (Priority: P2)

A new user visits the login screen and clicks the registration link. They are presented with a registration form requesting first name, last name, email, password, and optionally age. Upon submitting valid information, the system creates their account and sends a verification email. The user clicks the verification link in the email to activate their account, after which they can log in.

**Why this priority**: Registration is the second most critical flow — new users cannot use the app without creating an account. Email verification ensures account security and valid contact information.

**Independent Test**: Can be fully tested by filling out the registration form, checking that a verification email is dispatched, clicking the verification link, then logging in with the new credentials.

**Acceptance Scenarios**:

1. **Given** a visitor on the registration screen, **When** they fill in first name, last name, email, password, and submit, **Then** an account is created and a verification email is sent to the provided email address.
2. **Given** a visitor on the registration screen, **When** they fill in all required fields plus the optional age field and submit, **Then** the account is created with age stored.
3. **Given** a user who has just registered, **When** they click the verification link in the email, **Then** their account is marked as verified. If the app is installed, the link opens the app directly to a verification confirmation screen. If the app is not installed, the link opens a web confirmation page with a prompt to return to the app.
4. **Given** a visitor, **When** they attempt to register with an email address already in use, **Then** they see an error message indicating the email is already registered.
5. **Given** a visitor, **When** they submit the registration form with a missing required field (first name, last name, email, or password), **Then** they see validation errors for each missing field.
6. **Given** a visitor, **When** they submit a password that does not meet strength requirements, **Then** they see a message describing the password requirements.

---

### User Story 3 - Google OAuth Login & Registration (Priority: P3)

A user visits the login screen and clicks "Sign in with Google". They are redirected to Google's authentication flow. If they already have an account linked to that Google identity, they are logged in and redirected to the landing page. If they do not have an account, a new account is created using their Google profile information (first name, last name, email), the email is considered pre-verified (since Google has already verified it), and they are redirected to the landing page.

**Why this priority**: Google OAuth provides a frictionless alternative to email/password registration. It reduces signup abandonment and eliminates password fatigue. However, the core email/password flow must work first.

**Independent Test**: Can be fully tested by clicking the Google sign-in button, completing the Google auth flow, and confirming the landing page appears. For new users, confirm the account was created with Google profile data.

**Acceptance Scenarios**:

1. **Given** a visitor with no existing account, **When** they click "Sign in with Google" and authorize the application, **Then** a new account is created with their Google profile information (first name, last name, email), the email is marked as verified, and they are redirected to the landing page.
2. **Given** a user with an existing account linked to their Google identity, **When** they click "Sign in with Google" and authorize, **Then** they are logged in and redirected to the landing page.
3. **Given** a user with an existing email/password account using the same email as their Google account, **When** they click "Sign in with Google", **Then** the Google identity is linked to their existing account and they are logged in.
4. **Given** a visitor, **When** they click "Sign in with Google" but cancel or deny authorization, **Then** they are returned to the login screen with no error.

---

### User Story 4 - Forgot Password / Password Reset (Priority: P4)

A user who has forgotten their password clicks "Forgot password?" on the login screen. They enter their email address and submit the form. The system sends a password reset email containing a secure, time-limited link. The user clicks the link, enters a new password, and submits. Their password is updated and they can log in with the new password.

**Why this priority**: Password reset is essential for user retention — users who cannot recover access will abandon the application. It depends on the email sending infrastructure from P2 and the login flow from P1.

**Independent Test**: Can be fully tested by requesting a password reset for a verified account, checking that the reset email is dispatched, clicking the reset link, entering a new password, then logging in with the new password.

**Acceptance Scenarios**:

1. **Given** a user on the login screen, **When** they click "Forgot password?", **Then** they are shown a form requesting their email address.
2. **Given** a user on the forgot password form, **When** they enter a registered email address and submit, **Then** a password reset email is sent containing a secure reset link, and they see a confirmation message.
3. **Given** a user on the forgot password form, **When** they enter an email address that is not registered, **Then** they see the same confirmation message (to prevent email enumeration).
4. **Given** a user who received a reset email, **When** they click the reset link within the valid time window, **Then** the link opens the app (if installed) or a web page (if not) showing a form to enter a new password.
5. **Given** a user who received a reset email, **When** they click the reset link after it has expired, **Then** they see a message that the link has expired and are prompted to request a new one.
6. **Given** a user on the password reset form, **When** they enter a valid new password and submit, **Then** their password is updated and they are redirected to the login screen with a success message.

---

### Edge Cases

- What happens when a user clicks a verification link that has already been used? They see a message that their account is already verified with a link to log in.
- What happens when a user requests multiple password reset emails? Only the most recent reset link is valid; previous links are invalidated.
- What happens when a Google OAuth user tries to use "Forgot password"? They see a message suggesting they sign in with Google instead, since no password is set for their account.
- What happens when the email delivery service is temporarily unavailable? The user sees a message to try again later, and the system queues the email for retry.
- What happens when a user enters an age below 13? Registration is rejected with a message that users must be at least 13 years old (COPPA compliance).
- What happens when the verification link URL is malformed or tampered with? The system rejects it and shows an invalid link message.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a login screen with fields for email and password, a submit button, a "Sign in with Google" button, a "Register" link, and a "Forgot password?" link.
- **FR-002**: System MUST authenticate users via email/password, verifying credentials against stored account data.
- **FR-003**: System MUST authenticate users via Google OAuth, creating a new account from Google profile data if none exists.
- **FR-004**: System MUST block login for accounts whose email address has not been verified, displaying a clear message to the user.
- **FR-005**: System MUST redirect authenticated users to a landing page displaying the text "Landing Page".
- **FR-006**: System MUST provide a registration form requiring first name, last name, email, and password, with age as an optional field.
- **FR-007**: System MUST send a verification email upon registration using a transactional email service (SendGrid/Twilio).
- **FR-008**: System MUST activate user accounts only after the user clicks the email verification link.
- **FR-009**: System MUST provide a "Forgot password" flow that sends a password reset email containing a secure, time-limited link.
- **FR-010**: System MUST allow users to set a new password via the reset link and confirm the change.
- **FR-011**: System MUST enforce password strength requirements (minimum 8 characters, at least one uppercase letter, one lowercase letter, one digit, and one special character).
- **FR-012**: System MUST prevent email enumeration — forgot password and registration error messages MUST NOT reveal whether a specific email is registered.
- **FR-013**: System MUST link Google OAuth accounts to existing accounts when the email addresses match.
- **FR-014**: System MUST enforce a minimum age of 13 years for registration when age is provided.
- **FR-015**: System MUST invalidate previous password reset links when a new reset is requested.
- **FR-016**: Password reset links MUST expire after 24 hours.
- **FR-017**: System MUST allow unverified users to request a new verification email from the login screen when login is blocked due to unverified status.

### Key Entities

- **User Account**: Represents a registered user. Key attributes: first name, last name, email (unique), age (optional), email verified status, account creation date.
- **External Login**: Represents a linked third-party identity (e.g., Google). Associated with a User Account. Key attributes: provider name, provider user identifier.
- **Email Verification Token**: A single-use, time-limited token sent to the user's email upon registration to confirm email ownership.
- **Password Reset Token**: A single-use, time-limited token sent to the user's email to authorize setting a new password. Expires after 24 hours.

### Assumptions

- The login screen is built in the existing Expo/React Native application (`src/App`) using expo-router, targeting iOS, Android, and web.
- "Modern and clean" design means minimal visual clutter, ample whitespace, clear typography, and responsive layout across all target platforms (iOS, Android, web).
- Password strength rules follow industry standard: minimum 8 characters with uppercase, lowercase, digit, and special character.
- Email verification links expire after 48 hours.
- Password reset links expire after 24 hours.
- The landing page is a simple placeholder page — its design and content beyond "Landing Page" are out of scope.
- Google is the only third-party OAuth provider for this feature (no Apple, Facebook, etc.).
- Rate limiting on login attempts and password reset requests follows standard security practices (e.g., 5 failed login attempts triggers a temporary lockout).
- Email verification and password reset links use universal/deep links: they open the app directly if installed (via the `hoist://` URL scheme), or fall back to a web confirmation page if not.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete email/password registration in under 2 minutes (excluding email verification wait time).
- **SC-002**: Users can log in with valid credentials in under 10 seconds from page load to landing page.
- **SC-003**: 95% of verification and password reset emails are delivered within 60 seconds of being requested.
- **SC-004**: Google OAuth login/registration completes in under 15 seconds (including Google's auth flow).
- **SC-005**: 90% of new users successfully complete the registration and email verification flow on their first attempt.
- **SC-006**: Password reset flow (from clicking "Forgot password" to logging in with new password) can be completed in under 3 minutes (excluding email delivery wait time).
- **SC-007**: The login screen renders correctly and is fully functional on iOS, Android, and web platforms.

## Clarifications

### Session 2026-02-07

- Q: What is the frontend platform for login/registration screens? → A: Existing Expo/React Native app in `src/App` targeting iOS, Android, and web.
- Q: Can unverified users resend the verification email? → A: Yes, from the login screen after a blocked login attempt due to unverified status.
- Q: Where do email verification and password reset links take the user? → A: Universal/deep links — open the app if installed, fall back to web confirmation page if not.
