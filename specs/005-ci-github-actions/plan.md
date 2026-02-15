# Implementation Plan: CI/CD GitHub Actions Pipelines

**Branch**: `005-ci-github-actions` | **Date**: 2026-02-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-ci-github-actions/spec.md`

## Summary

Create two GitHub Actions workflow files: a **PR quality gate** (`pr.yml`) that validates builds, formatting, linting, and tests on every pull request targeting `master`, and a **CI workflow** (`ci.yml`) that validates builds and tests on every push to `master`. Configure branch protection rules to require the PR workflow as a required status check.

## Technical Context

**Language/Version**: .NET 10.0 (C#), Node.js (for Expo/React Native frontend)
**Primary Dependencies**: GitHub Actions, Docker (for Testcontainers SQL Server in tests)
**Storage**: N/A (CI/CD configuration only)
**Testing**: `dotnet test` (NUnit, 4 test projects), `jest` (via `npm test` in `src/App/`)
**Target Platform**: GitHub-hosted Ubuntu runners
**Project Type**: Infrastructure/CI — no source code changes, only workflow YAML files
**Performance Goals**: N/A
**Constraints**: GitHub Actions runner resources (Ubuntu latest); Docker available on GitHub-hosted runners
**Scale/Scope**: 2 workflow files, branch protection configuration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Applicable? | Status | Notes |
|-----------|-------------|--------|-------|
| I. Clean Architecture | No | PASS | CI/CD config, no backend code changes |
| II. CQRS via MediatR | No | PASS | No backend code changes |
| III. Domain-Driven Design | No | PASS | No domain changes |
| IV. API-First with Minimal APIs | No | PASS | No API changes |
| V. Test-Driven Quality | Yes | PASS | Workflows enforce the existing test strategy (unit, functional, integration) |
| VI. Fitness Domain Integrity | No | PASS | No domain changes |
| VII. Simplicity and YAGNI | Yes | PASS | Two simple workflow files, no over-engineering |
| VIII. Mobile-First with Expo RN | No | PASS | No mobile code changes, just running existing lint/test scripts |

**Gate result: PASS** — No violations. This feature creates infrastructure to enforce existing principles, particularly V (Test-Driven Quality).

## Project Structure

### Documentation (this feature)

```text
specs/005-ci-github-actions/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
.github/
└── workflows/
    ├── pr.yml           # PR quality gate workflow (NEW)
    └── ci.yml           # Post-merge CI workflow (NEW)
