# Blazor MudBlazor Starter — Claude Code Guide

Production-ready Blazor Server starter template with MudBlazor Material Design components, .NET 9, Docker, and Azure deployment.

**Live demo:** https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/

---

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| .NET 9 (SDK 9.0.202) | Runtime |
| Blazor Server | Interactive server-side rendering |
| MudBlazor 9.2.0 | Material Design UI components |
| MudBlazor.Translations 3.3.0 | Multi-language support |
| Docker | Multi-arch builds (amd64/arm64) |
| Azure App Service | Production hosting (Brazil South) |
| GitHub Actions | CI/CD (build, test, deploy) |
| GitHub Pages | Documentation site |

---

## Build Commands

```sh
dotnet restore                              # Restore dependencies
dotnet run --project src/WebClient          # Dev server (http://localhost:5000)
dotnet build src/WebClient -c Release       # Production build
docker build --build-arg TRIM=true --build-arg EXTRA_OPTIMIZE=true \
  -f src/WebClient/Dockerfile src/          # Docker build
```

### Build Arguments

| Arg | Default | Purpose |
|-----|---------|---------|
| AOT | false | Ahead-of-Time compilation |
| TRIM | false | ReadyToRun + SingleFile + SelfContained |
| EXTRA_OPTIMIZE | false | Remove symbols, disable debugger, invariant globalization |
| BUILD_CONFIGURATION | Debug | Debug or Release |

### Build Optimization Tiers

| Tier | AOT | TRIM | EXTRA_OPTIMIZE | Use Case |
|------|-----|------|----------------|----------|
| Debug | false | false | false | Local development |
| Release | false | true | true | Production deployment |

---

## Architecture

Blazor Server with real-time WebSocket communication:

```
Browser ↔ (WebSocket) ↔ ASP.NET Core Blazor Server
                        ├── Razor Components (pages, layouts, dialogs)
                        ├── MudBlazor Services (dialogs, snackbars, viewport)
                        ├── JavaScript Interop (localStorage persistence)
                        └── Health Checks (/healthz)
```

### Key Patterns

- **Dialog CRUD pattern** — `DialogService.ShowAsync<T>()` returns `DialogResult` with data
- **State persistence** — Dark mode and drawer state saved to `localStorage` via JS interop
- **Responsive breakpoints** — `IBrowserViewportService` + `IBrowserViewportObserver`
- **Virtualized data grid** — `MudDataGrid` with 69K+ rows, filtering, sorting, paging
- **Context menu** — Right-click row → clipboard copy via `IJSRuntime`
- **Breadcrumb navigation** — Reusable `Breadcrumb.razor` component with `BreadcrumbItem` list

### State Persistence Flow

```
OnAfterRenderAsync(firstRender: true) → Read from localStorage
Property setter → Write to localStorage
```

Persisted keys: `isDarkMode`, `drawerOpen`, `isSmallScreen`

---

## Project Structure

