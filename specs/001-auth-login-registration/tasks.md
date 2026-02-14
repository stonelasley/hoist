# Tasks: Authentication, Login & Registration

**Input**: Design documents from `/specs/001-auth-login-registration/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in the feature specification. Test tasks are omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Backend and mobile project setup — NuGet/npm packages, configuration, shared abstractions

- [x] T001 Add SendGrid NuGet package (`SendGrid`) to `Directory.Packages.props` and `src/Infrastructure/Infrastructure.csproj`
- [x] T002 Add Google API Auth NuGet package (`Google.Apis.Auth`) to `Directory.Packages.props` and `src/Infrastructure/Infrastructure.csproj`
- [x] T003 Install mobile dependencies: run `npm install expo-secure-store expo-auth-session expo-crypto expo-web-browser` in `src/App/`
- [x] T004 [P] Create API base URL constants in `src/App/constants/api.ts` with development and production URLs per quickstart.md environment config
- [x] T005 [P] Add SendGrid and Google configuration sections to `src/Web/appsettings.Development.json` per quickstart.md (SendGrid.ApiKey, SendGrid.FromEmail, SendGrid.FromName, Google.ClientId, App.BaseUrl, App.DeepLinkScheme)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 Extend `ApplicationUser` in `src/Infrastructure/Identity/ApplicationUser.cs` with `FirstName` (string, required, max 100), `LastName` (string, required, max 100), and `Age` (int?, nullable, min 13) per data-model.md
- [x] T007 Create EF Core entity configuration `src/Infrastructure/Data/Configurations/ApplicationUserConfiguration.cs` with column constraints for FirstName (MaxLength 100, Required), LastName (MaxLength 100, Required), and Age (nullable) per data-model.md
- [x] T008 Configure Identity token lifespans in `src/Infrastructure/DependencyInjection.cs`: set `DataProtectionTokenProviderOptions.TokenLifespan` to 48 hours for email verification, 24 hours for password reset per data-model.md
- [x] T009 [P] Create `IEmailSender` interface in `src/Application/Common/Interfaces/IEmailSender.cs` with methods: `SendVerificationEmailAsync(string email, string userId, string token)` and `SendPasswordResetEmailAsync(string email, string token)` per research.md Decision 3
- [x] T010 [P] Implement `SendGridEmailSender` in `src/Infrastructure/Services/SendGridEmailSender.cs` implementing `IEmailSender`: use SendGrid SDK to send verification and password reset emails with deep link URLs (format: `{App.BaseUrl}/api/users/verify-email-redirect?userId={userId}&token={token}` and `{App.BaseUrl}/api/users/reset-password-redirect?email={email}&token={token}`), bind SendGrid config from `IConfiguration` per research.md Decision 3 and quickstart.md
- [x] T011 Register `SendGridEmailSender` as `IEmailSender` in `src/Infrastructure/DependencyInjection.cs` DI container
- [x] T012 [P] Create `useAuth` hook in `src/App/hooks/useAuth.ts`: manage auth state (token, user, isAuthenticated, isLoading), store/retrieve tokens via `expo-secure-store`, expose `login()`, `logout()`, `setTokens()` methods, handle web fallback for secure store per research.md Decision 4
- [x] T013 [P] Create `useApi` hook in `src/App/hooks/useApi.ts`: wrap `fetch` with automatic bearer token injection from `useAuth`, handle 401 responses by attempting token refresh then logout, expose typed `get/post/put/delete` methods, use API base URL from `src/App/constants/api.ts` per research.md Decision 6
- [x] T014 Create auth service functions in `src/App/services/auth.ts`: typed API call functions for all 8 endpoints from contracts/auth-api.yaml — `login(email, password)`, `register(...)`, `googleLogin(idToken)`, `verifyEmail(userId, token)`, `resendVerification(email)`, `forgotPassword(email)`, `resetPassword(email, token, newPassword)`, `refreshToken(refreshToken)` — using the `useApi` hook's fetch wrapper
- [x] T015 Update root layout `src/App/app/_layout.tsx`: wrap app in AuthProvider from `useAuth`, add auth state check to redirect unauthenticated users to `(auth)/login` and authenticated users to `(app)/`
- [x] T016 Create auth route group layout `src/App/app/(auth)/_layout.tsx`: simple Stack layout for auth screens (login, register, forgot-password, reset-password, verify-email) with no header or minimal header
- [x] T017 Create authenticated route group layout `src/App/app/(app)/_layout.tsx`: layout for post-auth screens, redirect to login if not authenticated

**Checkpoint**: Foundation ready — backend has ApplicationUser extended, email sender wired, mobile has auth state management + API client + route groups. User story implementation can now begin.

---

## Phase 3: User Story 1 — Email/Password Login (Priority: P1) MVP

**Goal**: A verified user can log in with email/password and see "Landing Page". Unverified users are blocked with a resend option.

**Independent Test**: Create a verified user directly in DB, log in via mobile app, confirm landing page. Try unverified user and confirm block + resend.

### Implementation for User Story 1

- [x] T018 [US1] Create `LoginCommand` and `LoginCommandHandler` in `src/Application/Identity/Commands/Login/`: accept email + password (per LoginRequest schema in auth-api.yaml), use `UserManager.FindByEmailAsync` + `UserManager.CheckPasswordAsync`, check `EmailConfirmed` — if unverified return 401 with `{ "detail": "email_not_verified" }`, if valid use `SignInManager.PasswordSignInAsync` or generate bearer token via Identity's token endpoint, return `TokenResponse` (accessToken, refreshToken, expiresIn, tokenType) per contracts/auth-api.yaml
- [x] T019 [US1] Create `LoginCommandValidator` in `src/Application/Identity/Commands/Login/LoginCommandValidator.cs`: validate email (required, valid format) and password (required) using FluentValidation per data-model.md
- [x] T020 [US1] Create `ResendVerificationCommand` and handler in `src/Application/Identity/Commands/ResendVerification/`: accept email, find user, if exists and unverified generate new email confirmation token via `UserManager.GenerateEmailConfirmationTokenAsync`, send via `IEmailSender.SendVerificationEmailAsync`, always return 200 (prevent enumeration) per auth-api.yaml and FR-012
- [x] T021 [US1] Map `POST /api/users/login` and `POST /api/users/resend-verification` endpoints in `src/Web/Endpoints/Users.cs` using Minimal API pattern (`EndpointGroupBase`): login delegates to `LoginCommand` via MediatR, resend-verification delegates to `ResendVerificationCommand` — both `.AllowAnonymous()` per plan.md constitution check (Principle IV)
- [x] T022 [US1] Create login screen `src/App/app/(auth)/login.tsx`: modern, clean UI with email + password TextInputs, "Log In" button, "Sign in with Google" button (disabled placeholder for US3), "Register" link (navigation to register screen), "Forgot password?" link (navigation to forgot-password screen) — use theme colors from `src/App/constants/theme.ts` per FR-001 and spec assumption on "modern and clean" design
- [x] T023 [US1] Implement login form logic in `src/App/app/(auth)/login.tsx`: on submit call `auth.login()` from `src/App/services/auth.ts`, on success store tokens via `useAuth.setTokens()` and navigate to `(app)/`, on 401 with `email_not_verified` show "Please verify your email" message with "Resend verification email" button that calls `auth.resendVerification()`, on other 401 show "Invalid email or password" error per acceptance scenarios 1-4 in US1
- [x] T024 [US1] Create landing page `src/App/app/(app)/index.tsx`: display centered text "Landing Page" per FR-005, use theme styling from `src/App/constants/theme.ts`

**Checkpoint**: At this point, email/password login works end-to-end. A verified user logs in and sees "Landing Page". Unverified users are blocked with resend option.

---

## Phase 4: User Story 2 — Email/Password Registration with Email Verification (Priority: P2)

**Goal**: New users register with name/email/password/age, receive verification email, click link to verify, then log in.

**Independent Test**: Fill registration form, confirm verification email sent, click verification link (deep link or web), confirm account verified, log in with new credentials.

### Implementation for User Story 2

- [x] T025 [US2] Create `RegisterCommand` and `RegisterCommandHandler` in `src/Application/Identity/Commands/Register/`: accept firstName, lastName, email, password, age (nullable) per RegisterRequest schema, create `ApplicationUser` with `UserManager.CreateAsync`, set `UserName = email`, generate email confirmation token via `UserManager.GenerateEmailConfirmationTokenAsync`, send via `IEmailSender.SendVerificationEmailAsync`, return 200 on success, return 400 `ValidationProblem` on failure per auth-api.yaml
- [x] T026 [US2] Create `RegisterCommandValidator` in `src/Application/Identity/Commands/Register/RegisterCommandValidator.cs`: validate firstName (required, 1-100 chars, trimmed), lastName (required, 1-100 chars, trimmed), email (required, valid format), password (required, min 8 chars, uppercase, lowercase, digit, special char per FR-011), age (if provided, >= 13 per FR-014) using FluentValidation per data-model.md validation rules
- [x] T027 [US2] Create `VerifyEmailCommand` and handler in `src/Application/Identity/Commands/VerifyEmail/`: accept userId + token per VerifyEmailRequest schema, find user by ID, call `UserManager.ConfirmEmailAsync(user, token)`, return 200 on success, return 400 if token invalid/expired per auth-api.yaml
- [x] T028 [US2] Map `POST /api/users/register` and `POST /api/users/verify-email` endpoints in `src/Web/Endpoints/Users.cs`: register delegates to `RegisterCommand`, verify-email delegates to `VerifyEmailCommand` — both `.AllowAnonymous()` per plan.md (Principle IV)
- [x] T029 [P] [US2] Create web fallback pages for deep links: `src/Web/wwwroot/verify-email.html` (reads userId + token from URL query params, posts to `/api/users/verify-email`, shows success message with "Open App" link using `hoist://` scheme) and configure static file serving in `src/Web/Program.cs` if not already configured per research.md Decision 5
- [x] T030 [P] [US2] Create redirect endpoints in `src/Web/Endpoints/Users.cs`: `GET /api/users/verify-email-redirect` that accepts `userId` and `token` query params and redirects to `hoist://verify-email?userId={userId}&token={token}` (deep link) with web fallback to `wwwroot/verify-email.html` per research.md Decision 5
- [x] T031 [US2] Create registration screen `src/App/app/(auth)/register.tsx`: form with firstName, lastName, email, password TextInputs + optional age NumberInput, "Register" submit button, "Already have an account? Log in" link — use theme from `src/App/constants/theme.ts`, show validation errors per field per FR-006
- [x] T032 [US2] Implement registration form logic in `src/App/app/(auth)/register.tsx`: on submit call `auth.register()` from services/auth.ts, on success show "Check your email for a verification link" message and navigate to login, on 400 display field-level validation errors from `ValidationProblem` response, trim firstName/lastName whitespace before submit per acceptance scenarios 1-6 in US2
- [x] T033 [US2] Create email verification screen `src/App/app/(auth)/verify-email.tsx`: handles deep link `hoist://verify-email?userId=X&token=Y`, extract params from URL via expo-router `useLocalSearchParams`, call `auth.verifyEmail(userId, token)`, show success message with "Go to Login" button or error message if token invalid/expired per acceptance scenario 3 in US2

