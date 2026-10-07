# Learning paths

## Foundation: understand a component

| Lab | Time | Exercise |
|---|---|---|
| State and lifecycle (`/counter`) | 10 min | Compare component and scoped state across navigation and reload, in both hosts |
| Forms and transactional dialogs (`/labs/forms`) | 15 min | Submit invalid input, then edit and cancel |
| Localization and accessibility (`/labs/localization`) | 15 min | Switch to Portuguese and repeat using keyboard navigation |

Read [`CounterControl.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Learning/CounterControl.razor) for the child-to-parent `EventCallback`, and [`EditWeather.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Weather/EditWeather.razor) for the draft-copy pattern. Explain why the parent owns the committed object.

## Data: move from memory to persistence

| Lab | Time | Exercise |
|---|---|---|
| DataGrid experiments (`/weather`) | 20 min | Compare dataset sizes with a fixed seed and typed date filters |
| API and server paging (`/labs/api`) | 20 min | Compare payload size, add latency, cancel, and simulate a failure |
| SQLite notebook (`/labs/persistence`) | 25 min | Edit the same note in two tabs and observe the concurrency conflict |

Compare [`Weather.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Pages/Weather.razor), [`ForecastApi.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Forecasts/ForecastApi.cs), and [`NotebookService.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Notebook/NotebookService.cs). Ask which costs virtualization removes, which costs remain on the server, and how a short-lived DbContext differs from a circuit-scoped service. Then compare [`BrowserNotebookStore.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/StaticDemo/BrowserNotebookStore.cs), which keeps the same `INotebookStore` contract in the browser, and [`ForecastApiSimulator.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/StaticDemo/ForecastApiSimulator.cs), which answers the typed client without a server.

## Boundaries: ownership, access, and cancellation

| Lab | Time | Exercise |
|---|---|---|
| Authentication and policies (`/labs/auth`) | 20 min | Observe 401, 403, and 200 from the same protected endpoint |
| Files and cancellable work (`/labs/files`) | 25 min | Import quoted CSV; reject an invalid row; cancel processing |
| Observability and deployment (`/labs/observability`) | 20 min | Find a returned trace ID in structured console logs |

Demo personas share no notebook workspace identity. In the WebAssembly demo, personas only change what the browser renders; compare that with the server's 401/403/200 responses to see why authorization belongs on the server. The notebook is isolated by a separate protected browser cookie. The file-processing experiment is component-owned, not a durable job queue.

## Extend the toolbox

Pick one exercise at a time:

1. Add a new validation rule and protect its observable behavior with a test.
2. Add another translated string using the existing resource files.
3. Add an API sort field while keeping the input allowlist explicit.
4. Add a notebook property, create a migration, and test existing-data upgrades.
5. Replace demo identities with a real identity provider in your own fork.
6. Build a durable job lab using a bounded hosted queue and persisted state; compare its lifetime with the current component-owned example.

For each new lab, add catalog metadata, a source link, an exercise, ownership/lifetime notes, and the smallest meaningful test set. See [Testing](../testing/) and [Project structure](../project-structure/).
