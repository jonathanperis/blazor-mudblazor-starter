# Project Structure

```
blazor-mudblazor-starter/
├── .github/
│   ├── dependabot.yml                  # Dependabot configuration for dependency updates
│   └── workflows/
│       ├── build-check.yml             # PR validation: .NET build + Docker build + health check
│       ├── codeql.yml                  # CodeQL security analysis
│       ├── deploy.yml                  # GitHub Pages deployment
│       └── main-release.yml            # Release: optimized build + GHCR push + Azure deploy
├── src/
│   └── WebClient/
│       ├── Components/
│       │   ├── App.razor               # Root HTML document with MudBlazor CSS/JS imports
│       │   ├── Routes.razor            # Blazor router, defaults to MainLayout
│       │   ├── _Imports.razor          # Global using directives for all components
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor    # App shell: app bar, drawer, dark mode, responsive breakpoints
│       │   │   └── Breadcrumb.razor    # Reusable breadcrumb navigation component
│       │   ├── Pages/
│       │   │   ├── Home.razor          # Landing page (route: /)
│       │   │   ├── Counter.razor       # Interactive counter demo (route: /counter)
│       │   │   ├── Weather.razor       # Virtualized data grid with CRUD (route: /weather)
│       │   │   └── Error.razor         # Error page with request ID display (route: /Error)
│       │   └── Weather/
│       │       ├── AddWeather.razor    # MudDialog for adding weather entries
│       │       ├── EditWeather.razor   # MudDialog for editing weather entries
│       │       └── RemoveWeather.razor # MudDialog for delete confirmation
│       ├── Properties/
│       │   └── launchSettings.json     # Local dev profiles (HTTP on 5000, HTTPS on 5001)
│       ├── wwwroot/                    # Static assets (favicon, CSS)
│       ├── appsettings.json            # Base configuration (logging, allowed hosts)
│       ├── appsettings.Development.json # Development logging overrides
│       ├── Dockerfile                  # Multi-stage .NET 9 build (AMD64 + ARM64)
│       ├── Program.cs                  # App entry point, MudBlazor service registration
│       └── WebClient.csproj            # Project file: .NET 9, MudBlazor 9.2.0, AOT/Trim flags
├── .editorconfig                       # Code style settings
├── .gitignore                          # Git ignore rules
├── global.json                         # .NET SDK version pin (9.0.202, roll-forward: minor)
├── LICENSE                             # MIT license
├── README.md                          # Project overview and quick start
└── WebClient.sln                       # Solution file
```

## Key Directories

### `src/WebClient/Components/Layout/`

Contains the application shell. `MainLayout.razor` provides the MudBlazor layout with app bar, navigation drawer, dark mode toggle (persisted via localStorage), and responsive breakpoint handling. `Breadcrumb.razor` is a reusable component that accepts a list of `BreadcrumbItem` parameters.

### `src/WebClient/Components/Pages/`

Contains routable page components. Each page uses the `Breadcrumb` component for navigation context. The `Weather` page demonstrates a full CRUD workflow with `MudDataGrid`, dialog services, snackbar notifications, and clipboard integration.

### `src/WebClient/Components/Weather/`

Contains MudDialog components used by the Weather page for Add, Edit, and Remove operations. Each dialog uses `EditForm` with `DataAnnotationsValidator` for form validation (except RemoveWeather, which is a simple confirmation).

### `.github/workflows/`

Contains four GitHub Actions workflows: `build-check.yml` for PR validation (includes container health check against `/healthz`), `main-release.yml` for production releases to GHCR and Azure, `codeql.yml` for security analysis, and `deploy.yml` for GitHub Pages deployment.
