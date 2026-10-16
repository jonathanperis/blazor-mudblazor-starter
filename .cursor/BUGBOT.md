# Bugbot review rules

Report real bugs, security issues and regressions. Skip style nitpicks
and preference-only suggestions.

## Generated and vendored files

Do not comment on these paths; they are generated, vendored or lockfiles:

- `**/dist/**`
- `**/build/**`
- `**/out/**`
- `**/*.min.js`
- `**/package-lock.json`
- `**/bun.lock`
- `**/bun.lockb`
- `**/packages.lock.json`
- `**/Migrations/*.Designer.cs`
- `infra/main.json`

## Path-specific rules

### `**/*.cs`

Follow .NET coding conventions. Flag missing async/await, unobserved
tasks, CancellationTokenSource or IDisposable leaks, and nullable
annotation gaps. Do not suggest ConfigureAwait(false) in Blazor
components or their services: they must resume on the renderer's
synchronization context.

### `**/*.razor`

Read AGENTS.md contracts. Flag async work not cancelled on reset or
disposal, stale results that can overwrite newer state, missing
StateHasChanged/InvokeAsync after background updates, JS interop
failures that can break the shell, absolute "/" links in shared labs
(they break under the GitHub Pages base path), server-only APIs in
src/WebClient.Shared, and accessibility gaps: labels, accessible
names, focus, and live-region noise.

### `**/*.{astro,ts,mjs}`

Flag links that ignore the site base path, inline scripts that break
without JavaScript, duplicate IDs, and missing accessible names.

### `**/*.py`

Flag bare except clauses, mutable default arguments, and checks that
can pass without verifying anything (silent skips, broad regexes).

### `.github/workflows/**/*.yml`

Flag any user-controlled input interpolated directly in
run: blocks (workflow injection risk). Recommend env: + quoting.
Require full-SHA action pins, least-privilege permissions per job,
and timeouts.

### `infra/**/*.bicep`

Historical reference templates. Require infra/main.json to be
regenerated with the pinned Bicep version in the same change.
