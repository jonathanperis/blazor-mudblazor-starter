# Architecture reference

Shared labs in `WebClient.Shared` (Razor class library) rendered by two hosts: `WebClient` (Blazor Server) and `WebClient.Wasm` (static WebAssembly demo on GitHub Pages at `/demo/`). `LabCatalog` defines routes and teaching metadata; `LabFrame` wraps experiments, host notes and error recovery. `LearningHost` describes each host and its panel types. Feature modules support direct behavior tests.

- Forecasts are deterministic and local to the grid; the read-only API owns a separate seeded dataset.
- Dialogs edit drafts and commit only after validation/confirmation.
- Notebook pages use `INotebookStore`: EF Core + SQLite on the server (context factory, protected workspace cookie for page renders only, sliding 30 days), localStorage in the demo. GUID versions detect conflicts; 50 notes per workspace. SQLite migrations and data-protection keys live with the configured data directory.
- Demo cookie personas are Development-only by default. Notebook ownership is independent of persona. Protected endpoints enforce authorization and form endpoints enforce antiforgery.
- UI preferences persist `isDarkMode` and `drawerOpen`; viewport state is derived from CSS.
- Supported publishing is framework-dependent, optionally ReadyToRun. Culture and diagnostic support remain enabled.
- PR validation tests behavior, builds the Pages site (guide + demo, browser-checked) and containers. Release pushes `sha-<commit>`, verifies the digest natively per arch, promotes `latest` with provenance, and deploys the validated Pages artifact. Hostinger setup is pending. Azure templates remain historical reference material, not an active deployment path.

Authoritative commands and boundaries: root `AGENTS.md`, `README.md`, and `docs/wiki/`.
