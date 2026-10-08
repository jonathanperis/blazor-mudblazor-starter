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

// Like GitHub Pages: a file, then the same path with .html (prerendered routes), then a folder's index.html.
async function file(path, extension = true) {
  try {
    const info = await stat(path);
    if (!info.isDirectory()) return path;
  } catch {
    // Not a file or folder under that exact name.
  }
  if (extension && !path.endsWith('/') && await file(`${path}.html`, false)) return `${path}.html`;
  try {
    return (await stat(join(path, 'index.html'))).isFile() ? join(path, 'index.html') : null;
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

const axeSource = await readFile(new URL('../node_modules/axe-core/axe.min.js', import.meta.url), 'utf8');
// Accessibility rules our markup controls. MudBlazor's own internals that cannot be labelled from outside are
// excluded by selector, each reported upstream: tree-view and color-picker internals, pager selects, data-grid header
// buttons and loading bar, nav-group inner navs, nested lists, the popover provider, and avatar text colors (docs/wiki/testing.md).
const axeRules = ['link-in-text-block', 'color-contrast', 'landmark-unique', 'label', 'aria-input-field-name', 'button-name',
  'aria-hidden-focus', 'label-content-name-mismatch', 'aria-command-name', 'select-name', 'image-alt', 'duplicate-id-aria',
  'link-name', 'aria-toggle-field-name', 'aria-progressbar-name', 'svg-img-alt', 'document-title', 'page-has-heading-one'];
const axeExclude = [['.mud-treeview .mud-checkbox-input'], ['.mud-picker-color-content'], ['.mud-table-pagination'],
  ['.mud-table-root th .mud-icon-button'], ['.mud-table .mud-menu-icon-button-activator'], ['.mud-table .mud-progress-linear'],
  ['.mud-nav-group nav:not([aria-label])'], ['.mud-nested-list'], ['.mud-popover-provider'], ['.mud-avatar-text']];

// MudBreadcrumbs names every <nav> "Breadcrumb" and moves a passed aria-label to the inner <ol> (MudBlazor#13974).
const axeIgnore = [{ rule: 'landmark-unique', html: 'aria-label="Breadcrumb"' }];

async function audit(page, label) {
  if (!(await page.evaluate(() => 'axe' in window))) await page.evaluate(axeSource);
  const result = await page.evaluate(([rules, exclude]) => window.axe.run({ exclude }, { runOnly: { type: 'rule', values: rules } }), [axeRules, axeExclude]);
  return result.violations.flatMap((violation) => violation.nodes
    .filter((node) => !axeIgnore.some((ignore) => ignore.rule === violation.id && node.html.includes(ignore.html)))
    .map((node) => `${label}: ${violation.id} at ${node.target.join(' ')}`));
}

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
  // Reduced motion zeroes the app's animations, so contrast is measured on settled content, not mid-transition.
  const page = await browser.newPage({ reducedMotion: 'reduce' });
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  page.on('console', message => {
    // The first request for a deep link is answered by 404.html before it redirects into the app.
    if (message.type() === 'error' && !message.text().includes('404')) errors.push(message.text());
  });
  const heading = name => page.getByRole('heading', { level: 1, name });
  const timeout = { timeout: 60_000 };
  // A prerendered page is readable at once but interactive only when .NET has started and removed the start-up note.
  const started = () => page.locator('.demo-booting').waitFor({ state: 'detached', ...timeout });

  await step('deep links are served prerendered, with their own title, before .NET starts', async () => {
    for (const [route, title] of [['components/button', 'Button · Components'], ['labs/api', 'API and server paging'], ['samples', 'Page samples'], ['', 'Blazor learning sandbox']]) {
      const response = await fetch(`${base}${route}`);
      const html = await response.text();
      if (response.status !== 200) throw new Error(`/${route} returned ${response.status}`);
      if (!html.includes(`<title>${title}`) || (html.match(/<h1/g) ?? []).length !== 1) throw new Error(`/${route} is not prerendered`);
      if (!html.includes('class="demo-booting"')) throw new Error(`/${route} has no start-up note`);
    }
  });
  await step('deep link boots the app and keeps the route', async () => {
    await page.goto(`${base}labs/api`);
    await heading('API and server paging').waitFor(timeout);
    if (!page.url().endsWith('/demo/labs/api')) throw new Error(`unexpected URL ${page.url()}`);
    // The interactive app replaced the prerendered markup: one layout, and no start-up note.
    await started();
    if (await page.locator('.mud-layout').count() !== 1) throw new Error('prerendered and interactive layouts are both present');
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
    await started();
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
    // Applying reloads the page; the prerendered (English) page shows first, then the app in the stored culture.
    await Promise.all([page.waitForEvent('load', timeout), page.getByRole('button', { name: 'Apply language / Aplicar idioma' }).click()]);
    await started();
    await page.getByText('Bem-vindo ao laboratório de localização').waitFor(timeout);
    await page.getByText('Valor: 1.234,56').waitFor();
    if (await page.evaluate(() => document.documentElement.lang) !== 'pt') throw new Error('document language not updated');
    await page.evaluate(() => localStorage.setItem('culture', 'en-US'));
  });
  await step('CSV import validates the whole file in the browser', async () => {
    await page.goto(`${base}labs/files`);
    await heading('Files and cancellable work').waitFor(timeout);
    await started();
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
    await started();
    await page.getByRole('combobox', { name: 'Dataset size' }).click();
    await page.getByText('69,420', { exact: true }).click();
    await page.getByRole('button', { name: 'Generate dataset' }).click();
    await page.getByText('69,420 records in component memory').waitFor(timeout);
  });
  const accessibility = [];
  await step('quick search opens with Ctrl+K and lands on an example', async () => {
    await page.goto(`${base}components/button`);
    await heading('Button').waitFor(timeout);
    await started();
    await page.keyboard.press('Control+k');
    const search = page.getByRole('dialog').getByRole('combobox');
    await search.fill('alert playground');
    await page.locator('.quick-search-item', { hasText: 'Alert' }).first().waitFor();
    accessibility.push(...await audit(page, 'quick search'));
    await page.keyboard.press('Enter');
    await heading('Alert').waitFor();
    if (!page.url().endsWith('/demo/components/alert#alertplayground')) throw new Error(`search opened ${page.url()}`);
    if (await page.getByRole('dialog').count()) throw new Error('the search dialog stayed open');
  });
  await step('a playground link restores its settings', async () => {
    await page.goto(`${base}components/button?playground=buttonplayground&label=From%20a%20link&disabled=true#buttonplayground`);
    await heading('Button').waitFor(timeout);
    await started();
    const playground = page.locator('#buttonplayground');
    await playground.getByText('Settings restored from a shared link.').waitFor();
    await playground.getByRole('button', { name: 'From a link' }).waitFor();
    if (!(await playground.locator('.playground-code').innerText()).includes('Disabled="true"')) throw new Error('generated markup was not restored');
    await playground.getByRole('button', { name: 'Reset' }).click();
    await playground.getByRole('button', { name: 'Buy tickets' }).waitFor();
  });
  await step('after one visit, the demo starts offline from the service worker cache', async () => {
    const context = await browser.newContext({ reducedMotion: 'reduce' });
    try {
      const visit = await context.newPage();
      await visit.goto(`${base}labs/api`);
      await visit.locator('.demo-booting').waitFor({ state: 'detached', ...timeout });
      // The worker precaches the runtime while installing, then takes control of the open page.
      await visit.waitForFunction(() => navigator.serviceWorker.controller !== null, null, timeout);
      await context.setOffline(true);
      await visit.goto(`${base}components/button`);
      await visit.getByRole('heading', { level: 1, name: 'Button' }).waitFor(timeout);
      await visit.getByRole('button', { name: 'Code' }).first().click();
      await visit.getByRole('button', { name: 'Hide code' }).waitFor();
    } finally {
      await context.close();
    }
  });
  await step('unknown routes render the not-found page', async () => {
    await page.goto(`${base}no-such-lab`);
    await heading('Page not found').waitFor(timeout);
  });
  await step('every page renders without an error boundary', async () => {
    const routes = (await appRoutes()).filter((route) => route !== '/not-found' && route !== '/Error').sort();
    await page.goto(base);
    await started();
    const broken = [];
    for (const route of routes) {
      // In-app navigation keeps one runtime, so the sweep covers every page in seconds.
      await page.evaluate((target) => window.Blazor.navigateTo(target), route.replace(/^\//, ''));
      try {
        await page.waitForFunction((target) => location.pathname.endsWith(target === '/' ? '/demo/' : target) && document.querySelectorAll('h1').length === 1, route, { timeout: 15_000 });
        await page.waitForTimeout(150);
        if (await page.getByText('failed to render').count()) broken.push(`${route}: example failed to render`);
        accessibility.push(...await audit(page, route));
      } catch {
        broken.push(`${route}: no single h1`);
      }
    }
    if (broken.length) throw new Error(broken.join('; '));
    console.log(`  ${routes.length} routes`);
  });
  await step('every page passes the accessibility rules our markup controls', async () => {
    for (const path of ['', 'docs/', 'docs/gallery/', 'docs/getting-started/']) {
      const response = await page.goto(`http://127.0.0.1:${server.address().port}${prefix}/${path}`, { waitUntil: 'load' });
      // The server answers unknown paths with 404.html; auditing that page would hide a missing one.
      if (!response?.ok()) throw new Error(`site /${path} returned ${response?.status()}`);
      accessibility.push(...await audit(page, `site /${path}`));
    }
    if (accessibility.length) {
      console.error(`  ${accessibility.join('\n  ')}`);
      throw new Error(`${accessibility.length} violations`);
    }
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
