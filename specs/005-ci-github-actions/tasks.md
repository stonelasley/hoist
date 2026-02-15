# Tasks: CI/CD GitHub Actions Pipelines

**Input**: Design documents from `/specs/005-ci-github-actions/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md

**Tests**: No test tasks — this feature is infrastructure-only (workflow YAML files). Verification is done via manual PR/merge testing per quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the GitHub Actions workflow directory structure

- [x] T001 Create `.github/workflows/` directory at repository root

---

## Phase 2: User Story 1 - PR Quality Gate (Priority: P1) MVP

**Goal**: Automated workflow validates every PR targeting `master` — builds backend, checks formatting, lints frontend, runs all tests.

**Independent Test**: Open a PR against `master` and verify the `backend` and `frontend` jobs appear in the PR checks tab. Confirm format violations fail the backend job, lint errors fail the frontend job, and a clean PR passes all checks.

### Implementation for User Story 1

- [x] T002 [US1] Create PR quality gate workflow in `.github/workflows/pr.yml` with:
  - Trigger: `pull_request` targeting `master` (opened, synchronize, reopened)
  - Job `backend` on `ubuntu-latest`: checkout (`actions/checkout@v4`), setup .NET 10.0.x (`actions/setup-dotnet@v4`), `dotnet restore`, `dotnet build --no-restore -tl`, `dotnet format --verify-no-changes --no-restore`, `dotnet test --no-build`
  - Job `frontend` on `ubuntu-latest` (parallel with backend): checkout (`actions/checkout@v4`), setup Node 22 (`actions/setup-node@v4`), `npm ci` (working-directory: `src/App`), `npm run lint` (working-directory: `src/App`), `npm test` (working-directory: `src/App`)

**Checkpoint**: PR workflow file exists. Opening a PR against `master` triggers both `backend` and `frontend` jobs.

---

## Phase 3: User Story 2 - CI Build on Merge (Priority: P2)

**Goal**: Automated workflow validates every push to `master` — builds and tests only, no formatting or linting checks.

**Independent Test**: Push or merge to `master` and verify the CI workflow runs with `backend` and `frontend` jobs. Confirm no lint or format steps are present.

### Implementation for User Story 2

- [x] T003 [P] [US2] Create CI build workflow in `.github/workflows/ci.yml` with:
  - Trigger: `push` to `master`
  - Job `backend` on `ubuntu-latest`: checkout (`actions/checkout@v4`), setup .NET 10.0.x (`actions/setup-dotnet@v4`), `dotnet restore`, `dotnet build --no-restore -tl`, `dotnet test --no-build`
  - Job `frontend` on `ubuntu-latest` (parallel with backend): checkout (`actions/checkout@v4`), setup Node 22 (`actions/setup-node@v4`), `npm ci` (working-directory: `src/App`), `npm test` (working-directory: `src/App`)
  - No `dotnet format` step, no `npm run lint` step

**Checkpoint**: CI workflow file exists. Pushing to `master` triggers both `backend` and `frontend` jobs without lint/format checks.

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Branch protection enforcement and final validation

- [x] T004 Configure branch protection rules on `master` via `gh` CLI requiring `backend` and `frontend` status checks from the PR workflow before merge is allowed
- [x] T005 Run quickstart.md validation steps to confirm both workflows trigger correctly and branch protection blocks merges without passing checks

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **US1 (Phase 2)**: Depends on Phase 1 (directory must exist)
- **US2 (Phase 3)**: Depends on Phase 1 (directory must exist) — can run in parallel with US1 (different files)
- **Polish (Phase 4)**: Depends on Phase 2 (PR workflow must exist for branch protection to reference its status checks). Depends on both workflows being merged to `master` for validation.

### User Story Dependencies

- **User Story 1 (P1)**: Independent — creates `pr.yml`
- **User Story 2 (P2)**: Independent — creates `ci.yml` in a different file, can run in parallel with US1

### Parallel Opportunities

- T002 and T003 modify different files (`pr.yml` vs `ci.yml`) and can be implemented in parallel
- T004 must wait until PR workflow exists and has run at least once (GitHub requires status checks to have been reported before they can be required)
- T005 must wait until both workflows are merged and branch protection is configured

---

## Parallel Example: User Stories 1 & 2

```bash
# These can run in parallel (different files):
Task T002: "Create PR quality gate workflow in .github/workflows/pr.yml"
Task T003: "Create CI build workflow in .github/workflows/ci.yml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (create directory)
2. Complete Phase 2: User Story 1 (pr.yml)
3. **STOP and VALIDATE**: Open a test PR and verify all checks run
4. Merge to `master` if ready

### Incremental Delivery

1. Complete Setup → Directory ready
2. Add US1 (pr.yml) → Test via PR → Merge (MVP!)
3. Add US2 (ci.yml) → Test via push to master → Merge
4. Configure branch protection (T004) → Validate enforcement (T005)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- T004 (branch protection) requires the PR workflow to have run at least once so GitHub recognizes the status check names
- Both workflow files are self-contained YAML — no shared templates or reusable workflows needed
- Commit after each task or logical group
