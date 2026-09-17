# Blazor learning sandbox

A Swiss-army-knife learning project for **Blazor Server and MudBlazor**. Explore working examples, read their source, change one thing, and observe the result.

[![Build Check](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/build-check.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/build-check.yml)
[![CodeQL](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/codeql.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/codeql.yml)

**[Live labs](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/labs)** · **[Learning guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/)**

## Run locally

Install **.NET 10 SDK 10.0.401** (or a later patch in that SDK feature band). The SDK includes the current .NET runtime. Cloud credentials are optional.

```sh
git clone https://github.com/jonathanperis/blazor-mudblazor-starter.git
cd blazor-mudblazor-starter
dotnet restore --locked-mode
dotnet run --project src/WebClient
```

Open **http://localhost:5000/labs**. For local TLS, run `dotnet run --project src/WebClient --launch-profile https`.

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
| `/labs/observability` | Structured logs, trace IDs, liveness/readiness, optional Docker/Azure deployment |

Each lab includes an objective, prerequisites, an approximate duration, a source link, a common mistake, and an exercise. Reset controls make experiments repeatable.

### Understand the data lifetime

- **Counter:** component state resets on navigation; circuit-scoped state survives navigation but resets with a new circuit.
- **Grid:** private in-memory data resets on navigation/reload. Choose 100–69,420 records and a random seed. Virtualization limits rendering, not dataset allocation.
- **API:** deterministic, read-only server dataset with bounded pages. The UI demonstrates actual HTTP requests.
- **Notebook:** SQLite data and data-protection keys live in ignored `src/WebClient/App_Data/`. A protected, essential workspace cookie scopes notes to this browser. Demo personas are separate from workspace ownership.
- **CSV/work:** imports replace data only after complete validation. Processing stops when the page is disposed; it is not a durable queue.
- **Authentication:** fixed demonstration personas are enabled by default only in Development. Azure explicitly disables them. Use an identity provider for real accounts.

## Stack

| Package / tool | Version |
|---|---|
| SDK | 10.0.401 |
| Microsoft.ApplicationInsights.AspNetCore | 3.1.2 |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.12 |
| MudBlazor | 9.10.0 |
| MudBlazor.Translations | 3.6.0 |

The docs use Astro 7 and Sätteri, with a committed Bun lockfile. NuGet lockfiles cover application and test dependencies. Renovate maintains dependency updates.

## Verify and experiment

```sh
dotnet test -c Release
dotnet publish src/WebClient -c Release -o artifacts/publish
dotnet tool restore
dotnet ef migrations list --project src/WebClient
```

For documentation, use Node.js 22.12+ and Bun:

```sh
cd docs
bun install --frozen-lockfile
npm run check:drift
npm run build
npm run check:rendered
bun audit
```

See the [testing guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/testing/) for test boundaries, HTTP smoke checks, and measurement exercises.

## Docker

```sh
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
docker run --rm -p 5000:5000 -v learning-data:/app/App_Data blazor-learning
```

The container runs as `app` and listens on port 5000. The named volume preserves SQLite and workspace-protection keys. Omit it for a disposable environment. Demo sign-in is disabled in the default container environment.

The supported publishing experiment is `--build-arg READY_TO_RUN=true`. `BUILD_CONFIGURATION` defaults to `Release`. Native AOT and trimming are not supported modes for this Blazor Server sample; globalization and diagnostics stay enabled.

## Delivery

- **PRs:** behavioral tests, locked restore with vulnerability checks, published-app HTTP checks, docs build/link/drift checks, dependency review, Bicep compilation, workflow linting, and container checks/scanning.
- **Main:** validates again and publishes a multi-architecture GHCR image with a commit tag and a manifest digest. `latest` is a convenience tag; Azure deployment uses the digest.
- **Azure:** optional, enabled with `AZURE_DEPLOY_ENABLED=true`. Uses OIDC and the `azure-sandbox` GitHub environment. No publish profile is needed.
- **GitHub Pages:** uses the shared `pages-docs-deploy.yml@main` workflow.

Follow the [deployment lab](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/deployment/) for resource names, permissions, configuration, persistence limitations, and cleanup.

## Structure

`src/WebClient/Components/` contains the shell and lab pages. `Features/` contains forecast, notebook, identity, localization, and learning support code. `tests/WebClient.Tests/` contains component and integration tests. `docs/wiki/` is the learning guide; `infra/` is the optional Azure deployment.

## License

MIT — see [LICENSE](LICENSE).
