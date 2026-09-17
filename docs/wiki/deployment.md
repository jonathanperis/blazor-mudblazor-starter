# Docker and hosting

## Current delivery boundary

**Hostinger is the intended application host. This project's hosting environment is not configured yet.** The release workflow validates and publishes container images; it does not deploy the application. There is no active public lab URL advertised by this repository.

## Local container exercise

```sh
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
docker run --rm -p 5000:5000 -v learning-data:/app/App_Data blazor-learning
```

The image uses .NET 10, runs as `app`, and listens on port 5000. `src/.dockerignore` excludes local build artifacts, data files, and environment files.

| Docker argument | Default | Purpose |
|---|---|---|
| `BUILD_CONFIGURATION` | `Release` | Build configuration |
| `READY_TO_RUN` | `false` | Compare supported ReadyToRun publishing |

Both publishing modes keep culture and diagnostics support. `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest` is a convenience tag. Use `sha-<commit>` or a manifest digest to identify the exact build.

## PR validation

`build-check.yml` runs behavioral tests, locked dependency restore/audit, published-app HTTP smoke checks, documentation build/link/drift checks, dependency review, infrastructure compilation, workflow linting, and a Docker matrix for both ReadyToRun values. Trivy scans the default image for high/critical vulnerabilities.

The existing protected-branch names `setup-build-test` and `container-test` are preserved. The latter requires the complete container matrix to pass.

## Release flow

`main-release.yml` has two jobs:

1. **validate** calls the reusable Build Check workflow.
2. **publish** builds linux/amd64 and linux/arm64 in one manifest and publishes `sha-<commit>` plus `latest`. The manifest digest identifies the image for a future deployment.

Release runs are serialized with cancellation disabled. Package write permission belongs only to publishing. No application-deployment job, hosting credentials, or deployment environment is configured.

## Before configuring Hostinger

Choose and document the actual Hostinger service/environment before adding a deployment workflow. The repository currently assumes no particular Hostinger product or deployment API.

- Confirm support for the .NET container, the selected CPU architecture, and long-lived WebSocket connections.
- Configure the hostname, TLS termination, reverse proxy, internal HTTP port, and trusted forwarded-header handling for that environment.
- Retain SQLite and data-protection keys together on storage writable by the `app` user. Plan backups and restore testing.
- Keep demo authentication disabled outside an explicitly educational environment.
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

## Documentation publishing

The separate `deploy.yml` workflow delegates to `jonathanperis/.github/.github/workflows/pages-docs-deploy.yml@3a6707da1d9f043bc3fa760bc08525db96d34c9d` for the static GitHub Pages guide. The reviewed commit pin is checked against this guide, and only the optional public analytics ID is passed as a secret. This is documentation publishing, separate from application hosting.
