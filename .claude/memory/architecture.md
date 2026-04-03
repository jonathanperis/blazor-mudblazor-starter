---
name: Blazor Starter Architecture
description: Blazor Server patterns, MudBlazor service integration, state persistence, responsive design approach
type: project
---

## Component Architecture

**MainLayout.razor** is the application shell implementing:
- `IBrowserViewportObserver` for responsive breakpoint detection
- `IAsyncDisposable` for cleanup
- JavaScript interop for localStorage persistence (dark mode, drawer state)

**Dialog CRUD Pattern** (demonstrated in Weather page):
```
Weather.razor → DialogService.ShowAsync<AddWeather>() → DialogResult.Data.As<T>()
```
Each dialog is a separate component: AddWeather, EditWeather, RemoveWeather.

**Data Grid Pattern:**
- `MudDataGrid<T>` with `Virtualize=true` for 69K+ rows
- Multi-selection, filtering, sorting, right-click context menu
- Quick filter across multiple columns

## State Persistence

UI preferences persisted to browser localStorage:
- `isDarkMode` — dark/light theme toggle
- `isDrawerOpen` — navigation drawer state
- Read on `OnAfterRenderAsync(firstRender: true)`, saved on property change

## Responsive Design

`IBrowserViewportService` triggers `NotifyBrowserViewportChangeAsync`:
- Small screens (Sm/Xs): Toggle icon button for dark mode
- Large screens: Full switch control for dark mode
- Drawer behavior adapts to screen size

## Build Optimization Tiers

| Tier | AOT | TRIM | EXTRA_OPTIMIZE | Use Case |
|------|-----|------|----------------|----------|
| Debug | false | false | false | Local development |
| Release | false | true | true | Production deployment |

TRIM enables ReadyToRun + SingleFile + SelfContained.
EXTRA_OPTIMIZE strips symbols, debugger, globalization.
