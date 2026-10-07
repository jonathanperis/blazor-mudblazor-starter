# Product context

## Purpose

Blazor learning sandbox is a Swiss-army-knife school project for exploring Blazor, MudBlazor, and common application boundaries through working examples. The labs run in a Blazor Server host and in a static WebAssembly live demo.

## Audience

- Learners connecting C# and web concepts to observable behavior.
- Developers trying a pattern before adapting it to another project.
- Maintainers adding small, explainable experiments.

## Promise

Each lab makes its objective, source, behavior, lifetime, failure modes, and tradeoffs visible. A learner can predict, try, inspect, change, and reset an experiment.

## Learning surfaces

State/lifecycle, forms/dialogs, DataGrid, HTTP APIs, SQLite, authentication policies, localization/accessibility, files/cancellation, and observability/deployment.

## Product rules

- Local-first: foundational labs require no external account. The live demo requires nothing but a browser.
- Prefer focused feature modules shared by both hosts. Server-only boundaries sit behind a contract with a labeled browser stand-in; never present a stand-in as the real boundary.
- Keep synthetic data reproducible and resettable.
- Teach correct defaults: transactional editing, validation, cancellation, ownership, and server-side authorization.
- Label demonstration boundaries explicitly. Fixed personas and disposable cloud storage are learning choices.
- Add tests where they prove a material behavior and serve as examples.
- Keep cloud deployment optional and its cost/lifetime assumptions documented.

## Success signals

- A learner can open the live demo or run the app and complete the first exercise quickly.
- Each experiment has a direct source link and a clear next exercise.
- Cancel, reset, validation, and error recovery behave predictably.
- Documentation commands and implementation remain synchronized.
