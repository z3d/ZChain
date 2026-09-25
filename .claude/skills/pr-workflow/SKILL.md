---
name: pr-workflow
description: Create branches, commits, and pull requests following project conventions. Use when the user wants to commit changes, create a PR, push code, or mentions git workflow.
allowed-tools: Bash, Read, Glob
---

# Pull Request Workflow

Branch off an up-to-date `main` as `feature/`, `fix/`, `chore/`, or `refactor/` + a short description.

**Before every commit**, in this order:

```bash
dotnet format src/ZChain.sln
dotnet build src/ZChain.sln    # warnings are errors, so this is a real gate
dotnet test src/ZChain.sln
```

Commit messages are imperative mood, ~50-char summary, body explaining **why** rather than what, and:

```
Co-Authored-By: Claude <noreply@anthropic.com>
```

PR body:

```markdown
## Summary
- Change 1
- Change 2

## Test plan
- [x] Unit tests pass
- [x] Integration tests pass
- [ ] Manual testing (if applicable)

Generated with [Claude Code](https://claude.com/claude-code)
```

Include benchmark numbers in the PR description whenever the change touches mining, hashing, or concurrency — a performance claim without a before/after on the same machine isn't reviewable.

CodeRabbit reviews automatically; triage its Critical and High findings before merging. Merge with `gh pr merge <number> --squash` to keep `main` linear, then delete the local branch.

This repo commits under a specific identity:

```bash
git config user.name "z3d"
git config user.email "925699+z3d@users.noreply.github.com"
```

## Related skills

- `security-review` — the audit pass before merging crypto or concurrency changes
- `benchmark` — producing the numbers a performance PR needs
