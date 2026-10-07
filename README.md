# Blazor learning sandbox

A Swiss-army-knife learning project for **Blazor and MudBlazor**. Explore working examples, read their source, change one thing, and observe the result. The same labs run in two hosts: **Blazor Server** locally, and a static **Blazor WebAssembly** live demo on GitHub Pages.

[![Build Check](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/build-check.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/build-check.yml)
[![CodeQL](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/codeql.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/codeql.yml)

**[Live demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/)** · **[Learning guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/)** · Run the full labs locally using the commands below.

The live demo runs entirely in your browser. Labs that need a server (HTTP API, SQLite notebook, sign-in, culture cookie, diagnostics) use labeled browser stand-ins there; run the server app to see the real boundaries.

## Run locally

Install **.NET 10 SDK 10.0.401** (or a later patch in that SDK feature band). The SDK includes the current .NET runtime. Cloud credentials are optional.

```sh
git clone https://github.com/jonathanperis/blazor-mudblazor-starter.git
cd blazor-mudblazor-starter
dotnet restore --locked-mode
dotnet run --project src/WebClient
```

Open **http://localhost:5000/labs**. For local TLS, run `dotnet run --project src/WebClient --launch-profile https`. To run the WebAssembly host behind the live demo, use `dotnet run --project src/WebClient.Wasm` and open **http://localhost:5100/**.

## Choose a lab

| Route | Lesson |
|---|---|
| `/counter` | Parameters, event callbacks, component state, and circuit lifetime |
| `/labs/forms` | Data annotations, field feedback, draft editing, cancel versus confirm |
| `/weather` | Typed DataGrid filters, sorting, CRUD, selection, virtualization, deterministic datasets |
| `/labs/api` | Typed HTTP client, server paging, latency, errors, empty states, cancellation |
| `/labs/persistence` | SQLite, EF Core migrations, workspace isolation, optimistic concurrency |
| `/labs/auth` | Demo personas, cookie authentication, antiforgery, server-enforced policies |
| `/labs/localization` | English/Portuguese, culture formatting, themes, keyboard and live-region practice |
| `/labs/files` | Bounded CSV import/export and cancellable server work with progress |
| `/labs/observability` | Structured logs, trace IDs, liveness/readiness, Docker and hosting considerations |

Each lab includes an objective, prerequisites, an approximate duration, a source link, a common mistake, and an exercise. Reset controls make experiments repeatable.

### Understand the data lifetime

- **Counter:** component state resets on navigation; scoped state survives navigation but resets with a new circuit (server) or a reload (WebAssembly).
- **Grid:** private in-memory data resets on navigation/reload. Choose 100–69,420 records and a random seed. Virtualization limits rendering, not dataset allocation.
- **API:** deterministic, read-only server dataset with bounded pages. The server host sends actual HTTP requests; the live demo answers the same typed client with an in-browser message handler.
- **Notebook:** SQLite data and data-protection keys live in ignored `src/WebClient/App_Data/`. A protected, essential workspace cookie scopes notes to this browser; it renews on each visit. A workspace holds at most 50 notes. Demo personas are separate from workspace ownership. The live demo keeps notes in localStorage with the same version-conflict rules.
- **CSV/work:** imports replace data only after complete validation, and export/import round trips are lossless. Processing stops when the page is disposed; it is not a durable queue.
- **Authentication:** fixed demonstration personas are enabled by default only in Development. Use an identity provider for real accounts. In the live demo, personas only change the UI; only a server can enforce a policy.

## Stack

| Package / tool | Version |
|---|---|
| SDK | 10.0.401 |
| Microsoft.ApplicationInsights.AspNetCore | 3.1.2 |
| Microsoft.AspNetCore.Components.Authorization | 10.0.12 |
| Microsoft.AspNetCore.Components.Web | 10.0.12 |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.12 |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.12 |
| Microsoft.Extensions.Localization | 10.0.12 |
| MudBlazor | 9.11.0 |
| MudBlazor.Translations | 3.6.0 |

The docs use Astro 7, Sätteri, and TypeScript 7, with a committed Bun lockfile; Playwright drives the demo browser check. NuGet lockfiles cover every project. Renovate maintains dependency updates.

## Verify and experiment

```sh
dotnet test -c Release --no-restore
dotnet publish src/WebClient -c Release --no-restore -o artifacts/publish
dotnet publish src/WebClient.Wasm -c Release --no-restore -o artifacts/wasm
dotnet tool restore
dotnet ef migrations list --project src/WebClient
```

For documentation, use the Node version in [`docs/.node-version`](docs/.node-version), Bun from the `packageManager` field in [`docs/package.json`](docs/package.json), and Python 3 for the verification scripts:

```sh
cd docs
bun install --frozen-lockfile
npm run check:drift
npm run check:types
npm run build
python3 ../scripts/prepare-pages-demo.py
npm run check:rendered
npm run check:demo
bun audit
```

See the [testing guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/testing/) for test boundaries, HTTP smoke checks, and measurement exercises. The [docs maintainer README](docs/README.md) explains authoring, local URLs, and the static-site structure.

## Docker

```sh
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
docker run --rm -p 5000:5000 -v learning-data:/app/App_Data blazor-learning
```

The container runs as the non-root `app` user and listens on port 5000. The named volume preserves SQLite and workspace-protection keys. Omit it for a disposable environment. Demo sign-in is disabled in the default container environment.

The supported publishing experiment is `--build-arg READY_TO_RUN=true`. `BUILD_CONFIGURATION` defaults to `Release`. Native AOT and trimming are not supported modes for this Blazor Server sample; globalization and diagnostics stay enabled.

## Delivery

- **PRs:** behavioral tests, locked restore with vulnerability checks, published-app HTTP checks, the Pages site build (guide plus WebAssembly demo) with drift/link/rendered checks and a browser check of the demo, dependency review, Bicep compilation with a stale-template check, workflow linting, and container checks/scanning.
- **Main:** validates again, publishes a multi-architecture GHCR image under `sha-<commit>`, runs and scans that exact digest natively on amd64 and arm64, then points `latest` at it with signed build provenance. Use the digest in the run summary to identify an exact build.
- **GitHub Pages:** the release workflow deploys the guide and the live demo that the validation run built and checked.
- **Application hosting:** Hostinger is the intended target for the server app, but this project's environment is not configured. The release workflow does not deploy the server app.

Follow the [hosting guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/deployment/) for the current delivery boundary and the decisions required before configuring Hostinger.

## Structure

`src/WebClient.Shared/` contains the shell, the lab pages and browser-safe feature code shared by both hosts. `src/WebClient/` is the Blazor Server host: endpoints, SQLite notebook, cookie identity and server panels. `src/WebClient.Wasm/` is the static WebAssembly host for the live demo. `tests/WebClient.Tests/` contains component, integration and static-demo tests. `docs/wiki/` is the learning guide; `infra/` retains the previous Azure templates as reference material.

## License

MIT — see [LICENSE](LICENSE).
