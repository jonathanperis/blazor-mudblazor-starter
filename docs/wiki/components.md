# Components

## Layout Components

### MainLayout.razor

The root layout component that provides the MudBlazor application shell. Inherits from `LayoutComponentBase` and implements `IBrowserViewportObserver` for responsive design.

**Features:**
- `MudThemeProvider` with bindable dark mode toggle, persisted to localStorage
- Purple `MudAppBar` branded as **MudBlazor Starter**, with drawer toggle, dark mode control, and project overflow menu
- `MudDrawer` with `MudNavMenu` links labeled **Overview**, **Counter demo**, and **DataGrid demo**
- Project overflow links for GitHub, documentation, and health check
- `MudPopoverProvider`, `MudDialogProvider`, and `MudSnackbarProvider` for MudBlazor services
- Responsive breakpoint detection: displays a `MudToggleIconButton` on small screens and a `MudSwitch` on larger screens
- Semantic `<main>` wrapper and accessible labels for shell controls
- UI state (dark mode, drawer open, screen size) persisted to localStorage and restored on first render

**Key behavior:**
- Subscribes to `IBrowserViewportService` for breakpoint change notifications
- Implements `IAsyncDisposable` to unsubscribe from viewport events
- Delays rendering until localStorage state is loaded to prevent flash of unstyled content

### Breadcrumb.razor

A reusable compact breadcrumb navigation component that wraps `MudBreadcrumbs` in a `MudContainer` instead of a heavy raised card.

**Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `Items` | `List<BreadcrumbItem>` | List of breadcrumb items to display |

Uses a custom separator template with `MudIcon` (arrow forward icon) and an `aria-label="Breadcrumb"` navigation label. Each page defines its own breadcrumb items and passes them to this component.

---

## Page Components

### Home.razor

Route: `/`

Production-oriented starter overview page. It presents a concise hero, proof chips, GitHub/docs/DataGrid CTAs, a clone/run command block, and feature cards that explain the included deployment, documentation, and MudBlazor UI patterns.

### Counter.razor

Route: `/counter`

Interactive **Counter demo** page. Shows a prominent current count value, a primary **Increment count** button, and a **Reset** button that is disabled while the count is zero. Demonstrates Blazor component state and event handling without reading like untouched scaffold filler.

### Weather.razor

Route: `/weather`

Full-featured **DataGrid demo** page demonstrating CRUD operations, virtualization, row selection, paging, and clipboard integration.

**Features:**
- `MudDataGrid` with 69,420 generated weather forecast entries
- Header framed as **MudDataGrid showcase** with capability chips for virtualization, CRUD dialogs, and right-click copy
- Action row with **Add record**, **Remove selected**, and a selected-count chip
- `Remove selected` stays disabled until at least one row is selected
- Shortened record IDs via `ShortId(Guid)` to avoid full GUID visual noise
- Date, Temperature (C/F), and Summary columns without duplicated stress-test columns
- Multi-selection support with `SelectColumn`
- Quick filter search across displayed columns
- Sortable and filterable columns with `SortMode.Multiple`
- Virtualized rendering for performance with large datasets
- Fixed header with configurable page sizes (10, 25, 50, 100, 500, 1000, 5000)
- Loading state with simulated 2-second delay

**CRUD Operations:**
- Add: opens `AddWeather` dialog via `IDialogService`, appends a new entry, and shows `Weather record added.`
- Edit: opens `EditWeather` dialog with the selected item and replaces the entry in-place
- Remove: opens `RemoveWeather` confirmation dialog and removes all selected items

**Context Menu:**
- Right-click on a row to copy a single line or all selected lines to the clipboard
- Clipboard data is formatted as semicolon-separated values
- Empty-selection snackbar messages explicitly tell the user to select rows first

**Data model** (`WeatherForecast`): Defined as a nested class with `Id` (Guid), `Date` (DateTime), `TemperatureC` (int), `Summary` (string?), and computed `TemperatureF`.

### Error.razor

Route: `/Error`

Error page that displays when an unhandled exception occurs. Shows the request ID from `Activity.Current` or `HttpContext.TraceIdentifier` when available. Includes guidance about the Development environment.

---

## Weather Dialog Components

### AddWeather.razor

A `MudDialog` wrapped in an `EditForm` with `DataAnnotationsValidator`. Provides text fields for Weather ID (Guid), Date, Temperature (C), and Summary. On valid submission, returns the new `WeatherForecast` via `DialogResult.Ok` and shows a success snackbar notification.

### EditWeather.razor

A `MudDialog` wrapped in an `EditForm` for editing an existing weather entry.

**Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `Item` | `Weather.WeatherForecast` | The weather entry to edit |

The Weather ID field is read-only. On valid submission, returns the edited item via `DialogResult.Ok` and shows a success snackbar notification.

### RemoveWeather.razor

A simple `MudDialog` confirmation prompt. Displays a warning message asking the user to confirm deletion. On confirmation, returns `DialogResult.Ok(true)` and shows a success snackbar notification. Does not use `EditForm` since no data input is required.

---

## MudBlazor Components Used

| Component | Usage |
|---|---|
| `MudLayout`, `MudAppBar`, `MudDrawer`, `MudMainContent` | Application shell structure |
| `MudThemeProvider` | Material Design theming with dark mode |
| `MudNavMenu`, `MudNavLink` | Side navigation |
| `MudBreadcrumbs` | Compact page navigation breadcrumbs |
| `MudDataGrid`, `PropertyColumn`, `SelectColumn`, `TemplateColumn` | DataGrid demo table |
| `MudDataGridPager` | Data grid pagination |
| `MudDialog`, `MudDialogProvider` | Modal dialogs for CRUD operations |
| `MudSnackbar`, `MudSnackbarProvider` | Toast notifications |
| `MudButton`, `MudIconButton`, `MudToggleIconButton` | Action buttons |
| `MudTextField` | Form inputs and search |
| `MudCard`, `MudCardHeader`, `MudCardContent`, `MudCardActions` | Content cards |
| `MudMenu`, `MudMenuItem` | Context menu and overflow menu |
| `MudSwitch` | Dark mode toggle (large screens) |
| `MudText`, `MudLink`, `MudSpacer`, `MudDivider`, `MudIcon`, `MudChip` | Typography and layout utilities |
| `MudPopoverProvider` | Popover rendering |