**Checkpoint**: At this point, full registration + email verification + login flow works. New users register, verify email, then log in.

---

## Phase 5: User Story 3 — Google OAuth Login & Registration (Priority: P3)

**Goal**: Users sign in with Google. New users auto-registered with Google profile data (email pre-verified). Existing users with matching email get Google linked.

**Independent Test**: Click "Sign in with Google", complete Google auth, confirm landing page appears. For new users, confirm account created. For existing email/password users, confirm Google linked.

### Implementation for User Story 3

- [x] T034 [US3] Create `GoogleLoginCommand` and `GoogleLoginCommandHandler` in `src/Application/Identity/Commands/GoogleLogin/`: accept idToken per GoogleLoginRequest schema, validate Google ID token using `Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync` with configured ClientId, extract email/firstName/lastName from Google payload, if user exists by email link Google login via `UserManager.AddLoginAsync` (provider: "Google", providerKey: Google sub) and return bearer token, if no user exists create `ApplicationUser` with `EmailConfirmed = true` + add Google login + return bearer token, return 401 if token invalid per auth-api.yaml and research.md Decision 2
- [x] T035 [US3] Create `GoogleLoginCommandValidator` in `src/Application/Identity/Commands/GoogleLogin/GoogleLoginCommandValidator.cs`: validate idToken (required, not empty) using FluentValidation
- [x] T036 [US3] Map `POST /api/users/google-login` endpoint in `src/Web/Endpoints/Users.cs`: delegates to `GoogleLoginCommand` via MediatR, `.AllowAnonymous()` per auth-api.yaml
- [x] T037 [US3] Implement Google Sign-In on mobile in `src/App/app/(auth)/login.tsx`: configure `expo-auth-session` with Google provider (Google ClientId from constants), on "Sign in with Google" button press trigger `AuthSession.useAuthRequest` flow, on success extract ID token from response, call `auth.googleLogin(idToken)` from services/auth.ts, on success store tokens via `useAuth.setTokens()` and navigate to `(app)/`, on cancel/deny return to login screen silently per acceptance scenarios 1-4 in US3 and research.md Decision 2

