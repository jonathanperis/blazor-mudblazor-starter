# blazor-mudblazor-starter

> Blazor Server starter template with MudBlazor Material Design components -- .NET 9, Docker, and Azure CI/CD ready

[![CI](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/build-check.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/build-check.yml) [![Release](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/main-release.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/main-release.yml) [![CodeQL](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/codeql.yml/badge.svg)](https://github.com/jonathanperis/blazor-mudblazor-starter/actions/workflows/codeql.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**[Live demo →](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/)** | **[Documentation →](https://jonathanperis.github.io/blazor-mudblazor-starter/)**

---

## About

A ready-to-use starter template for building interactive web applications with Blazor Server and MudBlazor. Comes pre-configured with a Material Design layout, navigation, dark mode toggle, and demo pages that demonstrate data binding, data grids, and CRUD dialogs. The project includes a multi-stage Dockerfile for AMD64 and ARM64 architectures, and a CI/CD pipeline that builds, pushes to GitHub Container Registry, and deploys to Azure Web App.

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET | 9.0 (SDK 9.0.202) | Runtime and SDK |
| Blazor Server | - | Interactive server-side rendering |
| MudBlazor | 9.2.0 | Material Design UI components |
| MudBlazor.Translations | 3.3.0 | Localization support |
| Docker | Multi-stage | AMD64 + ARM64 container builds |
| GitHub Actions | - | CI/CD to GHCR + Azure Web App |

## Features

- Pre-configured MudBlazor layout with app bar, navigation drawer, breadcrumbs, and dark mode toggle
- Demo pages: Home (landing), Counter (interactive counter), Weather (virtualized data grid with Add/Edit/Remove dialogs)
- Multi-architecture Docker image (AMD64 + ARM64) with health check endpoint
- Production-optimized builds with AOT compilation, ReadyToRun, and trimming support
- CI/CD pipeline: PR build checks with container health verification, main branch release to GHCR and Azure Web App
- Responsive design with breakpoint-aware UI (switch vs toggle for dark mode)
- Clipboard copy support for data grid rows via right-click context menu

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (optional)

### Quick Start

```bash
git clone https://github.com/jonathanperis/blazor-mudblazor-starter.git
cd blazor-mudblazor-starter
dotnet restore
dotnet run --project src/WebClient
```

Open `http://localhost:5000` in your browser.

### Run with Docker

```bash
docker build -t blazor-mudblazor -f src/WebClient/Dockerfile src/
docker run -p 5000:5000 blazor-mudblazor
```

## Project Structure

```
blazor-mudblazor-starter/
├── src/WebClient/
│   ├── Program.cs                  # App entry point, MudBlazor service registration
│   ├── WebClient.csproj            # .NET 9, MudBlazor 9.2.0, AOT/Trim build flags
│   ├── Dockerfile                  # Multi-stage build (AMD64 + ARM64)
│   ├── appsettings.json            # Base configuration
│   ├── appsettings.Development.json
│   ├── Properties/launchSettings.json
│   ├── wwwroot/                    # Static assets
│   └── Components/
│       ├── App.razor               # Root HTML document, MudBlazor CSS/JS imports
│       ├── Routes.razor            # Router setup with MainLayout default
│       ├── _Imports.razor          # Global using directives
│       ├── Layout/
│       │   ├── MainLayout.razor    # MudBlazor layout shell (app bar, drawer, dark mode)
│       │   └── Breadcrumb.razor    # Reusable breadcrumb navigation component
│       ├── Pages/
│       │   ├── Home.razor          # Landing page
│       │   ├── Counter.razor       # Interactive counter demo
│       │   ├── Weather.razor       # Data grid with CRUD operations
│       │   └── Error.razor         # Error page with request ID
│       └── Weather/
│           ├── AddWeather.razor    # Dialog for adding weather entries
│           ├── EditWeather.razor   # Dialog for editing weather entries
│           └── RemoveWeather.razor # Delete confirmation dialog
├── .github/workflows/
│   ├── build-check.yml             # PR validation: build + Docker + health check
│   └── main-release.yml            # Release: build + GHCR push + Azure deploy
├── WebClient.sln
├── global.json                     # .NET SDK 9.0.202
└── LICENSE
```

## CI/CD

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `build-check.yml` | Pull requests | .NET build + Docker health check (`/healthz`) |
| `main-release.yml` | Push to main | Release build + multi-arch GHCR push + Azure deploy |
| `codeql.yml` | Push/PR + weekly | C# security and quality analysis |
| `deploy-docs.yml` | Push/wiki edits | Wiki → HTML docs generation to GitHub Pages |

**Container image:** `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest` (amd64 + arm64)

**Deployment:** Azure Web App (Brazil South) via publish profile

## Dependency Management

Automated weekly updates via [Dependabot](https://docs.github.com/en/code-security/dependabot) for NuGet packages, Docker base images, and GitHub Actions versions.

## License

MIT -- see [LICENSE](LICENSE)
