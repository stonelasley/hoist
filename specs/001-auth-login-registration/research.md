# Research: Authentication, Login & Registration

**Feature Branch**: `001-auth-login-registration`
**Date**: 2026-02-07

## Decision 1: Authentication Flow — ASP.NET Core Identity API Endpoints

**Decision**: Use the existing `MapIdentityApi<ApplicationUser>()` endpoints already configured in `src/Web/Endpoints/Users.cs`. These provide `/api/users/register`, `/api/users/login`, `/api/users/refresh`, and related endpoints out of the box with bearer token authentication.

**Rationale**: The backend already has Identity fully configured with bearer token auth. The mobile client will call these endpoints and store the returned bearer token. This avoids reinventing auth and stays aligned with the existing infrastructure.

**Alternatives considered**:
- Custom JWT implementation — Rejected: more code, same result, violates YAGNI (Principle VII).
- Cookie-based auth — Rejected: not suitable for native mobile apps.

## Decision 2: Google OAuth Integration — Backend Exchange Pattern

**Decision**: Use the backend token exchange pattern. The mobile app uses `expo-auth-session` to obtain a Google OAuth token, then sends it to a custom backend endpoint (`POST /api/users/google-login`) that validates the Google token, finds or creates the user, and returns a bearer token.

**Rationale**: This keeps all user creation and account linking logic on the backend (Principle I: Clean Architecture, Principle VIII: mobile client MUST NOT contain business logic). The mobile app only handles the OAuth UI redirect.

**Alternatives considered**:
- Client-only Google Sign-In with Firebase — Rejected: adds Firebase dependency, splits auth state between Firebase and Identity.
- Direct Google token validation in mobile app — Rejected: violates Principle VIII (no business logic in mobile).

## Decision 3: Email Sending — SendGrid via Backend

**Decision**: Integrate SendGrid (specified by stakeholder) as a transactional email service in the Infrastructure layer. Create an `IEmailSender` interface in Application, implement `SendGridEmailSender` in Infrastructure.

**Rationale**: SendGrid/Twilio was explicitly required. Placing it behind an interface follows Clean Architecture (Principle I). The existing Identity framework supports custom `IEmailSender<ApplicationUser>` which integrates cleanly.

**Alternatives considered**:
- SMTP relay — Rejected: stakeholder specified SendGrid/Twilio.
- Azure Communication Services — Rejected: stakeholder specified SendGrid/Twilio.

## Decision 4: Token Storage on Mobile — expo-secure-store

**Decision**: Use `expo-secure-store` to store bearer tokens on the mobile client. This uses Keychain on iOS and EncryptedSharedPreferences on Android.

**Rationale**: Tokens are sensitive credentials. `expo-secure-store` is the Expo-recommended secure storage solution and works across all platforms. For web, fall back to httpOnly cookies or in-memory storage.

**Alternatives considered**:
- AsyncStorage — Rejected: not encrypted, tokens would be accessible to other apps on rooted devices.
- React Native Keychain — Rejected: not part of Expo managed workflow.

## Decision 5: Deep Linking — Expo Linking with hoist:// Scheme

**Decision**: Use Expo's built-in deep linking with the `hoist://` URL scheme (already configured in `app.json`) plus universal links for web fallback. Email verification and password reset links will be universal links that open the app or fall back to a web confirmation page hosted by the Web project.

**Rationale**: The `hoist://` scheme is already defined in `app.json`. Expo Router supports deep link routing natively. The backend Web project can serve the fallback pages for users without the app installed.

**Alternatives considered**:
- App-only deep links (no web fallback) — Rejected: users on desktop or without the app installed would be stuck.
- Third-party deep link service (Branch, Firebase Dynamic Links) — Rejected: unnecessary dependency for this use case.

## Decision 6: HTTP Client for Mobile — Fetch with Custom Hook

**Decision**: Use the built-in `fetch` API wrapped in a custom `useApi` hook that handles bearer token attachment, refresh, and error handling. No additional HTTP library needed.

**Rationale**: Fetch is built into React Native. A lightweight wrapper hook keeps the codebase simple (Principle VII: YAGNI) while centralizing auth header injection and token refresh logic.

**Alternatives considered**:
- Axios — Rejected: adds dependency for minimal benefit over fetch with a custom hook.
- React Query/TanStack Query — Deferred: valuable for caching but can be added later without affecting the auth layer.

## Decision 7: Extending ApplicationUser

**Decision**: Extend the existing `ApplicationUser` class (currently empty, extends `IdentityUser`) with `FirstName`, `LastName`, and `Age` (nullable int) properties. Update the registration command/endpoint to accept and store these fields.

**Rationale**: The spec requires first name, last name, and optional age. ASP.NET Core Identity's `IdentityUser` base class provides email, password hash, and security stamp. Custom fields go on `ApplicationUser` per the established pattern.

**Alternatives considered**:
- Separate UserProfile entity — Rejected: over-engineering for three fields (Principle VII).

## Decision 8: Email Verification — Custom Endpoints Using Identity Tokens

**Decision**: Use ASP.NET Core Identity's built-in email confirmation token generation (`UserManager.GenerateEmailConfirmationTokenAsync`) and validation (`UserManager.ConfirmEmailAsync`). Create custom endpoints that generate/validate these tokens and send emails via SendGrid.

**Rationale**: Identity already has secure token generation and validation. Adding custom endpoints on top lets us control the email content and deep link format while leveraging proven security infrastructure.

**Alternatives considered**:
- Custom token table — Rejected: duplicates Identity's built-in token infrastructure.
- Magic link login — Rejected: out of scope, stakeholder specified email/password + verification flow.
