# Documentation Site

The public documentation is an Astro static site in `docs/` and is published to GitHub Pages by `.github/workflows/deploy.yml`.

## Authoring Model

| Area | Purpose |
|---|---|
| `docs/wiki/*.md` | Markdown source pages rendered under `/docs/` |
| `docs/src/lib/sidebar.config.ts` | Navigation order and section grouping for Markdown pages |
| `docs/src/components/home/` | Custom landing page sections for the GitHub Pages root |
| `docs/astro.config.mjs` | Astro configuration, static output, base path, sitemap, Tailwind, and Markdown processor |
| `docs/out/` | Generated static output from `npm run build` |

## Local Commands

Use Bun from the `docs/` directory:

```bash
cd docs
bun install
npm run build
npm run check:rendered
```

Astro 7 requires Node.js 22.12 or newer. Bun remains the lockfile/install path, but build, preview, and validation commands should run through `npm run ...` so Astro uses the Node.js 22 runtime expected by the reusable Pages workflow.

Run the source-backed drift check from the repository root:

```bash
python3 scripts/check-docs-drift.py
```

## Adding or Renaming a Page

1. Add or rename the Markdown file in `docs/wiki/`.
2. Add the page slug to `SECTION_CATEGORIES` in `docs/src/lib/sidebar.config.ts`.
3. Link to the page with root-relative docs routes such as `../configuration/` from another docs page, or `/blazor-mudblazor-starter/docs/configuration/` when authoring absolute public links.
4. Run `npm run build`, `npm run check:rendered`, and `python3 scripts/check-docs-drift.py`.

## Markdown Processor

The site uses Astro 7 with the default Rust `.astro` compiler, Vite 8/Rolldown bundling, queued rendering, and the Rust-based Sätteri Markdown processor:

```js
import { satteri } from '@astrojs/markdown-satteri';

export default defineConfig({
  markdown: {
    processor: satteri(),
  },
});
```

Sätteri stays explicit so the rendered HTML smoke test protects the Markdown features this site depends on: routes, headings and anchors, tables, fenced code blocks, and internal links. Astro 7's stable request routing and cache APIs are not configured here because the docs deploy is a fully static GitHub Pages artifact, so there is no server/CDN adapter surface to cache or route dynamically.

## Deployment

`.github/workflows/deploy.yml` delegates to the reusable `jonathanperis/.github/.github/workflows/pages-docs-deploy.yml@main` workflow with inherited secrets. The reusable workflow installs the docs dependencies, builds the Astro site, and publishes the generated Pages artifact.
