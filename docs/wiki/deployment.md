# Docker and Azure

## Local container exercise

```sh
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
docker run --rm -p 5000:5000 -v learning-data:/app/App_Data blazor-learning
```

The image uses a supported .NET 10 SDK/runtime, runs as `app`, and listens on port 5000. The `src/.dockerignore` excludes local build artifacts, database files, and environment files.

`BUILD_CONFIGURATION` defaults to `Release`. Set `READY_TO_RUN=true` to compare ReadyToRun publishing with the default framework-dependent image. Both modes keep culture and diagnostics support.

`ghcr.io/jonathanperis/blazor-mudblazor-starter:latest` is a convenience image tag. Use a commit tag or manifest digest when you need to reproduce an exact image.

## Data lifetime in a deployment

SQLite and workspace-protection keys must be retained together. A Docker named volume does this locally. The sample Azure container is disposable (`WEBSITES_ENABLE_APP_SERVICE_STORAGE=false`), so notebook access is not guaranteed across container replacement. Treat it as a learning workspace, not an account-backed notebook service.

For a durable multi-instance application, move persistence and key storage to suitable shared services, replace demo identities with a real identity provider, and revisit circuit routing and concurrency. The current component-owned work example stops on disposal; a durable job needs persisted state and a hosted worker/queue.

## PR validation

`build-check.yml` runs behavioral tests, locked dependency restore/audit, published-app HTTP smoke checks, documentation build/link/drift checks, dependency review, infrastructure compilation, workflow linting, and a Docker matrix for both ReadyToRun values. Trivy scans the default image for high/critical vulnerabilities.

## Release flow

`main-release.yml` has three jobs:

1. **validate** calls the same reusable Build Check workflow.
2. **publish** builds linux/amd64 and linux/arm64 in one manifest and publishes `sha-<commit>` plus `latest`. Its digest is the deployment input.
3. **deploy** is optional. It logs in through OIDC and deploys Bicep with `containerImage=<image>@<digest>`. Updating the container configuration selects the exact published image.

Release runs are serialized with cancellation disabled. Package write permission belongs only to publishing; OIDC permission belongs only to deployment.

## Optional Azure setup

The template provisions an App Service Plan, Web App, Log Analytics workspace, and Application Insights. It defaults to B1 in Brazil South; review the chosen SKU and 90-day log retention as part of the cost exercise.

Before enabling the workflow:

1. Create your resource group and an Azure identity allowed to deploy the resources into it.
2. Configure a federated credential for GitHub environment `azure-sandbox`, with subject `repo:OWNER/REPO:environment:azure-sandbox` and audience `api://AzureADTokenExchange`.
3. Create the GitHub environment and apply the approval rules appropriate to your own deployment.
4. Supply the configuration below. Make the GHCR image publicly pullable, or configure private-registry access separately.

| GitHub setting | Purpose |
|---|---|
| Variable `AZURE_DEPLOY_ENABLED` | Set exactly `true` to enable deployment on main |
| Variable `AZURE_RESOURCE_GROUP` | Existing target resource group |
| Variable `AZURE_WEBAPP_NAME` | Your unique Web App name |
| Variable `AZURE_APP_SERVICE_PLAN` | Plan to provision/manage |
| Variable `AZURE_LOCATION` | Optional; defaults to `brazilsouth` |
| Secret `AZURE_CLIENT_ID` | Federated identity's client ID |
| Secret `AZURE_TENANT_ID` | Azure tenant |
| Secret `AZURE_SUBSCRIPTION_ID` | Target subscription |

No publish-profile secret is used. The workflow overrides the maintainer-specific names in `infra/main.bicepparam`; when deploying manually, provide your own parameter file or overrides.

The Web App declares `WEBSITES_PORT=5000`, WebSockets, client affinity, TLS 1.2+, HTTPS-only access, disabled FTPS, and `/healthz/ready`. Always-on follows whether the selected plan is F1. Demo sign-in is explicitly disabled. `APPLICATIONINSIGHTS_CONNECTION_STRING` connects SDK telemetry to the provisioned resource.

## Validate infrastructure without deploying

```sh
az bicep build --file infra/main.bicep --stdout
az bicep build-params --file infra/main.bicepparam --stdout
```

The checked-in `infra/main.json` is generated from Bicep. Regenerate it after template changes. Compilation proves template validity, not subscription permissions or a successful live deployment.

After your exercise, inspect and remove only the resources you created. Resource-group deletion also removes all unrelated resources in that group, so use a dedicated group for an isolated exercise.

## Documentation deployment

`deploy.yml` delegates to `jonathanperis/.github/.github/workflows/pages-docs-deploy.yml@main`. It installs from the Bun lockfile and publishes the static site. See [Documentation site](../documentation/).
