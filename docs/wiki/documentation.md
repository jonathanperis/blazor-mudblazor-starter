# Documentation site

The guide is an Astro 7 static site with the Sätteri Markdown processor, Vite 8, and TypeScript 7. GitHub Pages hosts the built HTML together with the WebAssembly live demo under `/demo/`; no server adapter is required. The [maintainer README](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/README.md) maps the source directories and commands.

## Commands

From `docs/`, use Node.js 26.9.0 from [`.node-version`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/.node-version), Bun 1.4.2 from [`package.json`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/package.json), and Python 3. Astro's supported minimum is Node.js 22.12. Check the selected executables with `node --version`, `bun --version`, and `python3 --version`.

```sh
bun install --frozen-lockfile
npm run check:drift
npm run check:types
npm run build
npm run check:rendered
npm run check:demo
bun audit
```

`check:demo` needs the demo in `out/demo/`; see the [testing guide](../testing/) for the publish step.

Bun manages the committed lockfile. `npm run` uses the Node executable on PATH for the Astro CLI; it does not select a runtime version. Optional `PUBLIC_GA_ID` enables analytics in the generated site; omit it for a local-only guide.

`npm run dev` serves the overview at `http://localhost:4321/` and the guide at `/docs/`. After building, `npm run preview` serves the production paths under `http://localhost:4321/blazor-mudblazor-starter/`. Use the actual port printed by Astro if 4321 is busy.

## Authoring

- Add a Markdown file under `wiki/`.
- Add its label and topic-specific description to `PAGE_METADATA` in [`src/lib/sidebar.config.ts`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/src/lib/sidebar.config.ts), and its slug to a navigation category.
- Each page is rendered once, with its own heading IDs. The docs root renders the overview.
- Use `../configuration/` from a standalone docs page, and `./configuration/` from the overview.
- These are rendered-site links, not GitHub Markdown-file links. Navigate the published guide to follow them; use full GitHub `blob/main/` URLs when linking implementation files.
- Keep source versions in the README/configuration reference rather than repeating them in marketing copy.

The build validates sidebar/page coverage. The source drift check compares SDK/package facts, toolchain pins, landing-page stack badges and locked-restore instructions, lab routes, and release-job documentation with their sources. Rendered checks validate every generated route, product identity, unique titles, distinct topic descriptions, canonical URLs, document language, one current-page link per guide page, unique IDs, local links/anchors/assets (including links that escape the repository base path), repository source-file links, code blocks, tables, the 404 page, the demo entry point, and complete sitemap coverage.

`check:types` checks TypeScript files and configuration; the Astro build validates template compilation. These offline checks do not request external URLs; `check:demo` is the only browser check. Audit GitHub About and the deployed site separately when product identity, dependencies, or hosting changes.

## Navigation and accessibility

Guide pages use a book layout: numbered contents on the left (the current page is marked with `aria-current`), the article, and an "On this page" list built from the page's `h2` headings on wide screens, with previous and next links at the end. On small screens the contents collapse behind a native `<details>` disclosure, so keyboard and screen-reader behavior comes from the browser; without JavaScript the contents stay open. A skip link targets the main content.

The landing page reads the lab catalog, the gallery pages and the page samples from the application source at build time (`src/lib/catalog.ts`), so its contents cannot drift from the app. Links into the demo are checked against the app's real routes by `check:rendered`.

The design matches the application: warm paper, ink and one vermilion accent, Fraunces for display type and IBM Plex for text and code. The shared styles support light/dark schemes (code blocks use Shiki's dual themes), visible focus indicators, table/code overflow, and reduced-motion preferences. Wide tables are wrapped in a focusable scroll region that keeps native table semantics, and overflowing code blocks become focusable so keyboard users can scroll them. Fonts are self-hosted from `src/fonts` and bundled by Vite, so the site makes no third-party font requests. Manual browser/assistive-technology verification remains a separate test layer.

## Dependency updates

Update direct dependency ranges deliberately, then run `bun update` to refresh the compatible transitive graph. Review `bun.lock`, run the checks above, and confirm `bun install --frozen-lockfile` succeeds. `bun audit` runs in every pull request. When an advisory has a fix within the consumers' ranges, `bun update` resolves it; use `overrides` in `package.json` only when a consumer pins a vulnerable range, and remove them once upstream catches up. Do not force a transitive dependency across its consumer's major-version range; PostCSS, for example, consumes nanoid 3.

## Analytics and privacy

Setting the `PUBLIC_GA_ID` repository secret loads Google Analytics 4 on every page of the published guide. The ID must look like `G-XXXXXXX`; anything else is ignored. There is no consent banner, so set the secret only if your privacy notice and audience allow it. The demo never loads analytics.

## Publishing

The Build Check `pages` job builds the guide, publishes the WebAssembly demo into `out/demo/`, and runs every check above with the Node and Bun versions pinned in `.node-version` and `package.json`. On `main`, the same job uploads the checked site and the Main Release workflow's `deploy-pages` job deploys it, so the published site is exactly what was validated. See [Docker and hosting](../deployment/) for the deep-link redirect.
