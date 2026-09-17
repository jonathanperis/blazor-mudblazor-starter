# Documentation site

The [published learning guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/) and [project overview](https://jonathanperis.github.io/blazor-mudblazor-starter/) are an Astro static site deployed to GitHub Pages. The interactive Blazor labs run separately; see the [application README](../README.md).

## Prerequisites

- **Node.js 26.9.0**, pinned in [`.node-version`](.node-version), is used by PR checks and the Pages build. Astro's supported minimum remains Node.js 22.12.
- **Bun 1.4.2**, recorded in [`package.json`](package.json), installs and locks dependencies. PR CI pins this version; the shared Pages installer selects the latest Bun release.
- **Python 3** runs the source and rendered-output checks.

Select these tools with your preferred version manager, then check `node --version`, `bun --version`, and `python3 --version`. `npm run` uses the Node executable on PATH; it does not select a Node version itself.

## Commands

Run from this directory (`docs/`):

| Command | Action |
|---|---|
| `bun install --frozen-lockfile` | Install the locked dependencies |
| `npm run dev` | Start the Astro dev server at `http://localhost:4321/` |
| `npm run build` | Build to `./out/` using Astro 7, Vite 8, and the Rust compiler |
| `npm run preview` | Serve the built site at `http://localhost:4321/blazor-mudblazor-starter/` |
| `npm run check:drift` | Verify README/wiki source-backed facts against current code and workflows |
| `npm run check:types` | Check TypeScript source and configuration with TypeScript 7; does not type-check Astro templates |
| `npm run check:rendered` | Verify built routes, metadata, links, assets, source references, Markdown, and sitemap coverage |
| `bun audit` | Check the locked dependency tree for known vulnerabilities |

Run `npm run build` before previewing or checking rendered HTML. Development has no repository path prefix: its guide is at `/docs/`. Production and preview use `/blazor-mudblazor-starter/docs/`. If port 4321 is busy, use the address Astro prints.

## Source map and authoring

| Path | Responsibility |
|---|---|
| `wiki/*.md` | Learning guide content; `home.md` renders the guide root |
| `src/lib/sidebar.config.ts` | Topic labels, unique descriptions, navigation groups and order |
| `src/pages/docs/[...slug].astro` | One static route per topic, sidebar and mobile navigation |
| `src/pages/index.astro`, `src/components/home/` | Project landing page |
| `src/layouts/BaseLayout.astro` | Shared HTML head, product metadata and optional analytics |
| `src/styles/`, `public/` | Styles, icons and robots.txt |
| `astro.config.mjs` | Sätteri processor, sitemap, production base path and output directory |
| `out/` | Generated output; ignored by Git |
| `../scripts/check-docs-*.py` | Offline source and rendered-output verification |

Add a Markdown topic, its `PAGE_METADATA` entry and its navigation category entry together. Wiki links target **site routes**, such as `../configuration/`, rather than `.md` files; read the published guide when navigating topics from GitHub. Use full GitHub `blob/main/` URLs for implementation source links. See the [authoring guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/documentation/) for verification boundaries and dependency maintenance.

## Publishing

[`deploy.yml`](../.github/workflows/deploy.yml) calls a reviewed, commit-pinned shared Pages workflow and passes the Node version. PR checks run the docs checks before merge; the shared publishing workflow installs and builds the site independently. Repository About metadata and external URL availability require a separate read-only audit.

## Environment

Copy `.env.example` to `.env` and fill in local values when needed.

| Variable | Description |
|---|---|
| `PUBLIC_GA_ID` | Optional Google Analytics 4 Measurement ID |
