# Architecture reference

One .NET 10 Blazor Server host. `LabCatalog` defines routes and teaching metadata; `LabFrame` wraps experiments and error recovery. Feature modules support direct behavior tests.

- Forecasts are deterministic and local to the grid; the read-only API owns a separate seeded dataset.
- Dialogs edit drafts and commit only after validation/confirmation.
- Notebook operations use an EF Core context factory, a protected browser workspace, and GUID concurrency tokens. SQLite migrations and data-protection keys live with the configured data directory.
- Demo cookie personas are Development-only by default. Notebook ownership is independent of persona. Protected endpoints enforce authorization and form endpoints enforce antiforgery.
- UI preferences persist `isDarkMode` and `drawerOpen`; viewport state is derived from CSS.
- Supported publishing is framework-dependent, optionally ReadyToRun. Culture and diagnostic support remain enabled.
- PR validation tests behavior and builds docs/containers; release publishes an immutable multi-arch digest and optionally deploys Azure through OIDC.

Authoritative commands and boundaries: root `AGENTS.md`, `README.md`, and `docs/wiki/`.
