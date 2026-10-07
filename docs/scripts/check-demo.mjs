// Browser check for the WebAssembly demo inside the built Pages site (docs/out).
// Serves the site like GitHub Pages: under the repository path, with 404.html for unknown paths.
// Run after `npm run build` and `python3 ../scripts/prepare-pages-demo.py`.
// Uses Playwright's Chromium; set PLAYWRIGHT_CHANNEL=chrome to use an installed Google Chrome instead.
import { createServer } from 'node:http';
import { readdir, readFile, stat } from 'node:fs/promises';
import { extname, join, normalize } from 'node:path';
import { chromium } from 'playwright';

const root = new URL('../out/', import.meta.url).pathname;
const shared = new URL('../../src/WebClient.Shared/Components/', import.meta.url).pathname;

// Every routable page in the shared library, read from its @page directive.
async function appRoutes(directory = shared) {
  const routes = [];
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) routes.push(...await appRoutes(path));
    else if (entry.name.endsWith('.razor')) routes.push(...[...(await readFile(path, 'utf8')).matchAll(/^@page\s+"([^"]+)"/gm)].map((match) => match[1]));
  }
  return routes;
}
const prefix = '/blazor-mudblazor-starter';
const types = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css', '.json': 'application/json', '.wasm': 'application/wasm', '.dat': 'application/octet-stream', '.png': 'image/png', '.ico': 'image/x-icon', '.xml': 'application/xml' };

async function file(path) {
  try {
    const info = await stat(path);
    return info.isDirectory() ? file(join(path, 'index.html')) : path;
  } catch {
    return null;
  }
}

