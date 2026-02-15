# Research: CI/CD GitHub Actions Pipelines

**Feature**: 005-ci-github-actions
**Date**: 2026-02-15

## R-001: GitHub Actions .NET Workflow Best Practices

**Decision**: Use `actions/setup-dotnet@v4` with .NET 10.0.x, `dotnet format --verify-no-changes` for format validation, and `dotnet test` from repo root.

**Rationale**: Standard GitHub Actions approach. `--verify-no-changes` exits non-zero if files would be modified. `dotnet test` discovers all projects via `Hoist.slnx`.

**Alternatives considered**:
- Running test projects individually — unnecessary complexity
- `dotnet format --check` — deprecated in favor of `--verify-no-changes`

## R-002: Testcontainers SQL Server in GitHub Actions

**Decision**: No special Docker configuration needed. GitHub-hosted Ubuntu runners include Docker. Testcontainers manages its own SQL Server container lifecycle.

**Rationale**: `Application.FunctionalTests` uses `Testcontainers.MsSql` which is self-contained.

**Alternatives considered**:
- GitHub Actions `services:` block — redundant with Testcontainers
- Skipping functional tests in CI — defeats pipeline purpose

## R-003: Frontend Build/Test/Lint Commands

**Decision**: `npm ci` for install, `npm run lint` (`expo lint`) for linting, `npm test` (`jest`) for tests. Working directory: `src/App/`.

**Rationale**: Project uses npm (`package-lock.json` present). `npm ci` ensures reproducible installs.

**Alternatives considered**:
- yarn/pnpm — not used by project
- `npx jest` directly — `npm test` is conventional

## R-004: Workflow Parallelism Strategy

**Decision**: Two parallel jobs per workflow (backend + frontend). Steps within each job run sequentially.

**Rationale**: BE and FE are independent. Within each, steps depend on prior steps (build before test, install before lint/test).

**Alternatives considered**:
- Single job — slower, no parallelism
- Separate job per check — over-engineered, excessive overhead

## R-005: Branch Protection Configuration

**Decision**: Use `gh` CLI to configure required status checks on `master`. Document manual alternative.

**Rationale**: Programmatic, reproducible configuration.

**Alternatives considered**:
- Manual GitHub UI only — not reproducible
- GitHub branch rulesets — overkill for simple required checks

## R-006: Node.js Version

**Decision**: Node.js 22.x (current LTS) with `actions/setup-node@v4`.

**Rationale**: Expo SDK 54 supports Node 18+. Node 22 is active LTS. No version constraint in project.

**Alternatives considered**:
- Node 18.x/20.x — older LTS versions, valid but not preferred
