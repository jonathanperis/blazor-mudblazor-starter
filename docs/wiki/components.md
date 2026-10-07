# Lab reference

## Shared shell

[`MainLayout.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Layout/MainLayout.razor) renders the shell immediately. Its first interactive render loads preferences through [`UiPreferences`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Learning/UiPreferences.cs); subsequent theme and drawer-button events await their writes. Only `isDarkMode` and `drawerOpen` are persisted; a responsive drawer that closes itself on a small screen is not saved as a preference. CSS determines the small-screen theme control. If storage or the interop script fails, preferences stay in memory and the shell keeps working.

Custom colors use MudBlazor palette variables. The shell includes a skip link, semantic main landmark, labeled icon controls, error recovery, and navigation to every lab. The skip link keeps the current path (a bare `#main-content` would resolve against `<base href>` and leave the page) and moves focus to the main landmark. Internal links are base-relative (`labs`, not `/labs`), so the same markup works at `/` and under the GitHub Pages demo path. **Reload lab** forces a full reload, because a link to the current address is an in-app navigation that keeps a failed error boundary.

[`LabFrame.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Learning/LabFrame.razor) reads `LabCatalog`, displays objectives, linked prerequisites, host notes and exercises, and wraps the experiment in an error boundary. Recovery invokes the experiment's reset callback before recreating child content. Labs without a safe reset callback offer a full reload.

## State and forms

- [`Counter.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Pages/Counter.razor) owns the component count; `CounterControl.razor` raises an event callback.
- `ScopedCounter` is a scoped service: one per interactive circuit in Blazor Server, one per browser tab in WebAssembly. Its lifetime differs from prerendered component state.
- [`WeatherForecast`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Forecasts/WeatherForecast.cs) defines validation, sensible defaults, a draft copy, and validated application of changes.
- `ForecastFields.razor` connects each input to the EditForm with `For` expressions.
- [`EditWeather.razor`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Weather/EditWeather.razor) edits a copy. The caller applies it only after a successful dialog result.

## DataGrid

[`/weather`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Pages/Weather.razor) uses a component-owned list and seeded generator. **Generate dataset** applies the chosen size and seed; **Reset experiment** also restores the defaults (100 rows, seed 42). Both recreate the grid, clearing its search, filters, sort, selection, and pagination. The date column binds the date value and uses a display format.

Search is debounced. Copy is available from visible row/toolbar controls; the row context menu is an additional shortcut whose anchor is inert, so it adds no empty focusable control. Selected CSV copy is limited to 1,000 rows. Dataset generation timings measure generation only, not browser rendering or concurrent-server capacity.

## HTTP API

[`GET /api/forecasts`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Forecasts/ForecastEndpoints.cs) accepts the parameters below. [`ForecastQuery`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Forecasts/ForecastApi.cs) holds the shared validation rules.

| Parameter | Default | Bound |
|---|---|---|
| `count` | 1000 | 1–69,420 |
| `page` | 0 | 0–69,420 |
| `pageSize` | 25 | 1–100 |
| `search` | empty | 120 characters |
| `delayMs` | 0 | 0–2000 |
| `descending` | false | Boolean, sort by date |
| `fail` | false | Boolean, simulate HTTP 503 |
| `culture` | `en-US` | `en-US` or `pt-BR`; how search matches dates and numbers |

Responses contain `{ items, total }`. Invalid bounds return a 400 validation problem (`application/problem+json`) whose `errors` are keyed by parameter. The API cannot see the caller's UI culture, so the typed client sends `culture`; dates also match as `yyyy-MM-dd`. The endpoint observes request cancellation. The typed client uses a configured internal base URL. In the [API page](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Pages/Labs/Api.razor), **Load page**, **Previous**, and **Next** start a new load, cancel the previous request, and ignore its late result. **Previous** and **Next** page the query that produced the visible results, not fields edited since. Editing fields alone does not start or cancel a request; press **Load page** to apply the changed inputs. **Cancel request**, reset, and disposal also cancel owned work.

## SQLite notebook

The page depends on [`INotebookStore`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Notebook/NotebookContracts.cs). It cancels its own work on disposal, ignores an older load that finishes after a newer one, and receives the prerendered list through `[PersistentState]`, so a page load reads the notebook once.

[`NotebookService`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Notebook/NotebookService.cs) creates a DbContext per operation. All queries and writes include the current workspace. The workspace cookie is issued only for page renders (not static files, APIs or health probes) and re-issued on each visit, so its 30-day lifetime slides. The root passes the protected-cookie workspace into the circuit as a server component parameter; interactive services do not depend on a live HttpContext. A GUID version token implements optimistic concurrency on SQLite; EF Core's concurrency exception becomes the shared `NotebookConflictException`. A workspace holds at most 50 notes. Conflicts retain the UI draft and require reloading the current note rather than silently overwriting another tab's change.

Migrations run at startup for this single-instance learning app. Notebook reset deletes only the current workspace's notes and requires explicit confirmation in the page.

## Identity, culture, files, and diagnostics

- [`POST /auth/demo`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient/Features/Identity/DemoIdentity.cs) accepts only `student` or `instructor`; it is disabled unless the demo feature is enabled. `POST /auth/logout` clears the cookie. Both require antiforgery validation.
- `GET /api/instructor` enforces an authenticated instructor policy on the server.
- `POST /culture` validates an antiforgery token and a supported culture (`en-US` or `pt-BR`), writes a culture cookie, and starts a new page/circuit.
- [`ForecastCsv`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Forecasts/ForecastCsv.cs) handles quoted commas, quotes, CRLF, and multiline fields. Imports are at most 1 MiB/1,000 rows and reject invalid or duplicate identities before returning a dataset. Export neutralizes formula-like summaries (and a leading apostrophe) with an apostrophe; import removes exactly one, so a round trip keeps every summary unchanged, even at the 120-character limit. Blank lines are ignored.
- [`BatchExperiment`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Learning/BatchExperiment.cs) runs bounded, cancellable asynchronous work. The page cancels it on reset or disposal, and its live region announces state changes rather than every progress step.
- `GET /api/diagnostics` logs a trace ID and returns it. `/healthz` is liveness; `/healthz/ready` queries SQLite.

## WebAssembly demo stand-ins

The [static demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/) renders the same pages with browser implementations from [`Features/StaticDemo`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/StaticDemo/ForecastApiSimulator.cs):

| Lab | Stand-in | What still holds | What only the server shows |
|---|---|---|---|
| API | `ForecastApiSimulator`, an `HttpMessageHandler` behind the same typed client | validation, latency, 503 failure, cancellation, paging | real HTTP, network payloads |
| Notebook | `BrowserNotebookStore` on localStorage | drafts, version conflicts between tabs, the note limit | SQLite, EF Core, migrations, workspace cookie |
| Authentication | `DemoAuthenticationStateProvider` | `AuthorizeView` and policies in the UI | authorization: 401/403/200 from an endpoint |
| Localization | culture in localStorage, applied before the runtime starts | resource strings, formatting, `lang` | request localization and the culture cookie |
| Observability | browser console logging | structured log with a trace ID | health checks, server traces |

Each lab shows a host note explaining the difference.
