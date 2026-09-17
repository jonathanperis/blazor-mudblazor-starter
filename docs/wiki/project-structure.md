# Project structure

```text
src/WebClient/
  Program.cs                     services, middleware, endpoints, startup migrations
  Components/
    Layout/                      shell, preferences, semantic navigation
    Learning/                    common lab frame and callback example
    Pages/                       overview, counter, DataGrid
      Labs/                      catalog and focused learning pages
    Weather/                     shared validated fields and draft dialogs
  Features/
    Forecasts/                   model, generator, CSV codec, API and typed client
    Learning/                    catalog, circuit state, preferences, cancellable work
    Notebook/                    workspace, EF Core service, health check, migrations
    Identity/                    educational cookie identities and culture endpoints
    Localization/                English and Portuguese resources
  wwwroot/                       theme-aware CSS, small JS interop helpers, icons
tests/WebClient.Tests/           bUnit, domain, HTTP and SQLite tests
scripts/                        docs drift/link checks and HTTP smoke checks
docs/wiki/                      Markdown learning guide
infra/                          historical Azure Bicep/ARM reference, compiled only
.config/dotnet-tools.json        pinned EF migration CLI
renovate.json                   shared dependency-update preset
```

## Boundaries

Keep related lab behavior together. Pages demonstrate the interaction; feature code owns reusable behavior that benefits from direct testing. One host is enough for these lessons.

The forecast model is independent of a Razor page. The notebook service depends on a short-lived context factory and learner workspace, not component rendering. The auth example enforces access at the endpoint, not just through UI visibility.

## Add a lab

1. Add its route under `Components/Pages/Labs/`.
2. Add metadata to `LabCatalog` and render its content through `LabFrame`.
3. State ownership, lifetime, failure behavior, and reset semantics.
4. Add focused feature code only when it provides a useful teaching/test boundary.
5. Add a meaningful behavior test, update route smoke checks, and document the lesson.

## Change the notebook schema

```sh
dotnet tool restore
dotnet ef migrations add YourChange --project src/WebClient --output-dir Features/Notebook/Migrations
dotnet test -c Release
```

Commit the migration, designer, and model snapshot together. The app applies migrations on startup; the SQLite integration test verifies that a fresh database can be created from them.
