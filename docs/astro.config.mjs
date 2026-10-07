import { defineConfig } from 'astro/config';
import { satteri } from '@astrojs/markdown-satteri';
import sitemap from '@astrojs/sitemap';

const isProd = process.env.NODE_ENV === 'production';

export default defineConfig({
  integrations: [sitemap()],
  markdown: {
    processor: satteri(),
    // Token colors only; backgrounds come from the site palette. defaultColor: false emits --shiki-light/--shiki-dark.
    shikiConfig: { themes: { light: 'vitesse-light', dark: 'vitesse-dark' }, defaultColor: false },
  },
  output: 'static',
  outDir: 'out',
  site: 'https://jonathanperis.github.io',
  base: isProd ? '/blazor-mudblazor-starter' : '',
});