const server = createServer(async (request, response) => {
  const path = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
  const local = path.startsWith(prefix) ? await file(normalize(join(root, path.slice(prefix.length)))) : null;
  const target = local && local.startsWith(root) ? local : join(root, '404.html');
  response.writeHead(target === local ? 200 : 404, { 'Content-Type': types[extname(target)] ?? 'application/octet-stream' });
  response.end(await readFile(target));
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const base = `http://127.0.0.1:${server.address().port}${prefix}/demo/`;

const browser = await chromium.launch({ channel: process.env.PLAYWRIGHT_CHANNEL || undefined });
const failures = [];
const step = async (name, action) => {
  try {
    await action();
    console.log(`PASS ${name}`);
  } catch (error) {
    failures.push(name);
    console.error(`FAIL ${name}: ${error.message.split('\n')[0]}`);
  }
};

try {
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  page.on('console', message => {
    // The first request for a deep link is answered by 404.html before it redirects into the app.
    if (message.type() === 'error' && !message.text().includes('404')) errors.push(message.text());
  });
  const heading = name => page.getByRole('heading', { level: 1, name });
  const timeout = { timeout: 60_000 };

  await step('deep link boots the app and keeps the route', async () => {
    await page.goto(`${base}labs/api`);
    await heading('API and server paging').waitFor(timeout);
    if (!page.url().endsWith('/demo/labs/api')) throw new Error(`unexpected URL ${page.url()}`);
  });
  await step('typed client pages through the in-browser API', async () => {
    await page.getByRole('button', { name: 'Load page' }).click();
    await page.getByText('Page 1 · 1000 matches').waitFor();
    await page.getByRole('button', { name: 'Next' }).click();
    await page.getByText('Page 2 · 1000 matches').waitFor();
  });
  await step('skip link stays on the page and focuses main content', async () => {
    await page.locator('a.skip-link').focus();
    await page.keyboard.press('Enter');
    await page.waitForFunction(() => document.activeElement?.id === 'main-content');
    if (!page.url().endsWith('/demo/labs/api')) throw new Error(`skip link navigated to ${page.url()}`);
  });
  await step('scoped state survives in-app navigation', async () => {
    await page.getByRole('link', { name: 'State and lifecycle' }).first().click();
    await heading('State and lifecycle').waitFor();
    await page.getByRole('button', { name: 'Increment scoped count' }).click();
    await page.getByRole('link', { name: 'Forms and transactional dialogs' }).first().click();
    await heading('Forms and transactional dialogs').waitFor();
    await page.getByRole('link', { name: 'State and lifecycle' }).first().click();
    await page.locator('[data-testid=scoped-count]', { hasText: '1' }).waitFor();
  });
  await step('notebook stores notes and detects a stale draft', async () => {
    await page.goto(`${base}labs/persistence`);
    await heading('SQLite notebook').waitFor(timeout);
    await page.getByLabel('Title').fill('Browser note');
    await page.getByRole('button', { name: 'Add note' }).click();
    await page.getByRole('heading', { level: 2, name: 'Browser note' }).waitFor();
    await page.getByRole('button', { name: 'Edit draft' }).click();
    await page.evaluate(() => {
      const notes = JSON.parse(localStorage.getItem('learning.notebook'));
      notes[0].version = crypto.randomUUID();
      localStorage.setItem('learning.notebook', JSON.stringify(notes));
    });
    await page.getByRole('button', { name: 'Save draft' }).click();
    await page.getByText('Conflict: another tab changed or deleted this note.').waitFor();
  });
  await step('browser personas drive UI authorization only', async () => {
    await page.getByRole('link', { name: 'Authentication and policies' }).first().click();
    await page.locator('#persona').selectOption('instructor');
    await page.getByRole('button', { name: 'Sign in as demo persona' }).click();
    await page.getByText('Signed in as Demo instructor.').waitFor();
    await page.getByText('Only a server can enforce it').waitFor();
  });
  await step('culture choice applies after reload', async () => {
    await page.getByRole('link', { name: 'Localization and accessibility' }).first().click();
    await page.locator('#culture').selectOption('pt-BR');
    await page.getByRole('button', { name: 'Apply language / Aplicar idioma' }).click();
    await page.getByText('Bem-vindo ao laboratório de localização').waitFor(timeout);
    await page.getByText('Valor: 1.234,56').waitFor();
    if (await page.evaluate(() => document.documentElement.lang) !== 'pt') throw new Error('document language not updated');
    await page.evaluate(() => localStorage.setItem('culture', 'en-US'));
  });
  await step('CSV import validates the whole file in the browser', async () => {
    await page.goto(`${base}labs/files`);
    await heading('Files and cancellable work').waitFor(timeout);
    const header = 'Id,Date,TemperatureC,Summary\n';
    await page.locator('#csv-file').setInputFiles({ name: 'bad.csv', mimeType: 'text/csv', buffer: Buffer.from(`${header}not-a-guid,2026-01-01,20,Mild\n`) });
    await page.getByText('Import rejected').waitFor();
    const row = `${crypto.randomUUID()},2026-01-01,20,"'-5 and windy"\n\n`;
    await page.locator('#csv-file').setInputFiles({ name: 'good.csv', mimeType: 'text/csv', buffer: Buffer.from(header + row) });
    await page.getByText('Imported 1 rows.').waitFor();
    await page.getByRole('cell', { name: '-5 and windy', exact: true }).waitFor();
  });
  await step('the largest grid dataset generates in the browser', async () => {
    await page.goto(`${base}weather`);
    await heading('DataGrid experiments').waitFor(timeout);
    await page.getByRole('combobox', { name: 'Dataset size' }).click();
    await page.getByText('69,420', { exact: true }).click();
    await page.getByRole('button', { name: 'Generate dataset' }).click();
    await page.getByText('69,420 records in component memory').waitFor(timeout);
  });
  await step('unknown routes render the not-found page', async () => {
    await page.goto(`${base}no-such-lab`);
    await heading('Page not found').waitFor(timeout);
  });
  await step('every page renders without an error boundary', async () => {
    const routes = (await appRoutes()).filter((route) => route !== '/not-found' && route !== '/Error').sort();
    await page.goto(base);
    await page.getByRole('heading', { level: 1 }).waitFor(timeout);
    const broken = [];
    for (const route of routes) {
      // In-app navigation keeps one runtime, so the sweep covers every page in seconds.
      await page.evaluate((target) => window.Blazor.navigateTo(target), route.replace(/^\//, ''));
      try {
        await page.waitForFunction((target) => location.pathname.endsWith(target === '/' ? '/demo/' : target) && document.querySelectorAll('h1').length === 1, route, { timeout: 15_000 });
        await page.waitForTimeout(150);
        if (await page.getByText('failed to render').count()) broken.push(`${route}: example failed to render`);
      } catch {
        broken.push(`${route}: no single h1`);
      }
    }
    if (broken.length) throw new Error(broken.join('; '));
    console.log(`  ${routes.length} routes`);
  });
  if (errors.length) {
    failures.push('console');
    console.error(`FAIL browser console errors:\n${errors.join('\n')}`);
  }
} finally {
  await browser.close();
  server.close();
}

if (failures.length) {
  console.error(`WebAssembly demo browser check failed: ${failures.join(', ')}`);
  process.exit(1);
}
console.log('WebAssembly demo browser checks passed');
