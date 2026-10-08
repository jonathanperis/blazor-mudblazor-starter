# Project structure

The labs live in one Razor class library. Two hosts render them: a Blazor Server app with real server boundaries, and a static WebAssembly app published as the live demo.

```text
src/
  Directory.Build.props          shared nullable, lockfile and NuGet audit settings
  WebClient.Shared/              labs reused by both hosts
    Components/
      Layout/                    shell, preferences, skip link, semantic navigation
      Learning/                  lab frame (objective, source, notes, reset) and callback example
      Pages/                     overview, counter, DataGrid, not found
        Labs/                    catalog and focused learning pages
      Weather/                   validated fields and draft dialogs
      StaticDemo/                browser panels for sign-in, culture and diagnostics
    Features/
      Forecasts/                 model, generator, CSV codec, query rules and typed client
      Learning/                  catalog, host description, scoped state, preferences, work
      Notebook/                  notebook contract, drafts, snapshots and errors
      StaticDemo/                in-browser API handler, localStorage notebook, demo personas
      Localization/              English and Portuguese resources
    wwwroot/                     theme-aware CSS and small JS interop helpers
  WebClient/                     Blazor Server host
    Program.cs                   services, middleware, endpoints, startup migrations
    Components/                  document shell, router, error page
      Server/                    server panels: cookie sign-in, culture post, diagnostics
    Features/
      Forecasts/                 HTTP endpoint
      Notebook/                  workspace cookie, EF Core store, health check, migrations
      Identity/                  educational cookie identities and culture endpoint
  WebClient.Wasm/                static WebAssembly host for GitHub Pages
    Program.cs                   browser services and culture startup
    wwwroot/index.html           boot page and deep-link restore
    wwwroot/service-worker.*.js  runtime cache for repeat and offline visits
  WebClient.Prerender/           build-time tool: renders every demo route to static HTML
tests/WebClient.Tests/           bUnit, domain, HTTP, SQLite and static-demo tests
scripts/                         docs drift/rendered checks, Pages demo assembly, HTTP smoke
docs/                            Astro guide, 404 page, browser check of the demo
infra/                           historical Azure Bicep/ARM reference, compiled only
.config/dotnet-tools.json        pinned EF migration CLI
renovate.json                    shared dependency-update preset
```

## Boundaries

Start with [`LabCatalog.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Learning/LabCatalog.cs) for the lesson map, [`LearningHost.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Learning/LearningHost.cs) for what each host provides, [`StaticDemoHost.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/StaticDemo/StaticDemoHost.cs) for the browser stand-ins and lab notes of the static demo, and the two host entry points: [`WebClient/Program.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Program.cs) and [`WebClient.Wasm/Program.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Wasm/Program.cs). [`IntegrationTests.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/tests/WebClient.Tests/IntegrationTests.cs) shows the HTTP and persistence examples. The [documentation-site guide](../documentation/) explains the separate Astro project.

Shared code must run in a browser: no `HttpContext`, EF Core, cookies or antiforgery. A lab that needs a server boundary depends on a contract instead:

| Lab need | Contract | Blazor Server | WebAssembly demo |
|---|---|---|---|
| Forecast API | `ForecastApiClient` (`HttpClient`) | real HTTP endpoint | `ForecastApiSimulator` message handler |
| Notebook | `INotebookStore` | SQLite with EF Core | `BrowserNotebookStore` on localStorage |
| Sign-in, culture, diagnostics | `LearningHost` panel types | cookie and form-post panels | browser panels |

The labs label each stand-in. Policies are enforced only by the server host: browser code can be changed by its user.

## Add a lab

1. Add its route under `WebClient.Shared/Components/Pages/Labs/`, using a base-relative `Href` for links.
2. Add metadata to `LabCatalog` and render its content through `LabFrame`.
3. State ownership, lifetime, failure behavior, and reset semantics.
4. If it needs a server, define a contract and implement it for both hosts, or add a host panel. Explain the difference in `StaticDemoHost`'s `LabNotes`.
5. Add a meaningful behavior test, update the route smoke checks and the demo browser check, and document the lesson.

## Change the notebook schema

```sh
dotnet tool restore
dotnet ef migrations add YourChange --project src/WebClient --output-dir Features/Notebook/Migrations
dotnet test -c Release
```

Commit the migration, designer, and model snapshot together. The app applies migrations on startup; the SQLite integration test verifies that a fresh database can be created from them. The browser notebook stores JSON, so keep `BrowserNote` compatible or migrate it in `learning.js`.
