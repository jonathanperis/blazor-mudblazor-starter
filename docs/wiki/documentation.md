# Documentation site

The guide is an Astro 7 static site with the Sätteri Markdown processor and Vite 8. GitHub Pages hosts the built HTML; no server adapter is required.

## Commands

Use Node.js 22.12+ and Bun from `docs/`:

```sh
bun install --frozen-lockfile
npm run check:drift
npm run build
npm run check:rendered
bun audit
```

Bun manages the committed lockfile. `npm run` uses Node for the Astro CLI. Optional `PUBLIC_GA_ID` enables analytics in the generated site; omit it for a local-only guide.

## Authoring

- Add a Markdown file under `wiki/`.
- Add its slug and label to `src/lib/sidebar.config.ts`.
- Each page is rendered once, with its own heading IDs. The docs root renders the overview.
- Use `../configuration/` from a standalone docs page, and `./configuration/` from the overview.
- Keep source versions in the README/configuration reference rather than repeating them in marketing copy.

The build validates sidebar coverage. Rendered checks validate all generated pages, unique IDs, links/anchors, code blocks, tables, and the sitemap advertised by robots.txt. The source drift check compares package/SDK facts and release-job documentation against implementation.

## Navigation and accessibility

The sidebar highlights the current page and provides topic filtering with a result announcement. On mobile, closed navigation is inert. Opening it moves focus, traps keyboard focus within navigation, and makes content inert; Escape closes it and restores focus. A skip link targets the main content.

The shared styles support light/dark schemes, visible focus indicators, table/code overflow, and reduced-motion preferences. Manual browser/assistive-technology verification remains a separate test layer.

## Dependency updates

The manifest contains narrow same-major transitive overrides for smol-toml, PostCSS, and nanoid because upstream ranges/locks previously selected vulnerable versions. Revisit these overrides when upstream dependencies catch up; keep the audit and rendered checks passing.

## Publishing

`.github/workflows/deploy.yml` calls the shared `pages-docs-deploy.yml` workflow at a reviewed full commit SHA. PR validation builds and checks docs before they can reach that deployment path.
