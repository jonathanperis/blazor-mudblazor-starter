# Testing and experiments

## Run the checks

```sh
dotnet restore --locked-mode
dotnet test -c Release --no-restore
dotnet publish src/WebClient -c Release --no-restore -o artifacts/publish
dotnet publish src/WebClient.Wasm -c Release --no-restore -o artifacts/wasm
dotnet list package --vulnerable --include-transitive
```

Tests use xUnit, bUnit, ASP.NET Core's in-process server, and real SQLite migrations. Test databases live in task-owned directories under the test build output and are removed after the test host shuts down.

## What the tests prove

| Layer | Behavior |
|---|---|
| Components | Draft cancellation/confirmation, required-field rejection, callback/reset behavior, preference write paths, storage failure fallback, unsaved self-closing drawer |
| Forecast services | Deterministic IDs and paging, validated commits, CSV quoting, bounds, lossless escaping and blank lines, culture-aware search, cancellation |
| HTTP integration | Prerendered headings on every lab, liveness/readiness, bounded API requests with validation problems, cancellation and simulated failure, security headers, path-preserving skip link, base-relative links |
| Identity | Antiforgery rejection, anonymous/student/instructor policy results, disabled demo sign-in |
| Persistence | Migrations, workspace isolation, stale-update/delete conflicts, scoped reset, note limit, page-only sliding workspace cookie, tampered-cookie replacement |
| Static demo | In-browser API rules, encoded typed-client requests, localStorage notebook outcomes, browser personas, existing lab sources |
| Localization | Culture cookie, Portuguese resource text, culture-specific formatting |

These checks do not measure browser layout, assistive-technology behavior, or a live hosting environment. The WebAssembly demo has a browser check (below); the server app's browser behavior and post-deployment verification remain separate layers. Hostinger setup is pending.

## Check a published app through HTTP

Run the published app in one terminal:

```sh
dotnet artifacts/publish/WebClient.dll \
  --contentRoot "$PWD/artifacts/publish" --urls http://127.0.0.1:5000
```

Then:

```sh
python3 scripts/smoke-http.py --base-url http://127.0.0.1:5000
```

The script retries readiness, checks that every route prerenders its own heading, retrieves required static assets, and reads one API page. It does not click the UI.

## Check the WebAssembly demo in a browser

Build the Pages site with the demo, then drive it in Chromium:

```sh
dotnet publish src/WebClient.Wasm -c Release -o artifacts/wasm
cd docs
npm run build
python3 ../scripts/prepare-pages-demo.py
npx playwright install chromium   # once; or set PLAYWRIGHT_CHANNEL=chrome
npm run check:demo
```

The check serves `docs/out` like GitHub Pages (repository path, `404.html` fallback) and verifies deep links, the in-browser API, the skip link, scoped state, notebook conflicts, browser personas, culture reload, atomic CSV import, the largest grid, the not-found page, and a clean console.

## Compare performance honestly

1. Keep the same machine, build configuration, seed, and dataset size.
2. Warm up once and record several runs.
3. Separate generation time, server filtering time, network payload, and browser rendering.
4. Compare the in-memory grid with the paged API using the same dataset size.
5. Try ReadyToRun with `docker build --build-arg READY_TO_RUN=true ...` and compare startup plus image size against the default image.

The grid's displayed timing covers generation only. Virtualization is not a server memory limit. A stress dataset demonstrates a tradeoff; it is not a concurrency benchmark.

## Suggested manual UI pass

- Navigate using only the keyboard, including copy and dialog actions.
- Edit, cancel, reopen; confirm the original value remains.
- Submit empty and invalid fields; inspect focus and validation feedback.
- Switch themes at desktop/mobile widths and inspect contrast.
- Switch culture, reload, and verify selected language and formatting.
- Cancel API and file work; navigate away during a delayed request.
- Open the notebook in two tabs and provoke a conflict.
- Disconnect/reconnect and observe circuit ownership.

## Add the smallest meaningful test

Test the observable contract at the lowest sufficient layer. Keep one parameterized test when it can prove several related outcomes. Add a distinct case for an independently regressing boundary, such as a stale database version or a rejected oversized import.
