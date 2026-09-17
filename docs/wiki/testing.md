# Testing and experiments

## Run the checks

```sh
dotnet restore --locked-mode
dotnet test -c Release --no-restore
dotnet publish src/WebClient -c Release --no-restore -o artifacts/publish
dotnet list package --vulnerable --include-transitive
```

Tests use xUnit, bUnit, ASP.NET Core's in-process server, and real SQLite migrations. Test databases live in task-owned directories under the test build output and are removed after the test host shuts down.

## What the tests prove

| Layer | Behavior |
|---|---|
| Components | Draft cancellation/confirmation, required-field rejection, callback/reset behavior, preference write paths |
| Forecast services | Deterministic IDs and paging, validated commits, CSV quoting and bounds, cancellation |
| HTTP integration | Prerendered headings on every lab, liveness/readiness, bounded API requests, cancellation and simulated failure |
| Identity | Antiforgery rejection, anonymous/student/instructor policy results, disabled demo sign-in |
| Persistence | Migrations, workspace isolation, stale-update/delete conflicts, scoped reset |
| Localization | Culture cookie, Portuguese resource text, culture-specific formatting |

These checks do not measure browser layout, assistive-technology behavior, or a live Azure deployment. A separate browser session is needed for those observations.

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

The script retries readiness, checks every lab's prerendered heading, and retrieves required static assets. It does not click the UI.

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
