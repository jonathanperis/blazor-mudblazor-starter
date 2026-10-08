// Renders the Open Graph image and the browser icons with the site's own fonts and palette (DESIGN.md).
//   public/og-image.png       1200x630, from scripts/og-template.html
//   favicon.png, favicon-32x32.png, apple-touch-icon.png, favicon.ico (16, 32 and 48 px PNG frames)
// The icons replace the copies in docs/public and in both hosts' wwwroot, which serve the same files.
// Usage, from docs/: npm run brand:render (Playwright's Chromium; set PLAYWRIGHT_CHANNEL=chrome to use Google Chrome).
import { existsSync } from 'node:fs';
import { readFile, writeFile } from 'node:fs/promises';
import { chromium } from 'playwright';

const docs = new URL('../', import.meta.url);
const iconTargets = ['public/', '../src/WebClient/wwwroot/', '../src/WebClient.Wasm/wwwroot/'].map((path) => new URL(path, docs));
const fraunces = new URL('src/fonts/fraunces-latin-wght-italic.woff2', docs);

// An italic Fraunces "B" in vermilion on paper inside an ink rule: the wordmark's letter and the hero's accent.
// Small sizes use a heavier, larger letter so it survives 16 px. Apple masks its icon itself, so the touch icon is opaque and has no rule.
function iconHtml(size, { rule = true } = {}) {
  const border = rule ? Math.max(1, Math.round(size / 24)) : 0;
  const radius = rule ? Math.round(size * 0.08) : 0;
  return `<!doctype html><style>
    @font-face { font-family: "Fraunces"; src: url("data:font/woff2;base64,${fontData}"); font-weight: 300 900; font-style: italic; }
    * { margin: 0; }
    html, body { width: ${size}px; height: ${size}px; background: transparent; }
    div { box-sizing: border-box; width: ${size}px; height: ${size}px; display: grid; place-items: center; background: #F4F1EA;
      border: ${border}px solid #1C1B19; border-radius: ${radius}px; }
    span { font: italic ${size <= 32 ? 760 : 600} ${Math.round(size * (size <= 32 ? 0.94 : 0.84))}px/1 "Fraunces"; color: #B4441F; transform: translate(-3%, 3%); }
  </style><div><span>B</span></div>`;
}

// ICO files may hold PNG frames; every browser that reads favicon.ico accepts them.
function ico(frames) {
  const header = Buffer.alloc(6 + 16 * frames.length);
  header.writeUInt16LE(1, 2);
  header.writeUInt16LE(frames.length, 4);
  let offset = header.length;
  frames.forEach(({ size, png }, index) => {
    const entry = 6 + 16 * index;
    header.writeUInt8(size % 256, entry);
    header.writeUInt8(size % 256, entry + 1);
    header.writeUInt16LE(1, entry + 4);
    header.writeUInt16LE(32, entry + 6);
    header.writeUInt32LE(png.length, entry + 8);
    header.writeUInt32LE(offset, entry + 12);
    offset += png.length;
  });
  return Buffer.concat([header, ...frames.map((frame) => frame.png)]);
}

const fontData = (await readFile(fraunces)).toString('base64');
const browser = await chromium.launch({ channel: process.env.PLAYWRIGHT_CHANNEL || undefined });
try {
  const og = await browser.newPage({ viewport: { width: 1200, height: 630 }, deviceScaleFactor: 1 });
  await og.goto(new URL('scripts/og-template.html', docs).href);
  await og.evaluate(() => document.fonts.ready);
  await og.screenshot({ path: new URL('public/og-image.png', docs).pathname });
  console.log('wrote public/og-image.png');

  const render = async (size, options) => {
    const page = await browser.newPage({ viewport: { width: size, height: size }, deviceScaleFactor: 1 });
    await page.setContent(iconHtml(size, options));
    await page.evaluate(() => document.fonts.ready);
    const png = await page.screenshot({ omitBackground: options?.rule !== false });
    await page.close();
    return png;
  };
  const icons = {
    'favicon.png': await render(256),
    'favicon-32x32.png': await render(32),
    'apple-touch-icon.png': await render(180, { rule: false }),
    'favicon.ico': ico(await Promise.all([16, 32, 48].map(async (size) => ({ size, png: await render(size) }))))
  };
  for (const target of iconTargets) {
    const names = Object.keys(icons).filter((name) => existsSync(new URL(name, target)));
    for (const name of names) await writeFile(new URL(name, target), icons[name]);
    console.log(`wrote ${names.join(', ')} to ${new URL(target).pathname}`);
  }
} finally {
  await browser.close();
}