**Checkpoint**: At this point, Google OAuth works alongside email/password. Both new and existing users can sign in with Google.

---

## Phase 6: User Story 4 — Forgot Password / Password Reset (Priority: P4)

**Goal**: Users request password reset via email, receive a secure time-limited link, click it to set a new password.

**Independent Test**: Request reset for verified account, confirm email sent, click reset link (deep link or web), enter new password, log in with new password.

### Implementation for User Story 4

- [x] T038 [US4] Create `ForgotPasswordCommand` and handler in `src/Application/Identity/Commands/ForgotPassword/`: accept email per ForgotPasswordRequest schema, find user by email, if exists and has password (not OAuth-only) generate reset token via `UserManager.GeneratePasswordResetTokenAsync`, send via `IEmailSender.SendPasswordResetEmailAsync`, always return 200 regardless of whether user exists (prevent enumeration per FR-012), if user is OAuth-only and no password set skip email send per edge case in spec
- [x] T039 [US4] Create `ResetPasswordCommand` and handler in `src/Application/Identity/Commands/ResetPassword/`: accept email, token, newPassword per ResetPasswordRequest schema, find user by email, call `UserManager.ResetPasswordAsync(user, token, newPassword)`, return 200 on success, return 400 if token invalid/expired or password doesn't meet requirements per auth-api.yaml
- [x] T040 [US4] Create `ResetPasswordCommandValidator` in `src/Application/Identity/Commands/ResetPassword/ResetPasswordCommandValidator.cs`: validate email (required, valid format), token (required), newPassword (required, min 8 chars, uppercase, lowercase, digit, special char per FR-011) using FluentValidation
- [x] T041 [US4] Map `POST /api/users/forgot-password` and `POST /api/users/reset-password` endpoints in `src/Web/Endpoints/Users.cs`: both delegate to their commands via MediatR, both `.AllowAnonymous()` per auth-api.yaml
- [x] T042 [P] [US4] Create web fallback page `src/Web/wwwroot/reset-password.html` (reads email + token from URL query params, shows new password form, posts to `/api/users/reset-password`, shows success message with link to app) and create redirect endpoint `GET /api/users/reset-password-redirect` in `src/Web/Endpoints/Users.cs` that redirects to `hoist://reset-password?email={email}&token={token}` with web fallback per research.md Decision 5
- [x] T043 [US4] Create forgot password screen `src/App/app/(auth)/forgot-password.tsx`: form with email TextInput and "Send Reset Link" button, on submit call `auth.forgotPassword(email)` from services/auth.ts, show "If an account exists with that email, a reset link has been sent" confirmation message (same message for all inputs per FR-012), "Back to Login" link — use theme from `src/App/constants/theme.ts`
- [x] T044 [US4] Create reset password screen `src/App/app/(auth)/reset-password.tsx`: handles deep link `hoist://reset-password?email=X&token=Y`, extract params via expo-router `useLocalSearchParams`, show "New Password" + "Confirm Password" TextInputs, on submit call `auth.resetPassword(email, token, newPassword)`, on success show message and navigate to login, on 400 show error (expired link or weak password) per acceptance scenarios 4-6 in US4

