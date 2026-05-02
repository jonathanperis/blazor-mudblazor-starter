# Getting Started

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (9.0.202 or later)
- [Docker](https://www.docker.com/) (optional, for container builds)

## Run Locally

```bash
git clone https://github.com/jonathanperis/blazor-mudblazor-starter.git
cd blazor-mudblazor-starter
dotnet restore
dotnet run --project src/WebClient
```

Open `http://localhost:5000` in your browser.

The `https` launch profile is also available at `https://localhost:5001`.

## Run with Docker

Build the image from the `src/` context using the Dockerfile inside `src/WebClient/`:

```bash
docker build -t blazor-mudblazor -f src/WebClient/Dockerfile src/
docker run -p 5000:5000 blazor-mudblazor
```

Open `http://localhost:5000` in your browser.

### Docker Build Arguments

You can pass build arguments to control optimization:

```bash
docker build \
  --build-arg AOT=false \
  --build-arg TRIM=true \
  --build-arg EXTRA_OPTIMIZE=true \
  --build-arg BUILD_CONFIGURATION=Release \
  -t blazor-mudblazor -f src/WebClient/Dockerfile src/
```

See [Configuration](configuration) for details on each build argument.

## Access URLs

| Context | URL |
|---|---|
| Local (HTTP) | `http://localhost:5000` |
| Local (HTTPS) | `https://localhost:5001` |
| Docker container | `http://localhost:5000` |
| Live demo | [blazor-mudblazor-starterL(https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/.md) |
