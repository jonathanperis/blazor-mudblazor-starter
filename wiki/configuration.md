# Configuration

## Application Settings

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Key | Default | Description |
|---|---|---|
| `Logging:LogLevel:Default` | `Information` | Minimum log level for all categories |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Log level for ASP.NET Core framework logs |
| `AllowedHosts` | `*` | Allowed host headers (all hosts by default) |

### appsettings.Development.json

Overrides for local development. Currently mirrors the base logging configuration.

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` for local dev (auto-set by launch profiles) |
| `ASPNETCORE_URLS` | `http://+:5000` | Listening URL (set in Dockerfile for container builds) |

## .NET SDK Version

Pinned in `global.json`:

```json
{
  "sdk": {
    "version": "9.0.202",
    "rollForward": "minor"
  }
}
```

The `rollForward: minor` policy allows using any 9.0.x SDK version at or above 9.0.202.

## MudBlazor Service Registration

In `Program.cs`, MudBlazor is registered with:

```csharp
builder.Services.AddMudServices();
builder.Services.AddMudTranslations();
MudGlobal.UnhandledExceptionHandler = Console.WriteLine;
```

- `AddMudServices()` registers all MudBlazor services (dialogs, snackbar, scroll, resize, etc.)
- `AddMudTranslations()` registers localization support (MudBlazor.Translations 3.3.0)
- `MudGlobal.UnhandledExceptionHandler` routes unhandled exceptions to the console

## Docker Build Arguments

The Dockerfile accepts four build arguments that control the compilation output:

| Argument | Default | Values | Description |
|---|---|---|---|
| `AOT` | `false` | `true`, `false` | Enable Ahead-of-Time compilation for faster startup |
| `TRIM` | `false` | `true`, `false` | Enable ReadyToRun, single-file publish, and self-contained deployment |
| `EXTRA_OPTIMIZE` | `false` | `true`, `false` | Strip symbols, disable debugger support, invariant globalization |
| `BUILD_CONFIGURATION` | `Debug` | `Debug`, `Release` | .NET build configuration |

### Build Flag Details

**AOT** (`AOT=true`): Enables `PublishAot` with `OptimizationPreference` set to `Speed`. Produces a native binary with faster cold-start performance.

**TRIM** (`Trim=true`): Enables `PublishReadyToRun`, `PublishReadyToRunComposite`, `PublishSingleFile`, and `SelfContained`. Produces an optimized single-file deployment.

**EXTRA_OPTIMIZE** (`ExtraOptimize=true`): Applies aggressive optimizations for minimal binary size:
- `TrimmerRemoveSymbols` -- removes debug symbols
- `DebuggerSupport` -- disabled
- `InvariantGlobalization` -- uses invariant culture
- `EventSourceSupport` -- disabled
- `HttpActivityPropagationSupport` -- disabled
- `MetadataUpdaterSupport` -- disabled
- `StackTraceSupport` -- disabled
- `UseSystemResourceKeys` -- uses system resource keys instead of embedded strings

### Production Defaults

The `main-release.yml` pipeline uses these defaults:

```
AOT=false
TRIM=true
EXTRA_OPTIMIZE=true
BUILD_CONFIGURATION=Release
```
