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
| MudBlazor.Translations | Multi-language support |
| Tailwind-style responsive | Breakpoint-aware layout |
| Docker | Multi-arch builds (amd64/arm64) |
| Azure App Service | Production hosting |

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
| EXTRA_OPTIMIZE | false | Remove symbols, disable debugger |
| BUILD_CONFIGURATION | Debug | Debug or Release |

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

---

## Key Patterns

- **Dialog CRUD pattern** — `DialogService.ShowAsync<T>()` returns `DialogResult` with data
- **State persistence** — Dark mode and drawer state saved to `localStorage`
- **Responsive breakpoints** — `IBrowserViewportService` + `IBrowserViewportObserver`
- **Virtualized data grid** — `MudDataGrid` with 69K+ rows, filtering, sorting
- **Context menu** — Right-click row → clipboard copy

---

## Project Structure

```
blazor-mudblazor-starter/
├── src/WebClient/
│   ├── Program.cs                   # Entry point, service registration
│   ├── WebClient.csproj             # .NET 9, build optimization flags
│   ├── Dockerfile                   # Multi-stage build
│   └── Components/
│       ├── App.razor                # Root HTML document
│       ├── Routes.razor             # Router config
│       ├── Layout/
│       │   ├── MainLayout.razor     # App shell (bar, drawer, dark mode)
│       │   └── Breadcrumb.razor     # Navigation breadcrumbs
│       ├── Pages/
│       │   ├── Home.razor           # / — Landing page
│       │   ├── Counter.razor        # /counter — Click counter
│       │   └── Weather.razor        # /weather — Data grid CRUD demo
│       └── Weather/
│           ├── AddWeather.razor     # Add dialog
│           ├── EditWeather.razor    # Edit dialog
│           └── RemoveWeather.razor  # Delete confirmation
├── global.json                      # .NET SDK 9.0.202
├── WebClient.sln                    # Solution file
└── .github/workflows/               # CI/CD
```

---

## CI/CD

- **PR:** dotnet build (Debug) + Docker build + health check (/healthz, 20 retries)
- **Main:** Build (Release, TRIM=true, EXTRA_OPTIMIZE=true) + Multi-platform Docker push to GHCR + Azure deploy
- **Image:** `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest`
- **Deploy:** Azure Web App (Brazil South)
