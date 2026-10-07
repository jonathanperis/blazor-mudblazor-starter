// Regenerates the screenshots used as plates on the landing page, in light and dark schemes.
// Usage: start either host (e.g. `dotnet run --project src/WebClient`), then
//   node scripts/capture-screens.mjs http://localhost:5000/
// Uses Playwright's Chromium; set PLAYWRIGHT_CHANNEL=chrome to use an installed Google Chrome.
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5000/';
const out = new URL('../public/screens/', import.meta.url).pathname;
const shots = [{ name: 'gallery', route: 'components/button' }];

const browser = await chromium.launch({ channel: process.env.PLAYWRIGHT_CHANNEL || undefined });
try {
  for (const scheme of ['light', 'dark']) {
    const page = await browser.newPage({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1, colorScheme: scheme });
    await page.addInitScript((dark) => localStorage.setItem('isDarkMode', String(dark)), scheme === 'dark');
    for (const shot of shots) {
      await page.goto(new URL(shot.route, base).href, { waitUntil: 'networkidle' });
      await page.getByRole('heading', { level: 1 }).waitFor();
      await page.waitForTimeout(800);
      await page.screenshot({ path: `${out}${shot.name}-${scheme}.jpg`, type: 'jpeg', quality: 84 });
      console.log(`captured ${shot.name}-${scheme}.jpg`);
    }
    await page.close();
  }
} finally {
  await browser.close();
}
