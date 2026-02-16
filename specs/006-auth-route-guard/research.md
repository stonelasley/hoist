# Research: Auth Route Guard

## R1: Expo Router Auth Pattern

**Decision**: Use `useSegments()` + `useRouter()` in the root layout to redirect based on auth state.

**Rationale**: This is the documented Expo Router pattern for auth-based routing. It uses file-based routing conventions — route groups `(auth)` and `(app)` already exist in the project and naturally segment public vs. protected routes. The `useSegments()` hook returns the current route segments, allowing the guard to check which group the user is in without tracking individual routes.

**Alternatives considered**:
- **Per-screen auth check**: Each screen checks `isAuthenticated` and redirects individually. Rejected: violates DRY, easy to miss a screen, and causes visible screen flash before redirect.
- **Navigation listener**: Use React Navigation's `onStateChange` to intercept navigation. Rejected: more complex, doesn't prevent initial render of wrong screen, and fights against Expo Router's file-based paradigm.
- **Middleware/redirect config**: Expo Router doesn't have middleware. The `useSegments` pattern is the framework-endorsed approach.

**Key implementation detail**: The guard component must be a child of `AuthProvider` (to access auth context) but must wrap the `<Slot />` (to control what renders). This means it's an inner component within `RootLayout`, not the layout itself.

## R2: Preventing Screen Flash

**Decision**: Return `ActivityIndicator` from the auth gate while `isLoading` is `true`, instead of rendering `<Slot />`.

**Rationale**: If the navigation stack (`<Slot />` or `<Stack>`) renders before auth state is known, the router will display whichever screen matches the current URL — then immediately redirect, causing a visible flash. By not rendering the navigation tree at all during loading, no screen appears until the correct destination is known.

**Alternatives considered**:
- **SplashScreen.preventAutoHideAsync()**: Keep the native splash screen visible until auth resolves. Viable but adds complexity (need to call `hideAsync` at the right time) and the current auth loading is fast enough (~50ms on native with SecureStore) that a simple ActivityIndicator suffices.
- **Render Stack but hide with opacity**: Still mounts screens and triggers their effects. Rejected.

## R3: Logout Redirect

**Decision**: No explicit logout redirect code needed. The auth guard's `useEffect` automatically handles it.

**Rationale**: When `clearTokens()` is called (from any screen), it sets `isAuthenticated` to `false` in the auth context. The `AuthGate` component's `useEffect` watches `isAuthenticated` and `segments`, so it fires and redirects to `/(auth)/login`. This is the same mechanism that protects routes on initial load.

**Alternatives considered**:
- **Explicit `router.replace('/(auth)/login')` after `clearTokens()`**: Redundant since the guard handles it. Could cause double navigation if both fire.

## R4: (tabs) Boilerplate Cleanup

**Decision**: Delete `src/App/app/(tabs)/` directory entirely (3 files).

**Rationale**: These files are leftover from the Expo `create-expo-app` template. They contain a default "Welcome" page and "Explore" tab that are not part of the Hoist app. The `(tabs)` route group is never referenced by any navigation code. Removing it prevents Expo Router from auto-discovering these routes and potentially interfering with the auth guard's routing.

**Risk**: None. No code references these files. The `(tabs)` group is not declared in the root `<Stack>` but Expo Router auto-discovers route groups from the filesystem, so the directory's mere existence could cause routing ambiguity.
