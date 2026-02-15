# Feature Specification: CI/CD GitHub Actions Pipelines

**Feature Branch**: `005-ci-github-actions`
**Created**: 2026-02-15
**Status**: Draft
**Input**: User description: "I want to create github actions that run when a PR is opened and when a branch is merged into the default branch. The PR gate build should confirm the solution builds, the dotnet format has no changes, the frontend application lints successfully, and all tests in the BE and FE pass. When the feature branch is merged into CI it should confirm that the projects build and all tests pass. It should not confirm linting in the CI build."

## Clarifications

### Session 2026-02-15

- Q: Should configuring branch protection rules be in scope for this feature? → A: Yes, in scope — configure branch protection to require the PR workflow as a required status check.

## User Scenarios & Testing

### User Story 1 - PR Quality Gate (Priority: P1)

As a developer, when I open a pull request against the default branch, an automated workflow runs to validate my changes. The workflow confirms the backend solution builds, verifies code formatting compliance (dotnet format), checks that the frontend application passes linting, and runs all backend and frontend tests. The PR cannot be merged until all checks pass.

**Why this priority**: The PR gate is the primary mechanism for catching issues before code reaches the default branch. It enforces quality standards and prevents regressions from being merged.

**Independent Test**: Can be fully tested by opening a PR with intentional issues (formatting violations, lint errors, failing tests, build errors) and verifying each check correctly reports failure. A clean PR should show all checks passing.

**Acceptance Scenarios**:

1. **Given** a developer opens a PR against the default branch, **When** the workflow triggers, **Then** the backend solution is built and the result is reported as a check on the PR.
2. **Given** a developer opens a PR with code formatting violations, **When** the workflow runs dotnet format validation, **Then** the check fails and the PR is blocked from merging.
3. **Given** a developer opens a PR with frontend lint errors, **When** the workflow runs frontend linting, **Then** the check fails and the PR is blocked from merging.
4. **Given** a developer opens a PR with failing backend tests, **When** the workflow runs backend tests, **Then** the check fails and the PR is blocked from merging.
5. **Given** a developer opens a PR with failing frontend tests, **When** the workflow runs frontend tests, **Then** the check fails and the PR is blocked from merging.
6. **Given** a developer opens a PR where all checks pass, **When** the workflow completes, **Then** all checks are reported as successful and the PR is eligible for merge.
7. **Given** a developer pushes additional commits to an open PR, **When** the new commits are pushed, **Then** the workflow re-runs against the updated code.

---

### User Story 2 - CI Build on Merge (Priority: P2)

As a team, when a feature branch is merged into the default branch, an automated workflow runs to confirm the merged code builds and all tests pass. This workflow does not check formatting or linting -- it focuses solely on build integrity and test results.

**Why this priority**: Post-merge CI provides a safety net confirming the default branch remains in a healthy state after integration. It is secondary to the PR gate since the gate should catch most issues.

**Independent Test**: Can be fully tested by merging a branch into the default branch and verifying the workflow runs, builds the projects, and executes all tests without performing any lint or format checks.

**Acceptance Scenarios**:

1. **Given** a feature branch is merged into the default branch, **When** the merge completes, **Then** the CI workflow is triggered automatically.
2. **Given** the CI workflow runs after a merge, **When** the backend solution is built, **Then** a build failure is reported as a failed workflow run.
3. **Given** the CI workflow runs after a merge, **When** backend tests execute, **Then** test failures are reported in the workflow results.
4. **Given** the CI workflow runs after a merge, **When** frontend tests execute, **Then** test failures are reported in the workflow results.
5. **Given** the CI workflow runs after a merge, **When** all builds and tests pass, **Then** the workflow completes successfully.
6. **Given** the CI workflow runs after a merge, **Then** no formatting or linting checks are performed.

---

### Edge Cases

- What happens when the workflow encounters a flaky test? The workflow reports the failure; re-running the workflow is possible from the GitHub Actions UI.
- What happens when a dependency (e.g., NuGet package, npm package) is unavailable during the workflow? The build step fails and the workflow reports the failure with the relevant error.
- What happens when the PR targets a branch other than the default branch? The PR workflow should only trigger for PRs targeting the default branch.
- What happens when a PR is opened as a draft? The workflow should still run on draft PRs to give early feedback.

## Requirements

### Functional Requirements

- **FR-001**: System MUST run a PR workflow when a pull request is opened, synchronized (new commits pushed), or reopened against the default branch.
- **FR-002**: The PR workflow MUST build the backend solution and report success or failure.
- **FR-003**: The PR workflow MUST verify that code formatting produces no changes (i.e., code is already properly formatted).
- **FR-004**: The PR workflow MUST run frontend linting and report success or failure.
- **FR-005**: The PR workflow MUST run all backend tests and report success or failure.
- **FR-006**: The PR workflow MUST run all frontend tests and report success or failure.
- **FR-007**: System MUST run a CI workflow when code is pushed to the default branch (i.e., after a merge).
- **FR-008**: The CI workflow MUST build the backend solution and report success or failure.
- **FR-009**: The CI workflow MUST build the frontend application and report success or failure.
- **FR-010**: The CI workflow MUST run all backend tests and report success or failure.
- **FR-011**: The CI workflow MUST run all frontend tests and report success or failure.
- **FR-012**: The CI workflow MUST NOT include formatting or linting checks.
- **FR-013**: Both workflows MUST provide clear, actionable feedback when a step fails (visible in workflow logs).
- **FR-014**: The PR workflow steps SHOULD run in parallel where possible to minimize total execution time.
- **FR-015**: Branch protection rules MUST be configured on the default branch to require the PR workflow as a required status check before merging.

## Assumptions

- The default branch is `master` (based on current repository configuration).
- Backend tests require a SQL Server instance (provided via a Docker service container in the workflow, consistent with the project's use of Testcontainers).
- The frontend application is located in a standard Expo/React Native project directory and uses standard npm/yarn scripts for linting and testing.
- The workflows will use GitHub-hosted runners (Ubuntu).
- No secrets or external service credentials are required for build and test steps beyond what GitHub provides by default.
- Draft PRs should also trigger the PR workflow for early feedback.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Every pull request targeting the default branch automatically receives build, format, lint, and test feedback without manual intervention.
- **SC-002**: No code with formatting violations, lint errors, or failing tests can be merged to the default branch, enforced by branch protection rules requiring the PR workflow to pass.
- **SC-003**: Every merge to the default branch triggers a CI build and test run that confirms integration health.
- **SC-004**: Developers can identify the specific failing step and error details directly from the workflow summary without needing to reproduce locally.
- **SC-005**: The CI workflow (post-merge) does not include any linting or formatting checks, keeping it focused on build and test integrity.