**Checkpoint**: At this point, the full forgot password / reset flow works end-to-end.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Deep link wiring, edge cases, and final integration

- [x] T045 Configure deep link routing in `src/App/app.json` or expo config: ensure `hoist://` scheme handles `verify-email` and `reset-password` paths, map to `(auth)/verify-email` and `(auth)/reset-password` screens respectively per research.md Decision 5
- [x] T046 Handle edge case: already-verified user clicks verification link — in `VerifyEmailCommand` handler, if `EmailConfirmed` already true, return 200 with message "Email already verified" per edge case in spec
- [x] T047 Handle edge case: OAuth-only user uses forgot password — in `ForgotPasswordCommand` handler, check if user has a password hash, if not skip email and still return 200 (already handled in T038, verify) per edge case in spec
- [x] T048 Add password strength indicator or requirements hint to registration (`src/App/app/(auth)/register.tsx`) and reset password (`src/App/app/(auth)/reset-password.tsx`) screens — show "Password must be at least 8 characters with uppercase, lowercase, digit, and special character" per FR-011
- [x] T049 Run quickstart.md verification checklist: start backend via `dotnet run --project src/AppHost`, start mobile via `npx expo start` in `src/App/`, test all 11 checklist items from quickstart.md verification section

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (P1): Can start immediately after Phase 2
  - US2 (P2): Can start immediately after Phase 2 (independent of US1)
  - US3 (P3): Can start immediately after Phase 2 (independent of US1/US2)
  - US4 (P4): Can start immediately after Phase 2 (independent of US1-US3, though shares email infrastructure from Phase 2)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (Login)**: Depends only on Phase 2 foundation. No cross-story dependencies.
