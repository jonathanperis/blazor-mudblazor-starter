# Documentation site

The [published learning guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/) and [project overview](https://jonathanperis.github.io/blazor-mudblazor-starter/) are an Astro static site deployed to GitHub Pages together with the [WebAssembly live demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/) under `/demo/`. The Blazor Server app runs separately; see the [application README](../README.md).

## Prerequisites

- **Node.js 26.9.0**, pinned in [`.node-version`](.node-version), is used by PR checks and the Pages build. Astro's supported minimum remains Node.js 22.12.
- **Bun 1.4.2**, recorded in [`package.json`](package.json), installs and locks dependencies. CI reads this version from `packageManager` for both validation and the Pages build.
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
| `npm run check:rendered` | Verify built routes, metadata, links, assets, source references, Markdown, the 404 page, the demo entry point, and sitemap coverage |
| `npm run check:demo` | Drive the WebAssembly demo in Chromium against a local server that behaves like GitHub Pages |
| `bun audit` | Check the locked dependency tree for known vulnerabilities |

Run `npm run build` before previewing or checking rendered HTML. To include the demo, publish it from the repository root (`dotnet publish src/WebClient.Wasm -c Release -o artifacts/wasm`) and run `python3 ../scripts/prepare-pages-demo.py` after each build; `check:demo` needs a Chromium from `npx playwright install chromium` or `PLAYWRIGHT_CHANNEL=chrome`. Development has no repository path prefix: its guide is at `/docs/`. Production and preview use `/blazor-mudblazor-starter/docs/`. If port 4321 is busy, use the address Astro prints.

## Source map and authoring

| Path | Responsibility |
|---|---|
| `wiki/*.md` | Learning guide content; `home.md` renders the guide root |
| `src/lib/sidebar.config.ts` | Topic labels, unique descriptions, navigation groups and order |
| `src/pages/docs/[...slug].astro` | One static route per topic, sidebar and mobile navigation |
| `src/pages/index.astro` | Landing page; its contents are read from the app's catalogs at build time |
| `src/lib/catalog.ts` | Build-time reader for `LabCatalog.cs`, gallery pages and page samples |
| `src/components/` | Masthead, colophon and optional analytics |
| `src/styles/site.css`, `src/fonts/` | The lab-manual design system and self-hosted fonts (OFL licenses in `public/fonts/`) |
| `public/screens/` | Plates on the landing page; regenerate with `node scripts/capture-screens.mjs <app url>` |
| `src/pages/404.astro` | Site-wide not-found page; redirects demo deep links into the WebAssembly app |
| `scripts/check-demo.mjs` | Browser check of the published demo |
| `src/layouts/BaseLayout.astro` | Shared HTML head, product metadata and optional analytics |
| `public/` | Icons, font licenses and screenshots |
| `astro.config.mjs` | Sätteri processor, sitemap, production base path and output directory |
| `out/` | Generated output, including `out/demo/` when prepared; ignored by Git |
| `../scripts/check-docs-*.py` | Offline source and rendered-output verification |

Add a Markdown topic, its `PAGE_METADATA` entry and its navigation category entry together. Wiki links target **site routes**, such as `../configuration/`, rather than `.md` files; read the published guide when navigating topics from GitHub. Use full GitHub `blob/main/` URLs for implementation source links. See the [authoring guide](https://jonathanperis.github.io/blazor-mudblazor-starter/docs/documentation/) for verification boundaries and dependency maintenance.

## Publishing

The Build Check workflow's `pages` job builds the guide, adds the WebAssembly demo, and runs every check. On `main` it uploads that exact site, and [`main-release.yml`](../.github/workflows/main-release.yml) deploys it. Repository About metadata and external URL availability require a separate read-only audit.

## Environment

Copy `.env.example` to `.env` and fill in local values when needed.

| Variable | Description |
|---|---|
| `PUBLIC_GA_ID` | Optional Google Analytics 4 measurement ID (`G-…`); loads analytics on every guide page without a consent banner |
