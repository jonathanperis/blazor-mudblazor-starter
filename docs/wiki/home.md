# Blazor MudBlazor Starter

Production-ready Blazor Server starter template with MudBlazor Material Design components. Built on .NET 9 with Docker multi-arch support, GHCR publishing, Azure App Service deployment, CodeQL, and GitHub Pages documentation.

**[Live demo](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/)** · **[GitHub repository](https://github.com/jonathanperis/blazor-mudblazor-starter)**

---

## Quick Links

| Page | Description |
|---|---|
| [Getting Started](#getting-started) | Prerequisites, local run, Docker run |
| [Project Structure](#project-structure) | Directory layout and file descriptions |
| [Components](#components) | Blazor components reference |
| [Configuration](#configuration) | App settings, build flags, environment variables |
| [Deployment](#deployment) | Docker, CI/CD, and Azure deployment |
| [Documentation Site](#documentation) | Astro Pages authoring, Sätteri, and validation workflow |

## Key Features

- Pre-configured MudBlazor layout with purple app bar, navigation drawer, compact breadcrumbs, project links, and dark mode toggle
- Productized demo pages: Overview, Counter demo, and DataGrid demo with Add/Edit/Remove dialogs
- DataGrid showcase with 69,420 virtualized rows, shortened record IDs, row selection, paging, and right-click clipboard copy
- Multi-architecture Docker image (AMD64 + ARM64) with ASP.NET Core health endpoint at `/healthz`
- Production-optimized builds with optional AOT plus ReadyToRun, trimming, and extra optimization support
- CI/CD pipeline: PR build checks with container health verification, main branch release to GHCR and Azure
- Renovate dependency updates through the shared `github>jonathanperis/.github` preset
- GitHub Pages documentation site with project overview, reference docs, and deployment notes
