---
name: Always sync main before branching and before opening PRs
description: Fetch and pull origin/main before creating branches AND before opening PRs to avoid conflicts and stale state
type: feedback
---

Always fetch and pull main at two key moments:
1. **Before creating a new branch** — sync main first, then branch from it.
2. **Before opening a PR** — fetch main again to ensure nothing drifted.

**Why:** Prevents conflicts when the PR is merged and ensures the branch is based on the latest state. Skipping this has caused commits to not land properly.

**How to apply:** Run `git fetch origin main && git checkout main && git pull origin main` before `git checkout -b <branch>`, and again before `gh pr create`.
