# Blazor learning sandbox

A hands-on toolbox for learning Blazor Server and MudBlazor. Every lab is a small working example with source, an explanation, a common mistake, and something to try.

## Start here

1. [Run locally](./getting-started/).
2. [Choose a learning path](./learning-path/).
3. [Explore the lab reference](./components/).
4. [Run tests and measurements](./testing/).

[Live lab catalog](https://blazor-mudblazor-starter-hmdqebc9f4eneeep.brazilsouth-01.azurewebsites.net/labs) · [Source repository](https://github.com/jonathanperis/blazor-mudblazor-starter)

## How to learn here

Predict what an interaction will do. Try it. Read the implementation. Change one thing and compare. Use reset controls to return to a known dataset or state.

The default application runs locally using synthetic forecasts and SQLite. Cloud credentials are optional. Authentication uses clearly labeled local demo personas; the Azure deployment disables them.

## Map of the guide

| Topic | Purpose |
|---|---|
| [Configuration](./configuration/) | Runtime, culture, storage, API base URL and optional telemetry |
| [Docker and Azure](./deployment/) | Supported publishing modes and an optional deployment exercise |
| [Documentation site](./documentation/) | Add pages and verify links |
| [Project structure](./project-structure/) | Find the implementation and tests |

Delivery checks include behavior tests, dependency review, container scanning, and documentation checks. Renovate maintains package updates. These checks provide specific evidence; a green health endpoint alone does not prove that a UI interaction works.
