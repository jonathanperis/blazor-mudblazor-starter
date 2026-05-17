# Design Context

## Existing System

The application uses MudBlazor 9.3.0 on Blazor Server. The current shell includes:

- Dense `MudAppBar` with menu button, app title, theme control, and overflow menu.
- `MudDrawer` with `MudNavMenu` links for Home, Counter, and Weather.
- Reusable `Breadcrumb` component.
- `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, and `MudSnackbarProvider` at layout level.
- Dark mode and drawer state persisted in `localStorage`.
- Responsive behavior through `IBrowserViewportService`.

## Visual Baseline

The current aesthetic is default MudBlazor Material:

- Primary purple app bar.
- Light neutral page background.
- White elevated cards and papers.
- Material icons.
- Standard MudBlazor spacing utilities such as `pa-4`, `ma-4`, `ml-4`.
- Dense top navigation and left drawer.

This is the identity to preserve, but it needs stronger hierarchy, clearer content, better rhythm, and less placeholder-template behavior.

## Color Strategy

Restrained Material with one committed primary accent.

- Keep MudBlazor primary purple as the recognizable brand accent.
- Use tinted neutrals rather than pure black or pure white where custom CSS is introduced.
- Let status colors keep Material semantics: success for constructive actions, error for destructive actions, info or secondary for neutral guidance.
- Avoid decorative gradients. If color is added, it should clarify status, navigation, section ownership, or call-to-action priority.

## Theme Scene Sentence

A .NET developer evaluates the starter on a laptop during normal working hours, switching between code, documentation, and the live demo while deciding whether this is safe to fork for a real project. This favors a light default with a fully credible dark mode, not a dark-first spectacle.

## Typography

Use MudBlazor typography first. Improve hierarchy through component choices, weight, and size rather than introducing a new brand font unless the overhaul explicitly adds custom CSS.

- Home hero: stronger than `Typo.h6`, likely `Typo.h3` or `Typo.h4` depending on layout.
- Section titles: `Typo.h5` or `Typo.h6`.
- Supporting text: body text capped around 65 to 75 characters.
- Metadata and labels: `Typo.caption` or `Typo.body2`.

## Layout Principles

- Keep the app shell. Improve what sits inside it.
- Replace isolated full-width placeholder cards with purposeful sections.
- Use `MudGrid`, `MudStack`, `MudPaper`, `MudCard`, `MudAlert`, `MudChip`, and `MudButton` where they demonstrate real MudBlazor usage.
- Avoid identical card grids as the main page grammar. If cards are used, vary scale and purpose: hero proof panel, quick action row, feature proof, deployment proof, and demo navigation.
- Breadcrumbs should be compact and secondary, not a dominant card.
- Preserve drawer navigation but make active route and destination purpose clearer.

## Component Direction

### Home

The home page should become a starter showcase:

- Clear hero: what the starter is, why it is production-ready, and what to do next.
- Primary action: view GitHub or documentation.
- Secondary action: explore Weather demo.
- Proof strip: .NET 9, MudBlazor 9.3, Docker, Azure, CodeQL, GHCR.
- Small implementation notes or getting-started command block.
- Avoid generic “Hello, world!” copy.

### Weather

The Weather page should remain a high-density data-grid demo, not a literal weather app.

- Explain the purpose: virtualized 69K-row grid with filtering, sorting, CRUD dialogs, selection, and clipboard context menu.
- Group Add and Remove actions with clearer selection state.
- Keep `MudDataGrid`, but reduce duplicated demo columns or label them as horizontal-scroll stress-test columns if they stay.
- Add lightweight helper copy for right-click copy and selected-row actions.
- Preserve loading state and snackbar feedback.

### Counter

Counter can stay simple, but should be framed as a minimal interactivity demo rather than filler.

## Accessibility Expectations

- Interactive controls need clear accessible names.
- Theme toggle should communicate current mode and next action without long app-bar copy on desktop.
- Buttons that change data need clear destructive or constructive treatment.
- Breadcrumb text should not look disabled unless it is intentionally non-interactive and still readable.
- Color cannot be the only signal for action severity.
- Maintain keyboard navigation for drawer, menu, dialogs, and data grid.

## Motion

Use minimal Material-appropriate motion. Avoid attention-seeking animation. If added, limit it to purposeful state changes, drawer/menu behavior, and subtle page section entry. Do not animate layout properties.

## Copy Rules

- Be concrete and technical.
- Replace placeholder tutorial language with proof-led messaging.
- Avoid em dashes.
- Avoid repeating headings in body copy.
- Keep labels short in dense UI areas.

## A/B Testing Surfaces

Likely high-value experiments:

- Hero content density: concise positioning versus proof-rich technical summary.
- CTA hierarchy: GitHub-first versus documentation-first versus demo-first.
- Weather page framing: data-grid benchmark demo versus CRUD operations demo.
- Navigation density: persistent drawer emphasis versus content-first compact shell.
- Theme control placement: visible app-bar switch versus icon toggle plus menu item.
