# Documentation site

The guide is an Astro 7 static site with the Sätteri Markdown processor, Vite 8, and TypeScript 7. GitHub Pages hosts the built HTML; no server adapter is required. The [maintainer README](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/README.md) maps the source directories and commands.

## Commands

From `docs/`, use Node.js 26.9.0 from [`.node-version`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/.node-version), Bun 1.4.2 from [`package.json`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/package.json), and Python 3. Astro's supported minimum is Node.js 22.12. Check the selected executables with `node --version`, `bun --version`, and `python3 --version`.

```sh
bun install --frozen-lockfile
npm run check:drift
npm run check:types
npm run build
npm run check:rendered
bun audit
```

Bun manages the committed lockfile. `npm run` uses the Node executable on PATH for the Astro CLI; it does not select a runtime version. Optional `PUBLIC_GA_ID` enables analytics in the generated site; omit it for a local-only guide.

`npm run dev` serves the overview at `http://localhost:4321/` and the guide at `/docs/`. After building, `npm run preview` serves the production paths under `http://localhost:4321/blazor-mudblazor-starter/`. Use the actual port printed by Astro if 4321 is busy.

## Authoring

- Add a Markdown file under `wiki/`.
- Add its label and topic-specific description to `PAGE_METADATA` in [`src/lib/sidebar.config.ts`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/docs/src/lib/sidebar.config.ts), and its slug to a navigation category.
- Each page is rendered once, with its own heading IDs. The docs root renders the overview.
- Use `../configuration/` from a standalone docs page, and `./configuration/` from the overview.
- These are rendered-site links, not GitHub Markdown-file links. Navigate the published guide to follow them; use full GitHub `blob/main/` URLs when linking implementation files.
- Keep source versions in the README/configuration reference rather than repeating them in marketing copy.

The build validates sidebar/page coverage. The source drift check compares SDK/package facts, toolchain pins, landing-page stack badges and locked-restore instructions, lab routes, and release-job documentation with their sources. Rendered checks validate every generated route, product identity, distinct topic descriptions, canonical URLs, unique IDs, local links/anchors/assets, repository source-file links, code blocks, tables, and complete sitemap coverage.

`check:types` checks TypeScript files and configuration; the Astro build validates template compilation. These offline checks do not execute browser interactions or request external URLs. Audit GitHub About and the deployed site separately when product identity, dependencies, or hosting changes.

## Navigation and accessibility

The sidebar highlights the current page and provides topic filtering with a result announcement. On mobile, closed navigation is inert. Opening it moves focus, traps keyboard focus within navigation, and makes content inert; Escape closes it and restores focus. A skip link targets the main content.

The shared styles support light/dark schemes, visible focus indicators, table/code overflow, and reduced-motion preferences. Manual browser/assistive-technology verification remains a separate test layer.

## Dependency updates

Update direct dependency ranges deliberately, then run `bun update` to refresh the compatible transitive graph. Review `bun.lock`, run the checks above, and confirm `bun install --frozen-lockfile` succeeds. The previous smol-toml, PostCSS, and nanoid overrides were removed after the refreshed upstream graph passed the vulnerability audit. Do not force a transitive dependency across its consumer's major-version range; PostCSS, for example, consumes nanoid 3.

## Publishing

[`deploy.yml`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/.github/workflows/deploy.yml) calls the shared `pages-docs-deploy.yml` workflow at a reviewed full commit SHA and passes the selected Node version. PR validation runs the docs checks before merge. The shared publishing workflow installs and builds independently; it selects the latest Bun release, while PR CI pins the version recorded in `package.json`.
