# Getting started

## Try it without installing

Open the [live WebAssembly demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/). It runs the same labs in your browser; the API, notebook, sign-in, culture and diagnostics labs use labeled browser stand-ins. Run the server app below for the full experience.

## Prerequisites

- Install the .NET SDK specified in the repository's `global.json`.
- Docker is optional.
- Node.js, Bun, and Python 3 are needed only for the documentation-site workflow. See the [documentation guide](../documentation/) for current toolchain versions; Python 3 also runs the optional application HTTP smoke check.

## Run the app

```sh
git clone https://github.com/jonathanperis/blazor-mudblazor-starter.git
cd blazor-mudblazor-starter
dotnet restore --locked-mode
dotnet run --project src/WebClient
```

Open **http://localhost:5000/labs**. Start with the counter and keep the source open beside the application.

| Profile | Command | Address |
|---|---|---|
| HTTP | `dotnet run --project src/WebClient` | `http://localhost:5000` |
| HTTPS | `dotnet run --project src/WebClient --launch-profile https` | `https://localhost:5001` |

If your local HTTPS certificate is not trusted, follow the .NET SDK's development-certificate instructions. Clipboard support requires a secure context; localhost is normally treated as trustworthy.

## First experiment

1. Open `/counter` and increment both counts.
2. Visit `/labs/forms`, then return.
3. Observe that component state resets while scoped (circuit) state survives.
4. Reload the page and observe a new circuit.

In the WebAssembly demo there is no circuit: the scoped count lives for the browser tab's app instance, and a reload resets it too.

Continue with [learning paths](../learning-path/).

## Run a container

```sh
docker build -t blazor-learning -f src/WebClient/Dockerfile src/
docker run --rm -p 5000:5000 -v learning-data:/app/App_Data blazor-learning
```

The named volume retains notebook data and workspace-protection keys. Demo login is disabled in the default container; use the Development launch profile to study authentication locally.

## Run the WebAssembly host

```sh
dotnet run --project src/WebClient.Wasm
```

Open **http://localhost:5100/**. This is the host behind the live demo. Compare a lab in both hosts, for example the notebook, and read the lab's host note.

## Common setup questions

- **SDK not found:** `dotnet --list-sdks` must include the feature band in `global.json`.
- **API lab cannot connect:** its typed client uses `Learning:ApiBaseUrl`, defaulting to port 5000. Set it if you change the app's HTTP port.
- **Notes disappeared:** retain both the workspace cookie and the data directory. Clearing cookies gives you a new workspace.
- **A dependency update fails locked restore:** update and review the appropriate lockfile, then run the verification commands.

See [Configuration](../configuration/) and [Testing](../testing/) for details.
