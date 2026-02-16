# Quickstart: Auth Route Guard

## Prerequisites

- Node.js installed
- Expo CLI: `npx expo start` from `src/App/`
- Backend running: `dotnet run --project src/AppHost`
- Test account: `test@test.com` / `Test1234!`

## What Changed

1. **`src/App/app/_layout.tsx`** — Added `AuthGate` component that redirects based on auth state
2. **Deleted `src/App/app/(tabs)/`** — Removed unused Expo boilerplate (3 files)

## Manual Testing

### Test 1: Unauthenticated Launch
1. Clear app data or use a fresh browser session (web tokens are in-memory)
2. Open `http://localhost:8081`
3. **Expected**: Login screen appears (not the landing page or empty content)

### Test 2: Login Flow
1. From the login screen, enter `test@test.com` / `Test1234!`
2. **Expected**: Redirects to the landing page with workouts and exercises listed

### Test 3: Authenticated Launch (Native)
1. Log in on a native device/simulator
2. Close and reopen the app
3. **Expected**: Landing page appears directly (no login screen flash)

### Test 4: Authenticated Launch (Web)
1. Log in on web
2. Refresh the page
3. **Expected**: Login screen appears (tokens are in-memory on web, lost on refresh)

### Test 5: Navigation Guard
1. While not authenticated, manually navigate to `http://localhost:8081/(app)`
2. **Expected**: Redirected to login screen

### Test 6: Logout (when implemented)
1. Call `clearTokens()` from any screen
2. **Expected**: Automatically redirected to login screen

### Test 7: No Screen Flash
1. Observe app startup in all test cases above
2. **Expected**: Brief loading indicator, then the correct screen — no visible flash of wrong screen
