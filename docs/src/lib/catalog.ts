// Build-time reader for the application's own catalogs, so the site lists exactly what the app contains.
// Runs in Node during `astro build`; paths are relative to the docs/ working directory.
import { readFileSync, readdirSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';

const shared = resolve(process.cwd(), '../src/WebClient.Shared');

export interface Lab { slug: string; title: string; route: string; level: string; minutes: number; objective: string }
export interface GalleryPage { title: string; route: string; group: string; summary: string }
export interface Sample { title: string; route: string; summary: string }

export const GROUP_TITLES: Record<string, string> = {
  Actions: 'Actions',
  Inputs: 'Inputs and forms',
  Pickers: 'Pickers and uploads',
  DataDisplay: 'Data display',
  Charts: 'Charts',
  Feedback: 'Feedback and overlays',
  Navigation: 'Navigation',
  Layout: 'Layout and structure',
  Theming: 'Theming and globalization',
};
export const GROUP_ORDER = Object.keys(GROUP_TITLES);

function files(directory: string, suffix: string): string[] {
  return readdirSync(directory).flatMap((name) => {
    const path = join(directory, name);
    return statSync(path).isDirectory() ? files(path, suffix) : name.endsWith(suffix) ? [path] : [];
  });
}

// C# string literal contents: no escaped quotes are used in these catalogs.
const str = '"([^"]*)"';

export function labs(): Lab[] {
  const source = readFileSync(join(shared, 'Features/Learning/LabCatalog.cs'), 'utf8');
  const pattern = new RegExp(`new\\(${str},\\s*${str},\\s*${str},\\s*${str},\\s*(\\d+),\\s*\\[[^\\]]*\\],\\s*${str}`, 'g');
  const result = [...source.matchAll(pattern)].map(([, slug, title, route, level, minutes, objective]) => ({ slug, title, route, level, minutes: Number(minutes), objective }));
  if (result.length !== (source.match(/new\("/g) ?? []).length) throw new Error('docs catalog: could not read every lab in LabCatalog.cs');
  return result;
}

export function galleryPages(): GalleryPage[] {
  const pages = files(join(shared, 'Components/Gallery'), 'Page.razor').map((path) => {
    const source = readFileSync(path, 'utf8');
    const route = source.match(/@page\s+"([^"]+)"/)?.[1];
    const meta = source.match(new RegExp(`\\[ComponentPage\\(${str},\\s*GalleryGroup\\.(\\w+),\\s*${str}`));
    if (!route || !meta) throw new Error(`docs catalog: ${path} lacks @page or [ComponentPage]`);
    return { title: meta[1], group: meta[2], summary: meta[3], route };
  });
  return pages.sort((a, b) => GROUP_ORDER.indexOf(a.group) - GROUP_ORDER.indexOf(b.group) || a.title.localeCompare(b.title, 'en'));
}

export function samples(): Sample[] {
  return files(join(shared, 'Components/Samples'), 'Sample.razor').map((path) => {
    const source = readFileSync(path, 'utf8');
    const route = source.match(/@page\s+"([^"]+)"/)?.[1];
    const meta = source.match(new RegExp(`\\[PageSample\\(${str},\\s*${str}`));
    if (!route || !meta) throw new Error(`docs catalog: ${path} lacks @page or [PageSample]`);
    return { title: meta[1], summary: meta[2], route };
  }).sort((a, b) => a.title.localeCompare(b.title, 'en'));
}

export function groupedGallery(): { group: string; title: string; pages: GalleryPage[] }[] {
  const pages = galleryPages();
  return GROUP_ORDER.map((group) => ({ group, title: GROUP_TITLES[group], pages: pages.filter((page) => page.group === group) })).filter((section) => section.pages.length > 0);
}