```

**Structure Decision**: This feature only adds GitHub Actions workflow YAML files under `.github/workflows/`. No existing source code directories are modified. The `data-model.md` and `contracts/` artifacts are not applicable for this infrastructure-only feature.

## Complexity Tracking

No violations to track. Two YAML files with standard GitHub Actions patterns.

---

## Phase 0: Research

### R-001: GitHub Actions .NET Workflow Best Practices

**Decision**: Use `actions/setup-dotnet@v4` with .NET 10.0.x, run `dotnet format --verify-no-changes` for format validation, and `dotnet test` from the repo root to execute all test projects.

**Rationale**: Standard GitHub Actions approach for .NET projects. The `--verify-no-changes` flag on `dotnet format` exits with non-zero status if any files would be modified, making it ideal for CI format checks. Running `dotnet test` from root discovers all test projects via the `Hoist.slnx` solution file.

**Alternatives considered**:
- Running test projects individually: More complex, no benefit since `dotnet test` discovers all projects automatically.
- Using `dotnet format --check`: Deprecated in favor of `--verify-no-changes`.

### R-002: Testcontainers SQL Server in GitHub Actions

**Decision**: GitHub-hosted Ubuntu runners include Docker by default. Testcontainers will work out of the box — no special Docker service configuration needed. The `Application.FunctionalTests` project uses `Testcontainers.MsSql` which manages its own container lifecycle.

**Rationale**: Testcontainers is self-contained; it pulls and manages the SQL Server container image automatically during test execution. No `services:` block needed in the workflow YAML.

**Alternatives considered**:
- Using a GitHub Actions `services:` block with a SQL Server image: Unnecessary since Testcontainers handles this. Would create a redundant container.
- Skipping functional tests in CI: Defeats the purpose of the CI pipeline.

### R-003: Frontend Build/Test/Lint Commands

**Decision**: Use `npm ci` for dependency installation, `npx expo lint` (aliased as `npm run lint`) for linting, and `npm test` for running Jest tests. Working directory: `src/App/`.

**Rationale**: The project uses npm (evidenced by `package-lock.json`). `npm ci` is preferred over `npm install` in CI for reproducible builds. The existing `package.json` scripts are: `lint` → `expo lint`, `test` → `jest`.

**Alternatives considered**:
- Using `yarn` or `pnpm`: Project uses npm with `package-lock.json`.
- Running `npx jest` directly: `npm test` already maps to `jest` and is more conventional.

### R-004: Workflow Parallelism Strategy

**Decision**: For the PR workflow, use separate jobs for backend and frontend checks. Within the backend job, run build → format check → tests sequentially (tests depend on build). Within the frontend job, run install → lint → tests sequentially. The backend and frontend jobs run in parallel.

**Rationale**: Backend and frontend are independent and can run concurrently. Within each, steps are sequential because later steps depend on earlier ones (e.g., tests require a successful build, lint/test require `npm ci`).

**Alternatives considered**:
- All steps in a single job: Slower, no parallelism between BE and FE.
- Separate jobs per check (build, format, lint, test): Over-engineered, excessive job overhead, and creates complex dependency graphs.

### R-005: Branch Protection Configuration

**Decision**: Use `gh` CLI to configure branch protection rules on `master` requiring the PR workflow jobs as required status checks. Document the manual steps as an alternative if `gh` CLI access is unavailable.

**Rationale**: The `gh` CLI can programmatically configure branch protection via `gh api` calls to the GitHub REST API. This makes the setup reproducible and scriptable.

**Alternatives considered**:
- Manual GitHub UI configuration only: Not reproducible, easy to misconfigure.
- GitHub branch rulesets (newer feature): More complex, overkill for simple required checks.

### R-006: Node.js Version

**Decision**: Use Node.js 22.x (current LTS) with `actions/setup-node@v4`.

**Rationale**: Expo SDK 54 supports Node 18+. Node 22 is the active LTS as of 2026. No `.nvmrc` or `engines` field constrains the version.

**Alternatives considered**:
- Node 18.x: Older LTS, still supported but not recommended for new setups.
- Node 20.x: Previous LTS, also valid but 22.x is current.

---

## Phase 1: Design

### Workflow Design: PR Quality Gate (`pr.yml`)

**Trigger**: `pull_request` targeting `master` (events: `opened`, `synchronize`, `reopened`)

**Jobs**:

#### Job: `backend` (runs on `ubuntu-latest`)
1. **Checkout** — `actions/checkout@v4`
2. **Setup .NET** — `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`
3. **Restore** — `dotnet restore`
4. **Build** — `dotnet build --no-restore -tl`
5. **Format check** — `dotnet format --verify-no-changes --no-restore`
6. **Test** — `dotnet test --no-build`

#### Job: `frontend` (runs on `ubuntu-latest`, parallel with backend)
1. **Checkout** — `actions/checkout@v4`
2. **Setup Node** — `actions/setup-node@v4` with `node-version: '22'`
3. **Install deps** — `npm ci` (working directory: `src/App`)
4. **Lint** — `npm run lint` (working directory: `src/App`)
5. **Test** — `npm test` (working directory: `src/App`)

### Workflow Design: CI Build (`ci.yml`)

**Trigger**: `push` to `master`

**Jobs**:

#### Job: `backend` (runs on `ubuntu-latest`)
1. **Checkout** — `actions/checkout@v4`
2. **Setup .NET** — `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`
3. **Restore** — `dotnet restore`
4. **Build** — `dotnet build --no-restore -tl`
5. **Test** — `dotnet test --no-build`

#### Job: `frontend` (runs on `ubuntu-latest`, parallel with backend)
1. **Checkout** — `actions/checkout@v4`
2. **Setup Node** — `actions/setup-node@v4` with `node-version: '22'`
3. **Install deps** — `npm ci` (working directory: `src/App`)
4. **Test** — `npm test` (working directory: `src/App`)

**Key difference from PR workflow**: No `dotnet format` step, no `npm run lint` step.

### Branch Protection Configuration

Configure via `gh` CLI after workflows are merged:
- Required status checks: `backend` and `frontend` jobs from the PR workflow
- Require branches to be up to date before merging: recommended but optional
- Enforce for administrators: recommended

### Quickstart

See `quickstart.md` for developer verification steps after implementation.
