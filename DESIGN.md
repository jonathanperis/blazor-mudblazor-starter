# Design context

Use native MudBlazor Material components to make experiments familiar and readable. The visual hierarchy serves learning: objective, experiment, explanation, common mistake, exercise.

## Navigation

- Overview explains the learning purpose and offers a clear starting point.
- `/labs` is a searchable catalog; the drawer links every lab.
- Each lab has one page heading, a source link, linked prerequisites, difficulty and time. Host-specific differences appear as a labeled note inside the lab.
- The static demo shows a persistent banner explaining that server-only features use stand-ins, with a link to run locally.
- Retain `/counter` and `/weather` as recognizable foundational routes.

## Appearance

- Use MudBlazor palette variables for application surfaces, borders and text.
- Keep both themes readable, including native form controls and code blocks.
- Prefer simple outlined papers and typography over decorative marketing elements.
- Keep dense data tables horizontally scrollable without hiding important actions.

## Interaction

- Show loading, empty, error, cancellation and completion states explicitly.
- Keep destructive actions clearly named and scoped. Notebook reset requires confirmation.
- Expose clipboard actions as visible controls; context menus are an additional shortcut and must not add empty focusable controls.
- Use real form labels, visible focus, a main landmark, a skip link that stays on the page and polite status announcements. Announce state changes, not every progress step.
- Respect reduced-motion preferences.
- Browser storage failure must not prevent the shell from rendering.

## Documentation

Use the same quiet Material-inspired palette. Standalone topic pages have unique anchors, current-page navigation, accessible mobile focus handling, and keyboard-scrollable code/table overflow. The site loads no web fonts. Copy should explain mechanisms and tradeoffs with concrete language.
