# Docs

Astro static site deployed to GitHub Pages.

## Commands

Run from this directory (`docs/`):

| Command | Action |
|---|---|
| `bun install` | Install dependencies and update `bun.lock` |
| `npm run dev` | Start the Astro 7 dev server with Node.js 22.12+ |
| `npm run build` | Build to `./out/` using Astro 7, Vite 8, and the Rust compiler |
| `npm run preview` | Preview production build locally with Node.js 22.12+ |
| `npm run check:drift` | Verify README/wiki source-backed facts against current code and workflows |
| `npm run check:rendered` | Verify generated Astro HTML routes, anchors, tables, code blocks, and critical links |

Astro 7 requires Node.js 22.12 or newer. Keep Bun for dependency locking, but run Astro commands through `npm run ...` so the configured Node.js 22 runtime is used consistently in local and reusable GitHub Pages builds.

## Environment

Copy `.env.example` to `.env` and fill in local values when needed.

| Variable | Description |
|---|---|
| `PUBLIC_GA_ID` | Optional Google Analytics 4 Measurement ID |
