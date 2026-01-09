# Git Workflow

## Branch Strategy

### Naming Convention

```
feature/short-description    # New functionality
fix/issue-description        # Bug fixes
chore/maintenance-task       # Dependencies, config, cleanup
refactor/what-changed        # Code restructuring without behavior change
```

### Main Branch Protection

- `main` is the default branch
- All changes via pull request
- CI must pass before merge
- CodeRabbit review is informational (not blocking)

## Commits

### Message Format

```
Short summary in imperative mood (50 chars max)

Optional longer description explaining the "why" not the "what".
Wrap at 72 characters.

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Good Examples

```
Add multi-threaded mining support

Implement parallel nonce search using Task.WhenAny to race
threads. First thread to find valid hash wins and cancels others.

Co-Authored-By: Claude <noreply@anthropic.com>
```

```
Fix block state validation in verification

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Avoid

- `WIP`, `temp`, `fix stuff`
- Past tense: "Added feature" → "Add feature"
- Redundant prefixes: "feat:", "fix:" (branch name indicates this)

## Pull Requests

### Title

Match the primary commit message - short, imperative mood.

### Description Template

```markdown
## Summary
Brief description of changes (1-3 bullets)

## Test plan
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual verification (if applicable)

Generated with [Claude Code](https://claude.com/claude-code)
```

### Review Process

1. CodeRabbit provides automated review
2. Address any security or regression concerns
3. CI must be green
4. Squash merge to main (keeps history clean)

## Common Operations

### Starting New Work

```bash
git checkout main
git pull
git checkout -b feature/my-feature
```

### Updating Branch with Main

```bash
git fetch origin
git rebase origin/main
# Resolve conflicts if any
git push --force-with-lease
```

### Creating PR

```bash
gh pr create --title "Add feature X" --body "## Summary
- Added X
- Updated Y

## Test plan
- [x] Tests pass"
```

### After PR Merged

```bash
git checkout main
git pull
git branch -d feature/my-feature  # Delete local branch
```

## GitHub Actions

### CI Pipeline (`.github/workflows/dotnet.yml`)

Triggers:
- Push to `main`
- Pull requests to `main`

Steps:
1. Restore dependencies
2. Build solution
3. Run all tests

### Security Scanning (`.github/workflows/codeql.yml`)

- Runs on PRs and weekly schedule
- CodeQL analysis for C#
- Check results in Security tab

## Repository Settings

### User Configuration (this repo)

```bash
git config user.name "z3d"
git config user.email "925699+z3d@users.noreply.github.com"
```

### Files to Never Commit

- `BenchmarkDotNet.Artifacts/` - Benchmark results (machine-specific)
- `*.user` files
- `.vs/` directory
- `bin/`, `obj/` directories (in .gitignore)
