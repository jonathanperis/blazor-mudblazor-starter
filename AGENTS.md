# Blazor learning sandbox — agent instructions

A local-first school/playground project for Blazor Server and MudBlazor. Optimize for correct, explainable, resettable experiments. Read `PRODUCT.md` and `DESIGN.md` before changing the learning experience.

## Stack and layout

- Supported .NET 10 SDK pinned in `global.json`; package versions in `src/WebClient/WebClient.csproj`.
- One Blazor host, MudBlazor, translated component strings, EF Core SQLite, optional Application Insights.
- `Components/Pages/Labs/`: focused learning pages; `/counter` and `/weather` remain foundational routes.
- `Components/Learning/`: shared frame and callback example.
- `Features/`: models, API client/endpoints, notebook, identity, localization and learning support.
- `tests/WebClient.Tests/`: xUnit/bUnit, in-process HTTP, and real SQLite migration tests.
- `docs/`: Astro 7/Sätteri static guide; `infra/`: historical Azure reference templates, compiled only.

## Commands

```sh
dotnet restore --locked-mode
dotnet test -c Release --no-restore
dotnet publish src/WebClient -c Release -o artifacts/publish
dotnet run --project src/WebClient
dotnet tool restore
dotnet ef migrations list --project src/WebClient
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
```

In `docs/`, run `bun install --frozen-lockfile`, `npm run check:drift`, `npm run build`, `npm run check:rendered`, and `bun audit`. Use Node.js 22.12+ for the Astro CLI. HTTP smoke: `python3 scripts/smoke-http.py --base-url http://127.0.0.1:5000`.

## Contracts to preserve

- Edit dialogs work on a copy; only validated confirmation commits changes.
- API/CSV inputs are bounded. Imports return an entire valid dataset or fail without partial mutation.
- Notebook queries/writes include the current workspace and use optimistic concurrency. Create a DbContext per operation.
- Workspace cookies and protection keys are separate from demo persona cookies.
- Demo sign-in defaults to Development only. The instructor policy is enforced at the endpoint and login/culture posts require antiforgery validation.
- Theme and drawer writes are awaited. Screen size is derived, not persisted. Render usable content before JS interop.
- Reset/disposal cancels owned asynchronous work. Stale results must not overwrite reset state.
- Globalization and diagnostics stay enabled. Only ReadyToRun is exposed as a publishing experiment; Native AOT/trimming are not supported app modes.

## Delivery

PR checks cover tests, published routes/assets, docs, dependencies, workflows, Bicep and containers. Main validates and publishes both architectures with an immutable image digest. Hostinger is the intended application host, but its environment is not configured. The release workflow does not deploy the application.

Keep NuGet/Bun lockfiles current. Renovate uses the shared preset. Regenerate `infra/main.json` after Bicep changes. Do not deploy merely to verify a code change.

## Contribution

Use a feature branch and a pull request when requested; never commit or push without authorization. Conventional commits and rebase-only merging match repository conventions. Preserve unrelated work.

Community policy files are managed at `jonathanperis/.github`; do not duplicate them here. Keep task artifacts and local credentials ignored. The agent memory directory is `.agents/memory/`.
