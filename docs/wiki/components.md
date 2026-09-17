# Lab reference

## Shared shell

[`MainLayout.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Components/Layout/MainLayout.razor) renders the shell immediately. Its first interactive render loads preferences through [`UiPreferences`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Learning/UiPreferences.cs); subsequent theme and drawer events await their writes. Only `isDarkMode` and `drawerOpen` are persisted. CSS determines the small-screen theme control.

Custom colors use MudBlazor palette variables. The shell includes a skip link, semantic main landmark, labeled icon controls, error recovery, and navigation to every lab.

[`LabFrame.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Components/Learning/LabFrame.razor) reads `LabCatalog`, displays objectives and exercises, and wraps the experiment in an error boundary. Recovery invokes the experiment's reset callback before recreating child content. Labs without a safe reset callback offer a full reload.

## State and forms

- [`Counter.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Components/Pages/Counter.razor) owns the component count; `CounterControl.razor` raises an event callback.
- `CircuitCounter` is scoped to the interactive circuit. Its lifetime differs from prerendered component state.
- [`WeatherForecast`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Forecasts/WeatherForecast.cs) defines validation, sensible defaults, a draft copy, and validated application of changes.
- `ForecastFields.razor` connects each input to the EditForm with `For` expressions.
- [`EditWeather.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Components/Weather/EditWeather.razor) edits a copy. The caller applies it only after a successful dialog result.

## DataGrid

[`/weather`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Components/Pages/Weather.razor) uses a component-owned list and seeded generator. Reset recreates the grid, clearing its search, filters, sort, selection, and pagination. The date column binds the date value and uses a display format.

Search is debounced. Copy is available from visible row/toolbar controls as well as the context menu. Selected CSV copy is limited to 1,000 rows. Dataset generation timings measure generation only, not browser rendering or concurrent-server capacity.

## HTTP API

[`GET /api/forecasts`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Forecasts/ForecastApi.cs) accepts:

| Parameter | Default | Bound |
|---|---|---|
| `count` | 1000 | 1–69,420 |
| `page` | 0 | 0–69,420 |
| `pageSize` | 25 | 1–100 |
| `search` | empty | 120 characters |
| `delayMs` | 0 | 0–2000 |
| `descending` | false | Boolean, sort by date |
| `fail` | false | Boolean, simulate HTTP 503 |

Responses contain `{ items, total }`. Invalid bounds return 400. The endpoint observes request cancellation. The typed client uses a configured internal base URL. In the [API page](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Components/Pages/Labs/Api.razor), **Load page**, **Previous**, and **Next** start a new load, cancel the previous request, and ignore its late result. Editing fields alone does not start or cancel a request; press **Load page** to apply the changed inputs. **Cancel request**, reset, and disposal also cancel owned work.

## SQLite notebook

[`NotebookService`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Notebook/NotebookService.cs) creates a DbContext per operation. All queries and writes include the current workspace. The root passes the protected-cookie workspace into the circuit as a server component parameter; interactive services do not depend on a live HttpContext. A GUID version token implements optimistic concurrency on SQLite. Conflicts retain the UI draft and require reloading the current note rather than silently overwriting another tab's change.

Migrations run at startup for this single-instance learning app. Notebook reset deletes only the current workspace's notes and requires explicit confirmation in the page.

## Identity, culture, files, and diagnostics

- [`POST /auth/demo`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Identity/DemoIdentity.cs) accepts only `student` or `instructor`; it is disabled unless the demo feature is enabled. `POST /auth/logout` clears the cookie. Both require antiforgery validation.
- `GET /api/instructor` enforces an authenticated instructor policy on the server.
- `POST /culture` validates an antiforgery token and a supported culture (`en-US` or `pt-BR`), writes a culture cookie, and starts a new page/circuit.
- [`ForecastCsv`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Forecasts/ForecastCsv.cs) handles quoted commas, quotes, CRLF, and multiline fields. Imports are at most 1 MiB/1,000 rows and reject invalid or duplicate identities before returning a dataset. Export neutralizes formula-like summaries with an apostrophe, which remains literal on reimport.
- [`BatchExperiment`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Learning/BatchExperiment.cs) runs bounded, cancellable asynchronous work. The page cancels it on reset or disposal.
- `GET /api/diagnostics` logs a trace ID and returns it. `/healthz` is liveness; `/healthz/ready` queries SQLite.