```
blazor-mudblazor-starter/
├── src/WebClient/
│   ├── Program.cs                   # Entry point, service registration
│   ├── WebClient.csproj             # .NET 9, build optimization flags
│   ├── Dockerfile                   # Multi-stage build (base → build → publish → final)
│   ├── appsettings.json             # Base configuration
│   ├── Properties/launchSettings.json
│   └── Components/
│       ├── App.razor                # Root HTML document (MudBlazor CSS/JS)
│       ├── Routes.razor             # Router config (MainLayout default)
│       ├── _Imports.razor           # Global using directives
│       ├── Layout/
│       │   ├── MainLayout.razor     # App shell (bar, drawer, dark mode, viewport observer)
│       │   └── Breadcrumb.razor     # Navigation breadcrumbs
│       ├── Pages/
│       │   ├── Home.razor           # / — Landing page
│       │   ├── Counter.razor        # /counter — Click counter
│       │   ├── Weather.razor        # /weather — Data grid CRUD demo (69K+ rows)
│       │   └── Error.razor          # /Error — Error page with request ID
│       └── Weather/
│           ├── AddWeather.razor     # Add dialog (form + validation)
│           ├── EditWeather.razor    # Edit dialog (pre-filled form)
│           └── RemoveWeather.razor  # Delete confirmation dialog
├── .github/
│   ├── workflows/
│   │   ├── build-check.yml         # PR: dotnet build + Docker health check
│   │   ├── main-release.yml        # Main: Release build + GHCR push + Azure deploy
│   │   ├── codeql.yml              # Security analysis (C#, weekly + push/PR)
│   │   └── deploy-docs.yml         # Wiki → HTML docs generation + GitHub Pages
│   ├── codeql/codeql-config.yml    # CodeQL exclusions (obj/, bin/, generated code)
│   ├── dependabot.yml              # Weekly updates: NuGet, Docker, Actions
│   ├── docs-template.html          # Docs site template (dark theme, responsive)
│   └── generate-docs.js            # Node.js wiki-to-HTML converter
├── .claude/memory/                  # In-repo Claude memory (architecture reference)
├── .editorconfig                    # UTF-8, LF, 4-space indent (.cs/.razor)
├── global.json                      # .NET SDK 9.0.202 pinned
├── WebClient.sln                    # Solution file
└── LICENSE                          # MIT
```

---

## CI/CD Workflows

### Pull Requests (`build-check.yml`)
1. Restore + build (.NET Debug)
2. Build Docker image
3. Run container, health check `/healthz` (20 retries, 5s interval)

### Main Branch (`main-release.yml`)
1. Restore + build (.NET Release, TRIM=true, EXTRA_OPTIMIZE=true)
2. Multi-platform Docker build + push to GHCR (amd64 + arm64)
3. Deploy to Azure Web App (Brazil South)

### Security (`codeql.yml`)
- C# analysis with `security-and-quality` queries
- Triggers: push/PR to main + weekly schedule

### Documentation (`deploy-docs.yml`)
- Converts wiki Markdown → single-page HTML docs site
- Triggers: push to main, wiki edits (Gollum), manual dispatch
- Deploys to GitHub Pages via `actions/deploy-pages` (GitHub Actions deployment model)

### Dependency Updates (`dependabot.yml`)
- Weekly: NuGet packages, Docker base images, GitHub Actions versions

---

## Contribution Workflow

- **All changes** go through a feature branch + pull request
- **Merge strategy:** Rebase only (squash and merge commits disabled)
- **Auto merge:** Enabled
- **Delete branch on merge:** Enabled
- **Web commit signoff:** Required
- **Branch naming:** `feat/`, `fix/`, `docs/`, `chore/`, `ci/` prefixes
- **Commit style:** Conventional commits (see git log for examples)

### Branch Protection (main)
- Required linear history
- Force pushes blocked
- Branch deletions blocked

### Community Health Files

CODE_OF_CONDUCT, CONTRIBUTING, SECURITY, SUPPORT, and LICENSE are managed at the org level in the [jonathanperis/.github](https://github.com/jonathanperis/.github) repository. Do **not** create these files in this repo.

---

## Docker

```sh
# Build
docker build --build-arg TRIM=true --build-arg EXTRA_OPTIMIZE=true \
  -f src/WebClient/Dockerfile src/

# Run
docker run -p 5000:5000 blazor-mudblazor-starter
```

- **Base image:** `mcr.microsoft.com/dotnet/aspnet:9.0`
- **SDK image:** `mcr.microsoft.com/dotnet/sdk:9.0`
- **Non-root user:** `app`
- **Port:** 5000 (`ASPNETCORE_URLS=http://+:5000`)
- **Health check:** `/healthz`
- **Multi-stage:** base → build (with clang/zlib for AOT) → publish → final

### Container Image

- **Registry:** `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest`
- **Platforms:** linux/amd64, linux/arm64/v8
