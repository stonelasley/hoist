# Quickstart: CI/CD GitHub Actions Pipelines

**Feature**: 005-ci-github-actions

## Verification Steps

### 1. Validate Workflow Files

Confirm the workflow files exist and have valid YAML syntax:

```bash
# Check files exist
ls .github/workflows/pr.yml .github/workflows/ci.yml

# Validate YAML syntax (requires yq or similar)
gh workflow list
```

### 2. Test PR Workflow

1. Create a test branch from `master`
2. Make a small change (e.g., add a comment)
3. Push and open a PR against `master`
4. Verify in the PR's "Checks" tab:
   - `backend` job runs: build, format check, tests
   - `frontend` job runs: install, lint, tests
   - Both jobs complete (pass or fail as expected)

### 3. Test PR Workflow Failure Cases

**Format violation**:
```bash
# Introduce a formatting issue in any .cs file
# Open PR → backend job should fail at format check step
```

**Lint violation**:
```bash
# Introduce a lint error in src/App/
# Open PR → frontend job should fail at lint step
```

### 4. Test CI Workflow

1. Merge a PR to `master`
2. Navigate to Actions tab → CI workflow
3. Verify:
   - `backend` job runs: build, tests (no format check)
   - `frontend` job runs: install, tests (no lint)

### 5. Verify Branch Protection

```bash
# Check branch protection rules
gh api repos/{owner}/{repo}/branches/master/protection
```

Confirm:
- Required status checks include `backend` and `frontend`
- PR workflow must pass before merge is allowed
