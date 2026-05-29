# Deployment

## Docker

### Build the Image

The Dockerfile uses a multi-stage build with the .NET 9 SDK and ASP.NET runtime images. The build context is the `src/` directory.

```bash
docker build -t blazor-mudblazor -f src/WebClient/Dockerfile src/
```

With production optimizations:

```bash
docker build \
  --build-arg AOT=false \
  --build-arg TRIM=true \
  --build-arg EXTRA_OPTIMIZE=true \
  --build-arg BUILD_CONFIGURATION=Release \
  -t blazor-mudblazor -f src/WebClient/Dockerfile src/
```

### Run the Container

```bash
docker run -p 5000:5000 blazor-mudblazor
```

The container listens on port 5000 (`ASPNETCORE_URLS=http://+:5000`). The entry point is the compiled `./WebClient` binary.

### Multi-Architecture Support

The release pipeline builds both `linux/amd64` and `linux/arm64/v8` images. It uses Docker Buildx for both builds, QEMU for the arm64 job, and then merges both digests into the multi-arch `:latest` manifest. The Dockerfile installs `clang` and `zlib1g-dev` in the SDK stage so optional AOT compilation has the native toolchain it needs.

### Pre-built Image

The latest release image is available from GitHub Container Registry:

```bash
docker pull ghcr.io/jonathanperis/blazor-mudblazor-starter:latest
docker run -p 5000:5000 ghcr.io/jonathanperis/blazor-mudblazor-starter:latest
```

---

## CI/CD Pipelines

### build-check.yml (Pull Requests)

Triggered on pull requests to `main`. Runs two jobs:

1. **setup-build-test**: Sets up the .NET SDK from `global.json`, restores dependencies, and builds the project with debug settings (`AOT=false`, `TRIM=false`, `BUILD_CONFIGURATION=Debug`).

2. **container-test**: Builds a Docker image, runs the container on port 5030, and polls the `/healthz` endpoint up to 20 times (5-second intervals) to verify the application starts correctly. Fails the pipeline if the health check does not return HTTP 200.

### main-release.yml (Main Branch)

Triggered on push to `main` or manual dispatch. The current release flow is split into six jobs:

1. **setup-build-test**: Restores and builds with production settings (`AOT=false`, `TRIM=true`, `EXTRA_OPTIMIZE=true`, `BUILD_CONFIGURATION=Release`).

2. **build-push-amd64**: Sets up Docker Buildx, authenticates to GitHub Container Registry, builds the `linux/amd64` image, and pushes it as `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest`.

3. **deploy-infra**: Logs in to Azure with OIDC (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`) and deploys `infra/main.bicep`/`infra/main.bicepparam` with `azure/arm-deploy`.

4. **deploy-image**: Deploys the GHCR `:latest` image to Azure App Service with `azure/webapps-deploy` and the `AZURE_WEBAPP_PUBLISH_PROFILE` secret.

5. **build-push-arm64**: Sets up QEMU and Docker Buildx, builds `linux/arm64/v8`, and pushes it as `:latest-arm64`.

6. **merge-manifest**: Combines the amd64 and arm64 digests into the final multi-arch `:latest` manifest.

### codeql.yml

Runs CodeQL security analysis on the codebase.

### deploy.yml (GitHub Pages)

Triggered on push to `main` or manual dispatch. Delegates to the reusable `jonathanperis/.github/.github/workflows/pages-docs-deploy.yml@main` workflow with inherited secrets; that shared workflow builds the Astro docs from `docs/` and publishes the generated Pages artifact.

---

## Azure Web App

The application is deployed to Azure App Service in the Brazil South region. The workflow first keeps the Azure resources current with Bicep over OIDC, then deploys the GHCR container image to the Web App with the Azure publish profile.

The Bicep entry point creates or updates the Web App, Log Analytics Workspace, and Application Insights instance, but it treats the App Service Plan as an existing shared resource. If that plan is missing, the infrastructure deployment fails before image deployment.

**Live demo:** [blazor-mudblazor-starter](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/)

### Azure Deployment Requirements

- Resource group: `github-jonathanperis`
- Region: `brazilsouth`
- Existing App Service Plan: `github-jonathanperis`
  - The plan is referenced as `existing` in `infra/main.bicep`; this repository does not create it.
  - The default parameter notes it is a shared plan managed outside this repo.
- Web App name: `blazor-mudblazor-starter`
- OIDC secrets for infrastructure deployment: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID`
- The `AZURE_WEBAPP_PUBLISH_PROFILE` secret set in the GitHub repository settings (download from Azure Portal > Web App > Deployment Center > Manage publish profile)
- GHCR image access configured on the Azure Web App (the image is public via GitHub Packages)

### Observability

`infra/main.bicep` provisions Log Analytics and Application Insights, then passes telemetry settings into the Web App module. The app only registers `AddApplicationInsightsTelemetry()` when `APPLICATIONINSIGHTS_CONNECTION_STRING` is present, so local runs stay telemetry-free by default while Azure deployments emit telemetry automatically.

Azure app settings managed by Bicep:

| Setting | Purpose |
|---|---|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Enables Application Insights telemetry in `Program.cs` |
| `APPINSIGHTS_INSTRUMENTATIONKEY` | Compatibility setting for App Service/Application Insights integration |
| `ApplicationInsightsAgent_EXTENSION_VERSION` | Enables the App Service Application Insights extension (`~3`) |