- **US2 (Registration + Verification)**: Depends only on Phase 2 foundation. Shares `IEmailSender` from Phase 2 T009-T011.
- **US3 (Google OAuth)**: Depends only on Phase 2 foundation. Modifies `login.tsx` (also modified by US1 T022-T023), so if doing sequentially, do US1 before US3.
- **US4 (Password Reset)**: Depends only on Phase 2 foundation. Shares `IEmailSender` from Phase 2 T009-T011.

### Within Each User Story

- Backend commands/validators before endpoints
- Endpoints before mobile screens
- Mobile UI before mobile logic integration
- Web fallback pages can be parallel with mobile screens

### Parallel Opportunities

- T004, T005 can run in parallel (different files)
- T009, T010 can run in parallel with T012, T013 (backend interfaces vs mobile hooks)
- T029, T030 can run in parallel with T031-T033 (web fallback vs mobile screens)
- T042 can run in parallel with T043-T044 (web fallback vs mobile screens)
- Within Phase 2, backend tasks (T006-T011) and mobile tasks (T012-T017) can run in parallel

---

## Parallel Example: User Story 2

```bash
# Backend (can run in parallel):
Task T025: "Create RegisterCommand in src/Application/Identity/Commands/Register/"
Task T026: "Create RegisterCommandValidator in src/Application/Identity/Commands/Register/"

# After T025-T026, map endpoints:
Task T028: "Map POST /api/users/register and POST /api/users/verify-email in src/Web/Endpoints/Users.cs"

# Mobile + Web fallback (can run in parallel with each other, after T028):
Task T029: "Create web fallback verify-email.html in src/Web/wwwroot/"
Task T031: "Create registration screen in src/App/app/(auth)/register.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: Foundational (T006-T017)
3. Complete Phase 3: User Story 1 — Login (T018-T024)
4. **STOP and VALIDATE**: Test login with pre-seeded verified user
5. Deploy/demo if ready — users can log in and see "Landing Page"

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Login) → Test independently → Deploy (MVP!)
3. Add US2 (Registration + Verification) → Test independently → Deploy
4. Add US3 (Google OAuth) → Test independently → Deploy
5. Add US4 (Password Reset) → Test independently → Deploy
6. Polish → Final validation → Deploy

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Login) + US3 (Google OAuth — modifies same login.tsx)
   - Developer B: US2 (Registration + Verification)
   - Developer C: US4 (Password Reset)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- The existing `MapIdentityApi<ApplicationUser>()` in `src/Web/Endpoints/Users.cs` provides built-in `/api/users/register` and `/api/users/login` — custom endpoints may need to replace or extend these defaults
- Identity bearer token generation is already configured in `src/Infrastructure/DependencyInjection.cs` — leverage `SignInManager` for token issuance
- All endpoint tasks modify the same file (`src/Web/Endpoints/Users.cs`) — execute sequentially or batch endpoint additions
