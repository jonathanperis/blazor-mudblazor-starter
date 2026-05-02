# blazor-mudblazor-starter

Blazor Server starter template with MudBlazor Material Design components. Built on .NET 9 with Docker multi-arch support and a CI/CD pipeline for GitHub Container Registry and Azure Web App deployment.

**[Live demoL(https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/.md)**

---

## Quick Links

| Page | Description |
|---|---|
| [Getting Started](getting_started) | Prerequisites, local run, Docker run |
| [Project Structure](project_structure) | Directory layout and file descriptions |
| [Components](components) | Blazor components reference |
| [Configuration](configuration) | App settings, build flags, environment variables |
| [Deployment](deployment) | Docker, CI/CD, and Azure deployment |

## Key Features

- Pre-configured MudBlazor layout with app bar, navigation drawer, breadcrumbs, and dark mode toggle
- Demo pages: Home, Counter, Weather (virtualized data grid with Add/Edit/Remove dialogs)
- Multi-architecture Docker image (AMD64 + ARM64) with health check endpoint at `/healthz`
- Production-optimized builds with AOT, ReadyToRun, and trimming support
- CI/CD pipeline: PR build checks with container health verification, main branch release to GHCR and Azure
- Responsive design with breakpoint-aware UI adaptation
- Right-click context menu with clipboard copy for data grid rows
