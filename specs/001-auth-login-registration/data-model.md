# Data Model: Authentication, Login & Registration

**Feature Branch**: `001-auth-login-registration`
**Date**: 2026-02-07

## Entities

### ApplicationUser (extends IdentityUser)

Existing entity at `src/Infrastructure/Identity/ApplicationUser.cs`. Extended with profile fields.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | string | Yes | Inherited from IdentityUser (GUID) |
| UserName | string | Yes | Inherited — set to email address |
| Email | string | Yes | Inherited — unique, used for login |
| EmailConfirmed | bool | Yes | Inherited — gates login access |
| PasswordHash | string | No | Inherited — null for OAuth-only users |
| SecurityStamp | string | Yes | Inherited — invalidates tokens on change |
| FirstName | string | Yes | **New** — max 100 chars |
| LastName | string | Yes | **New** — max 100 chars |
| Age | int? | No | **New** — nullable, minimum 13 if provided |

**Identity Rules**:
- Email is the unique identifier for login
- UserName is set equal to Email
- EmailConfirmed MUST be `true` before login is permitted
- SecurityStamp changes invalidate all existing tokens

### External Logins (ASP.NET Core Identity built-in)

Managed by Identity via `AspNetUserLogins` table. No custom entity needed.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| LoginProvider | string | Yes | e.g., "Google" |
| ProviderKey | string | Yes | Google's user ID |
| ProviderDisplayName | string | No | e.g., "Google" |
| UserId | string | Yes | FK to ApplicationUser |

### Email Verification Tokens (Identity built-in)

Managed by `UserManager.GenerateEmailConfirmationTokenAsync()`. Stored internally by Identity in `AspNetUserTokens`. No custom entity needed.

**Behavior**:
- Generated on registration
- Single-use (consumed on verification)
- Expiration: 48 hours (configured via `DataProtectionTokenProviderOptions`)

### Password Reset Tokens (Identity built-in)

Managed by `UserManager.GeneratePasswordResetTokenAsync()`. Stored internally by Identity.

**Behavior**:
- Generated on forgot password request
- Single-use (consumed on reset)
- Expiration: 24 hours (configured via `DataProtectionTokenProviderOptions`)
- New request invalidates previous tokens (via SecurityStamp rotation)

## State Transitions

### Account Lifecycle

```
[Unregistered] → Register → [Unverified]
[Unverified] → Verify Email → [Active]
[Unverified] → Resend Verification → [Unverified] (new token sent)
[Active] → Login → [Authenticated Session]
[Active] → Forgot Password → [Password Reset Pending]
[Password Reset Pending] → Reset Password → [Active]
```

### Google OAuth Account Creation

```
[Unregistered] → Google OAuth → [Active] (email pre-verified)
[Active with Password] → Google OAuth (same email) → [Active with Password + Google Linked]
```

## Relationships

```
ApplicationUser (1) ──── (*) AspNetUserLogins (External Logins)
ApplicationUser (1) ──── (*) AspNetUserTokens (Verification/Reset Tokens)
ApplicationUser (1) ──── (*) AspNetUserRoles (Roles)
```

## Validation Rules

- **FirstName**: Required, 1–100 characters, no leading/trailing whitespace
- **LastName**: Required, 1–100 characters, no leading/trailing whitespace
- **Email**: Required, valid email format, unique across all users
- **Password**: Minimum 8 chars, at least one uppercase, one lowercase, one digit, one special character
- **Age**: Optional. If provided, must be >= 13 (integer)
