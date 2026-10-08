# Blazor learning sandbox — agent instructions

A local-first school/playground project for Blazor and MudBlazor. The labs run in a Blazor Server host and in a static WebAssembly live demo on GitHub Pages. Optimize for correct, explainable, resettable experiments. Read `PRODUCT.md` and `DESIGN.md` before changing the learning experience.

## Stack and layout

- Supported .NET 10 SDK pinned in `global.json`; package versions in the `src/*/*.csproj` files; shared build settings in `src/Directory.Build.props` (inside the container build context).
- MudBlazor, translated component strings, EF Core SQLite (server only), optional Application Insights.
- `src/WebClient.Shared/`: Razor class library with the shell, every lab page (`Components/Pages/Labs/`; `/counter` and `/weather` remain foundational routes), the lab frame, browser-safe features, and the static-demo stand-ins. It must run in a browser: no `HttpContext`, EF Core, cookies or antiforgery.
- `src/WebClient.Shared/Components/Gallery/<Group>/`: component gallery pages (`[ComponentPage]`, one file per example, embedded sources); `Components/Samples/<Name>/`: page samples (`[PageSample]`). Catalogs are discovered by reflection; follow `docs/wiki/gallery.md` and `DESIGN.md`.
- `src/WebClient/`: Blazor Server host: endpoints, SQLite notebook, workspace cookie, demo identity, server panels.
- `src/WebClient.Wasm/`: static WebAssembly host for the GitHub Pages demo at `/demo/`, with a runtime-caching service worker. Its host profile and browser services are `StaticDemoHost` in the shared library.
- `src/WebClient.Prerender/`: build-time tool that renders every demo route to static HTML (`labs/api.html`) before the Pages artifact is assembled.
- `tests/WebClient.Tests/`: xUnit v3 (VSTest via `xunit.v3.mtp-off`)/bUnit, in-process HTTP, real SQLite migration, and static-demo tests.
- `docs/`: Astro 7/Sätteri static guide, site-wide 404 page and demo browser check; `infra/`: historical Azure reference templates, compiled only.

## Commands

```sh
dotnet restore --locked-mode
dotnet test -c Release --no-restore
dotnet publish src/WebClient -c Release -o artifacts/publish
dotnet publish src/WebClient.Wasm -c Release -o artifacts/wasm
dotnet run --project src/WebClient.Prerender -c Release -- artifacts/wasm/wwwroot
dotnet run --project src/WebClient
dotnet run --project src/WebClient.Wasm
dotnet tool restore
dotnet ef migrations list --project src/WebClient
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
```

In `docs/`, run `bun install --frozen-lockfile`, `npm run check:drift`, `npm run check:types`, `npm run build`, `python3 ../scripts/prepare-pages-demo.py` (after publishing and prerendering the WebAssembly host), `npm run check:rendered`, `npm run check:demo`, and `bun audit`. Select Node from `docs/.node-version`, Bun from `docs/package.json`'s `packageManager`, and Python 3 for the verification scripts. HTTP smoke: `python3 scripts/smoke-http.py --base-url http://127.0.0.1:5000`.

## Contracts to preserve

- Shared labs use base-relative links (`labs`, not `/labs`) so they work at `/` and under `/blazor-mudblazor-starter/demo/`. Server-only behavior goes behind a contract (`INotebookStore`, `ForecastApiClient`'s handler, `LearningHost` panels) with a labeled browser stand-in; explain differences in `StaticDemoHost`'s `LabNotes`.
- Edit dialogs work on a copy; only validated confirmation commits changes.
- API/CSV/notebook inputs are bounded (50 notes per workspace). Imports return an entire valid dataset or fail without partial mutation; CSV export/import round trips are lossless.
- Notebook queries/writes include the current workspace and use optimistic concurrency. Create a DbContext per operation.
- Workspace cookies and protection keys are separate from demo persona cookies. The workspace cookie is issued for page renders only and slides on each visit.
- Demo sign-in defaults to Development only. The instructor policy is enforced at the endpoint and login/culture posts require antiforgery validation. Browser personas in the static demo are UI-only and must say so.
- Theme and drawer-button writes are awaited; a self-closing responsive drawer is not persisted. Screen size is derived. Render usable content before JS interop, and degrade when storage or interop fails.
- Reset/disposal cancels owned asynchronous work. Stale results must not overwrite reset state.
- Globalization and diagnostics stay enabled. Only ReadyToRun is exposed as a server publishing experiment; Native AOT/trimming are not supported server modes. The WebAssembly host uses the SDK's default WebAssembly publishing with full ICU data.
- Every static-demo route is prerendered at build time, and a page that hits an error boundary fails the build. Pages must render without the browser: JavaScript runs after rendering, and browser-only data (the notebook) shows its loading state until the app starts.

## Delivery

PR checks cover tests, published routes/assets, the Pages site (guide plus WebAssembly demo, including a browser check), dependencies, workflows, Bicep (including stale `main.json`) and containers. Main validates, publishes both architectures under `sha-<commit>`, smoke-tests and scans that digest natively per architecture, then promotes `latest` with build provenance, and deploys the validated Pages artifact. Hostinger is the intended server host, but its environment is not configured. The release workflow does not deploy the server app.

Keep NuGet/Bun lockfiles current; update `global.json`, the Dockerfile SDK and the NuGet lockfiles together. Renovate uses the shared preset. Regenerate `infra/main.json` after Bicep changes. Do not deploy merely to verify a code change.

## Contribution

Use a feature branch and a pull request when requested; never commit or push without authorization. Conventional commits and rebase-only merging match repository conventions. Preserve unrelated work.

Community policy files are managed at `jonathanperis/.github`; do not duplicate them here. Keep task artifacts and local credentials ignored. The agent memory directory is `.agents/memory/`.
