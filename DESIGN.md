# Design context

The sandbox looks like a printed lab manual: warm paper, ink, one vermilion accent. It should feel made by a person who cares about typography, not assembled from a template. MudBlazor components stay native and recognizable; the identity comes from the theme, type, rules and spacing around them. The visual hierarchy serves learning: objective, experiment, explanation, common mistake, exercise.

## Identity

- **Palette:** paper `#F4F1EA`, sheet `#FBFAF6`, ink `#1C1B19`, soft ink `#5E5A52`, rules `#DDD7CB`, vermilion `#B4441F`. Dark: paper `#141311`, ink `#ECE7DD`, vermilion `#FF8A5C`. Secondary teal and ochre exist for MudBlazor's color parameters only. The palette lives in `LearningTheme.cs` (app) and `site.css` (guide); keep them identical and keep every text pair at WCAG AA, input borders at 3:1.
- **Type:** Fraunces (serif, often italic) for display and headings; IBM Plex Sans for text; IBM Plex Mono for code, metadata, kickers and numbers. All self-hosted under the OFL, never requested from a third party. Buttons are sentence case.
- **Devices:** mono uppercase kickers, numbered chapters and parts, dotted leaders, ruled tables (2px ink rule above, hairlines between), figure captions ("Plate 1 · …"), roman-numeral steps, a colophon. Corners are small (6px); shadows are rare; outlined papers over elevation.
- **Avoid:** gradients and glows, glassmorphism, emoji, icon-in-a-circle feature cards, purple, all-caps buttons, generic stock phrasing. If a section could appear on any SaaS landing page, rewrite it.

## Navigation

- Overview explains the learning purpose and offers a clear starting point.
- The app drawer reads like a table of contents: Labs, Components (grouped by family, the current group expanded), Page samples.
- `/labs`, `/components` and `/samples` are indexes; the components index is searchable.
- Each lab has one page heading, a source link, linked prerequisites, difficulty and time. Host-specific differences appear as a labeled note inside the lab.
- Each component page has one heading, the MudBlazor types it covers, links to the MudBlazor reference and its source, and an "On this page" list of examples.
- The static demo shows a persistent banner explaining that server-only features use stand-ins, with a link to run locally.
- Retain `/counter` and `/weather` as recognizable foundational routes.

## Gallery

- Every example is its own file; the code shown is the embedded source of exactly what rendered. Examples are written to be read: short, idiomatic, meaningful data.
- Where a component has two appearance enums, show every combination with `EnumMatrix`. Main components get a playground whose generated markup omits defaults.
- Examples sit on "specimen plates" (sheet, hairline border, faint dot grid). Wide content uses a flush plate and scrolls inside it.

## Interaction

- Show loading, empty, error, cancellation and completion states explicitly.
- Keep destructive actions clearly named and scoped. Notebook reset requires confirmation.
- Expose clipboard actions as visible controls; context menus are an additional shortcut and must not add empty focusable controls.
- Use real form labels, visible focus, a main landmark, a skip link that stays on the page and polite status announcements. Announce state changes, not every progress step.
- Respect reduced-motion preferences.
- Browser storage failure must not prevent the shell from rendering.

## Documentation site

The guide shares the identity above. Guide pages use a book layout: numbered contents, the article at a 68-character measure, an "On this page" list on wide screens, previous/next links. On small screens the contents collapse behind a native disclosure. Topic pages have unique anchors, current-page navigation and keyboard-scrollable code and tables. The landing page's contents are generated from the app's catalogs. Copy explains mechanisms and tradeoffs with concrete language.
