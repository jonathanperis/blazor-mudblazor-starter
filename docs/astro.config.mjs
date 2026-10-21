import { defineConfig } from 'astro/config';
import { satteri } from '@astrojs/markdown-satteri';
import sitemap from '@astrojs/sitemap';

export default defineConfig({
  integrations: [sitemap()],
  markdown: {
    processor: satteri(),
    // Token colors only; backgrounds come from the site palette. defaultColor: false emits --shiki-light/--shiki-dark.
    shikiConfig: { themes: { light: 'github-light-high-contrast', dark: 'vitesse-dark' }, defaultColor: false },
  },
  output: 'static',
  outDir: 'out',
  // The custom domain serves this project at the host root. A repository
  // base path would make every asset 404 once GitHub Pages leaves
  // jonathanperis.github.io/blazor-mudblazor-starter/.
  site: 'https://blazor-mudblazor-starter.jonathanperis.tech',
  base: '',
});
