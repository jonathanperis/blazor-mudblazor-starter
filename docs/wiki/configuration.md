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

The policy permits later patches in the same SDK feature band, but the lockfiles record an SDK-implicit package (`Microsoft.AspNetCore.App.Internal.Assets`) whose version follows the SDK's bundled runtime. Update `global.json`, the Dockerfile SDK tag and the lockfiles together; on a different SDK patch, `--locked-mode` restore fails with NU1004 until the lockfiles are regenerated. Package versions live in the project files and the README stack table; [`src/Directory.Build.props`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/Directory.Build.props) holds the shared nullable, lockfile and audit settings. Every project has a NuGet lockfile. The server and shared projects declare both Linux runtime identifiers so locked container restores support amd64 and arm64. Known NuGet advisories fail restore through the NU1901–NU1904 warning policy.

## Runtime settings

| Key | Default | Purpose |
|---|---|---|
| `Learning:ApiBaseUrl` | `http://127.0.0.1:5000/` | Internal HTTP address for the typed client and diagnostics lab |
| `Learning:DataDirectory` | `App_Data` under the content root | SQLite database and data-protection keys |
| `Learning:EnableDemoAuth` | true only in Development | Enable fixed educational personas |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | unset | Opt in to Application Insights export |
| `ASPNETCORE_URLS` / `ASPNETCORE_HTTP_PORTS` | launch profile, or port 5000 in the container | Server listener |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | unset | Trust `X-Forwarded-*` from a TLS-terminating proxy so cookies are marked `Secure`; expose the app only through that proxy |
| `HTTPS_PORT` | unset | Enable application-level HTTPS redirection when a TLS endpoint is configured |

Use double underscores for hierarchical environment variables, for example `Learning__ApiBaseUrl`. ASP.NET Core does not automatically load `.env` files; supply environment variables or development user secrets using the standard configuration providers.

```sh
Learning__ApiBaseUrl=http://127.0.0.1:5050/ \
  dotnet run --project src/WebClient --no-launch-profile --urls http://127.0.0.1:5050
```

The no-launch-profile example uses the default Production environment, so demo sign-in is disabled. To study the identity lab, run the default Development profile.

## Preferences and culture

Only theme and drawer preferences use localStorage. Storage denial leaves controls usable for the current visit. Viewport size is derived, not stored.

Request localization supports English and Brazilian Portuguese. Only the culture cookie selects a culture: the browser's `Accept-Language` is ignored, so MudBlazor's built-in labels never switch language on their own inside an English interface, and both hosts behave the same. The culture form performs a full redirect so a fresh circuit inherits the culture. The WebAssembly demo stores the culture in localStorage and applies it before the .NET runtime starts; it loads full ICU data (`BlazorWebAssemblyLoadAllGlobalizationData`) so pt-BR formatting works whatever the browser's language. This example and MudBlazor's translated strings change; the teaching guide remains English.

The notebook uses a separate protected workspace cookie, issued for page renders only and renewed on each visit. Retain its cookie and the data-protection keys to retain access to the same notes. Demo personas do not identify notebook ownership.

## Publishing experiments

| Docker argument | Default | Effect |
|---|---|---|
| `BUILD_CONFIGURATION` | `Release` | Compilation configuration |
| `READY_TO_RUN` | `false` | Supported ReadyToRun precompilation |

The server app remains framework-dependent. Native AOT is unsupported for Blazor Server. Trimming, invariant globalization, and diagnostic-stripping modes are intentionally absent from the server app. The WebAssembly demo uses the SDK's default WebAssembly publishing (IL trimming of framework assemblies is part of that model) with globalization enabled. Measure publishing options using the [testing guide](../testing/).

## Optional telemetry

The application registers Application Insights only when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set. Console logging and the diagnostics lab work without Azure. Avoid logging notebook content, imported data, cookies, or credentials. Application hosting is planned for Hostinger, but its environment is not configured.
