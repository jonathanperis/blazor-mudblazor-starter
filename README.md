# blazor-mudblazor-starter

**Blazor Server** starter template with **MudBlazor** Material Design components — built on **.NET 9** with Docker support and Azure deployment.

**Live demo:** [blazor-mudblazor-starter](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/)

---

## About

A ready-to-use starter template for building interactive web applications with Blazor Server and MudBlazor. Comes pre-configured with demo pages, Docker containerization, and a CI/CD pipeline for Azure deployment.

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 9 | Runtime and SDK |
| Blazor Server | Interactive server-side rendering |
| MudBlazor 8.3 | Material Design UI components |
| Docker | Multi-stage build (AMD64 + ARM64) |
| GitHub Actions | CI/CD to Docker Hub + Azure Web App |
| AOT + Trimming | Optimized production builds |

## Features

- Pre-configured MudBlazor layout with navigation and breadcrumbs
- Demo pages: Home, Counter, Weather (with CRUD dialogs)
- Multi-architecture Docker image (AMD64 + ARM64)
- Production-optimized with AOT compilation and trimming
- CI/CD pipeline: build, push to Docker Hub, deploy to Azure

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run Locally

```bash
dotnet restore
cd src/WebClient
dotnet run
```

### Run with Docker

```bash
docker build -t blazor-mudblazor -f src/WebClient/Dockerfile .
docker run -p 5000:5000 blazor-mudblazor
```

## Project Structure

```
blazor-mudblazor-starter/
├── src/WebClient/
│   ├── Program.cs              # Blazor Server setup + MudBlazor services
│   ├── Components/
│   │   ├── Layout/             # MainLayout, Breadcrumb
│   │   ├── Pages/              # Home, Counter, Weather, Error
│   │   └── Weather/            # Add, Edit, Remove dialogs
│   ├── Dockerfile              # Multi-stage .NET 9 build
│   └── WebClient.csproj
├── .github/workflows/          # CI/CD pipeline
└── WebClient.sln
```

## License

Licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.
