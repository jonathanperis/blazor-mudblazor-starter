# Configuration

## SDK and packages

`global.json` selects the supported SDK:

```json
{
  "sdk": {
    "version": "10.0.401",
    "rollForward": "latestPatch"
  }
}
```

The policy permits later patches in the same SDK feature band. Package versions live in `WebClient.csproj` and the README stack table. Application and test NuGet lockfiles make restores reproducible. The application declares both Linux runtime identifiers so locked container restores support amd64 and arm64. Known NuGet advisories fail restore through the NU1901–NU1904 warning policy.

## Runtime settings

| Key | Default | Purpose |
|---|---|---|
| `Learning:ApiBaseUrl` | `http://127.0.0.1:5000/` | Internal HTTP address for the typed client and diagnostics lab |
| `Learning:DataDirectory` | `App_Data` under the content root | SQLite database and data-protection keys |
| `Learning:EnableDemoAuth` | true only in Development | Enable fixed educational personas |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | unset | Opt in to Application Insights export |
| `ASPNETCORE_URLS` | launch profile or container port 5000 | Server listener |
| `HTTPS_PORT` | unset | Enable application-level HTTPS redirection when a TLS endpoint is configured |

Use double underscores for hierarchical environment variables, for example `Learning__ApiBaseUrl`. ASP.NET Core does not automatically load `.env` files; supply environment variables or development user secrets using the standard configuration providers.

```sh
Learning__ApiBaseUrl=http://127.0.0.1:5050/ \
  dotnet run --project src/WebClient --no-launch-profile --urls http://127.0.0.1:5050
```

The no-launch-profile example uses the default Production environment, so demo sign-in is disabled. To study the identity lab, run the default Development profile.

## Preferences and culture

Only theme and drawer preferences use localStorage. Storage denial leaves controls usable for the current visit. Viewport size is derived, not stored.

Request localization supports English and Brazilian Portuguese. The culture form performs a full redirect so a fresh circuit inherits the culture. This example and MudBlazor's translated strings change; the teaching guide remains English.

The notebook uses a separate protected workspace cookie. Retain its cookie and the data-protection keys to retain access to the same notes. Demo personas do not identify notebook ownership.

## Publishing experiments

| Docker argument | Default | Effect |
|---|---|---|
| `BUILD_CONFIGURATION` | `Release` | Compilation configuration |
| `READY_TO_RUN` | `false` | Supported ReadyToRun precompilation |

The app remains framework-dependent. Native AOT is unsupported for Blazor Server. Trimming, invariant globalization, and diagnostic-stripping modes are intentionally absent. Measure publishing options using the [testing guide](../testing/).

## Optional telemetry

The application registers Application Insights only when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set. Console logging and the diagnostics lab work without Azure. Avoid logging notebook content, imported data, cookies, or credentials. The Bicep deployment uses SDK instrumentation, without a duplicate auto-instrumentation agent.
