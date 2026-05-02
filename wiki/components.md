# Components

## Layout Components

### MainLayout.razor

The root layout component that provides the application shell. Inherits from `LayoutComponentBase` and implements `IBrowserViewportObserver` for responsive design.

**Features:**
- `MudThemeProvider` with bindable dark mode toggle, persisted to localStorage
- `MudAppBar` with drawer toggle button, app title, dark mode switch, and overflow menu
- `MudDrawer` with `MudNavMenu` containing links to Home, Counter, and Weather pages
- `MudPopoverProvider`, `MudDialogProvider`, and `MudSnackbarProvider` for MudBlazor services
- Responsive breakpoint detection: displays a `MudToggleIconButton` on small screens and a `MudSwitch` on larger screens
- All UI state (dark mode, drawer open, screen size) persisted to localStorage and restored on first render

**Key behavior:**
- Subscribes to `IBrowserViewportService` for breakpoint change notifications
- Implements `IAsyncDisposable` to unsubscribe from viewport events
- Delays rendering until localStorage state is loaded to prevent flash of unstyled content

### Breadcrumb.razor

A reusable breadcrumb navigation component that wraps `MudBreadcrumbs` in a `MudCard`.

**Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `Items` | `List<BreadcrumbItem>` | List of breadcrumb items to display |

Uses a custom separator template with `MudIcon` (arrow forward icon). Each page defines its own breadcrumb items and passes them to this component.

---

## Page Components

### Home.razor

Route: `/`

Landing page that displays a welcome card with a link to the MudBlazor documentation. Uses the `Breadcrumb` component with a single "Home" item.

### Counter.razor

Route: `/counter`

Interactive counter demonstration. Displays the current count in a `MudCard` with a "Click me" `MudButton` that increments the value. Demonstrates Blazor's reactive data binding with a simple `_currentCount` field and `IncrementCount` method.

### Weather.razor

Route: `/weather`

Full-featured data grid page demonstrating CRUD operations, virtualization, and clipboard integration.

**Features:**
- `MudDataGrid` with 69,420 generated weather forecast entries
- Multi-column display: Id, Date, Temperature (C/F), Summary (with duplicate columns for horizontal scroll demo)
- Multi-selection support with `SelectColumn`
- Quick filter search across all displayed columns
- Sortable and filterable columns with `SortMode.Multiple`
- Virtualized rendering for performance with large datasets
- Fixed header with configurable page sizes (10, 25, 50, 100, 500, 1000, 5000)
- Loading state with simulated 2-second delay

**CRUD Operations:**
- Add: Opens `AddWeather` dialog via `IDialogService`, appends new entry to the collection
- Edit: Opens `EditWeather` dialog with the selected item, replaces the entry in-place
- Remove: Opens `RemoveWeather` confirmation dialog, removes all selected items

**Context Menu:**
- Right-click on a row to copy a single line or all selected lines to the clipboard
- Clipboard data formatted as semicolon-separated values

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
| `MudBreadcrumbs` | Page navigation breadcrumbs |
| `MudDataGrid`, `PropertyColumn`, `SelectColumn`, `TemplateColumn` | Weather data table |
| `MudDataGridPager` | Data grid pagination |
| `MudDialog`, `MudDialogProvider` | Modal dialogs for CRUD operations |
| `MudSnackbar`, `MudSnackbarProvider` | Toast notifications |
| `MudButton`, `MudIconButton`, `MudToggleIconButton` | Action buttons |
| `MudTextField` | Form inputs and search |
| `MudCard`, `MudCardHeader`, `MudCardContent`, `MudCardActions` | Content cards |
| `MudMenu`, `MudMenuItem` | Context menu and overflow menu |
| `MudSwitch` | Dark mode toggle (large screens) |
| `MudText`, `MudLink`, `MudSpacer`, `MudDivider`, `MudIcon` | Typography and layout utilities |
| `MudPopoverProvider` | Popover rendering |
