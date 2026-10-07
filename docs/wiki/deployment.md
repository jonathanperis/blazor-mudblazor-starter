# Docker and hosting

## Current delivery boundary

**Hostinger is the intended host for the Blazor Server app. Its environment is not configured yet.** The release workflow validates and publishes container images; it does not deploy the server app.

The [live demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/) is the WebAssembly build of the same labs, published as static files to GitHub Pages next to this guide. It has no server: the API, notebook, sign-in, culture and diagnostics labs use labeled browser stand-ins.

## Local container exercise

```sh
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
docker run --rm -p 5000:5000 -v learning-data:/app/App_Data blazor-learning
```

The image uses .NET 10, runs as the non-root `app` user (numeric `APP_UID`, so orchestrators can verify `runAsNonRoot`), and listens on port 5000 through `ASPNETCORE_HTTP_PORTS`. The SDK stage runs on the build machine's architecture and cross-publishes for the target, so multi-architecture builds never run the SDK under emulation. `src/.dockerignore` excludes the WebAssembly host, local build artifacts, data files, IDE folders and environment files.

The runtime image has no `curl` or `wget`, so it declares no `HEALTHCHECK`. Configure the orchestrator to probe `/healthz` (liveness) and `/healthz/ready` (SQLite readiness).

| Docker argument | Default | Purpose |
|---|---|---|
| `BUILD_CONFIGURATION` | `Release` | Build configuration |
| `READY_TO_RUN` | `false` | Compare supported ReadyToRun publishing |

Both publishing modes keep culture and diagnostics support. `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest` is a convenience tag. Use `sha-<commit>` or a manifest digest to identify the exact build.

## PR validation

[`build-check.yml`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/.github/workflows/build-check.yml) runs behavioral tests, locked dependency restore/audit, published-app HTTP smoke checks, the Pages site build (guide plus WebAssembly demo) with type/link/drift/rendered checks and a browser check of the demo, dependency review, infrastructure compilation with a stale-`main.json` check, workflow linting, and a Docker matrix for both ReadyToRun values. Trivy scans the default image for fixable high/critical vulnerabilities (`--ignore-unfixed`), so a base-image CVE without a released fix does not block every change. Every job has a timeout, and a newer push to a pull request cancels the superseded run.

The scan uses the official, digest-pinned Trivy container with a read-only exported image archive. This preserves the repository's action allow-list and avoids requiring a nested third-party setup action or access to the Docker socket.

The existing protected-branch names `setup-build-test` and `container-test` are preserved. The latter requires the complete container matrix to pass.

## Release flow

`main-release.yml` builds each artifact once, verifies what it built, and only then points `latest` or the live site at it:

1. **validate** calls the reusable Build Check workflow. On `main`, its `pages` job also uploads the checked Pages site as the deployment artifact.
2. **publish** builds linux/amd64 and linux/arm64 in one manifest and pushes it only as `sha-<commit>`.
3. **verify** pulls that exact digest on a native amd64 runner and a native arm64 runner, runs the HTTP smoke checks, and scans each platform with Trivy.
4. **promote** points `latest` at the verified digest, records the digest in the run summary, and attaches signed build provenance (`gh attestation verify oci://<image>@<digest> --owner jonathanperis`).
5. **deploy-pages** deploys the validated guide and WebAssembly demo to GitHub Pages.

Release runs are serialized with cancellation disabled; Pages deployments share the `github-pages` concurrency group. Package write permission belongs only to publishing and promotion, and Pages permissions only to its deployment job. No server-deployment job, hosting credentials, or server deployment environment is configured.

## Before configuring Hostinger

Choose and document the actual Hostinger service/environment before adding a deployment workflow. The repository currently assumes no particular Hostinger product or deployment API.

- Confirm support for the .NET container, the selected CPU architecture, and long-lived WebSocket connections.
- Configure the hostname, TLS termination, reverse proxy and internal HTTP port. Behind a TLS-terminating proxy set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, so `Request.IsHttps` is true and the workspace and culture cookies are marked `Secure`. That setting trusts forwarded headers from any source, so the app must only be reachable through the proxy.
- Retain SQLite and data-protection keys together on storage writable by the `app` user. Plan backups and restore testing.
- Keep demo authentication disabled outside an explicitly educational environment.
- Review the notebook bounds for public use: 50 notes per workspace, 4,000 characters per note, and a workspace cookie whose 30-day lifetime slides on each visit. Workspaces that stop visiting keep their rows; add a retention job if storage matters.
- The app sends `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy` and a `frame-ancestors 'none'` policy. A full Content-Security-Policy needs tuning for MudBlazor's inline styles; add it at the proxy or in `Program.cs` once tested.
- Decide how the host pulls the exact image digest, how credentials are stored, and how a failed rollout is reversed.
- Verify `/healthz`, `/healthz/ready`, circuit reconnection, static assets, and representative browser interactions after deployment is authorized.

Application Insights remains optional through `APPLICATIONINSIGHTS_CONNECTION_STRING`; console logging and the diagnostics lab work without it.

## Data and work lifetime

A Docker named volume preserves SQLite and workspace-protection keys locally. Without persistent storage, replacement containers lose the notebook data or its workspace keys. A durable multi-instance application needs suitable shared storage and an identity model beyond the local demo personas.

The component-owned work example stops on disposal. A durable job needs persisted state and a hosted worker/queue.

## Historical Azure reference

`infra/` retains the earlier Bicep templates and generated `main.json` as learning/reference material. They are compiled in CI but are not used by the release workflow. Their resource names and storage assumptions are not Hostinger configuration.

```sh
az bicep build --file infra/main.bicep --stdout
az bicep build-params --file infra/main.bicepparam --stdout
```

## GitHub Pages: guide and live demo

The Pages site is one artifact built in the Build Check `pages` job:

1. Astro builds the guide into `docs/out`, including a site-wide `404.html`.
2. `dotnet publish src/WebClient.Wasm` produces the static WebAssembly app.
3. [`prepare-pages-demo.py`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/scripts/prepare-pages-demo.py) copies it to `docs/out/demo/`, rewrites `<base href>` to `/blazor-mudblazor-starter/demo/`, drops precompressed copies (Pages compresses responses itself), and verifies the boot assets.
4. `npm run check:rendered` checks the guide, and `npm run check:demo` drives the demo in Chromium against a local server that behaves like Pages.

GitHub Pages has no rewrite rules. A deep link such as `/demo/labs/api` is answered by `404.html`, which redirects to `/demo/?p=/labs/api`; `index.html` restores the path with `history.replaceState` before Blazor starts. This repository builds its own Pages artifact instead of the shared `pages-docs-deploy.yml` workflow, because the site now needs a .NET build step and the same checks as pull requests.

Only the optional public analytics ID is passed as a secret. Pages deployment is separate from hosting the server app.
