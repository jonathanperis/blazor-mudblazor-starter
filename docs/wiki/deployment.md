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

The release pipeline builds for both `linux/amd64` and `linux/arm64/v8` using Docker Buildx with QEMU emulation. The Dockerfile installs `clang` and `zlib1g-dev` in the SDK stage to support AOT compilation on both architectures.

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

Triggered on push to `main` or manual dispatch. Runs three sequential jobs:

1. **setup-build-test**: Restores and builds with production settings (`TRIM=true`, `EXTRA_OPTIMIZE=true`, `BUILD_CONFIGURATION=Release`).

2. **build-push-image**: Sets up QEMU and Docker Buildx, authenticates to GitHub Container Registry, builds the multi-arch image (`linux/amd64`, `linux/arm64/v8`), and pushes to `ghcr.io/jonathanperis/blazor-mudblazor-starter:latest`.

3. **deploy-image-azure**: Deploys the GHCR image to Azure Web App using the `azure/webapps-deploy` action with a publish profile stored in `AZURE_WEBAPP_PUBLISH_PROFILE` secret.

### codeql.yml

Runs CodeQL security analysis on the codebase.

### deploy.yml (GitHub Pages)

Triggered on push to `main` or manual dispatch. Deploys the static `docs/` directory to GitHub Pages using `actions/configure-pages`, `actions/upload-pages-artifact`, and `actions/deploy-pages`.

---

## Azure Web App

The application is deployed to Azure App Service in the Brazil South region. The deployment uses a container image from GHCR, configured via the Azure Web App publish profile.

**Live demo:** [blazor-mudblazor-starter](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/)

### Azure Deployment Requirements

- An Azure Web App configured for Linux container deployment
- The `AZURE_WEBAPP_PUBLISH_PROFILE` secret set in the GitHub repository settings (download from Azure Portal > Web App > Deployment Center > Manage publish profile)
- GHCR image access configured on the Azure Web App (the image is public via GitHub Packages)
