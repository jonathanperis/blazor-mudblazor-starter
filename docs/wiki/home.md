# Blazor learning sandbox

A hands-on toolbox for learning Blazor and MudBlazor. Every lab is a small working example with source, an explanation, a common mistake, and something to try. The same labs run in two hosts: Blazor Server locally, and a static [WebAssembly live demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/) in your browser.

## Start here

1. [Open the live demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/) or [run locally](./getting-started/).
2. [Choose a learning path](./learning-path/).
3. [Explore the lab reference](./components/).
4. [Run tests and measurements](./testing/).

[Source repository](https://github.com/jonathanperis/blazor-mudblazor-starter) · [Live WebAssembly demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/) · Run `/labs` locally for the Blazor Server host. Hostinger setup is pending; no server deployment is configured.

## How to learn here

Predict what an interaction will do. Try it. Read the implementation. Change one thing and compare. Use reset controls to return to a known dataset or state.

The server application runs locally using synthetic forecasts and SQLite. Cloud credentials are optional. Authentication uses clearly labeled demo personas enabled only in Development by default.

The live demo needs no installation. It runs .NET in the browser, so labs that need a server use labeled stand-ins: an in-browser API handler, a localStorage notebook, browser-only personas, and console logging. Each lab says what differs. Run the server host to see real HTTP, SQLite, cookies and server-enforced policies.

## Map of the guide

| Topic | Purpose |
|---|---|
| [Configuration](./configuration/) | Runtime, culture, storage, API base URL and optional telemetry |
| [Docker and hosting](./deployment/) | Publishing modes, the release flow, the Pages demo and the pending Hostinger setup |
| [Documentation site](./documentation/) | Add pages and verify links |
| [Project structure](./project-structure/) | Find the implementation and tests |

Delivery checks include behavior tests, dependency review, container scanning, documentation checks, and a browser check of the live demo. Renovate maintains package updates. These checks provide specific evidence; a green health endpoint alone does not prove that a UI interaction works.
