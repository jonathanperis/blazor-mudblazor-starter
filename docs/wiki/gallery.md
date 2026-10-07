# Component gallery and page samples

The gallery shows MudBlazor 9 components as working examples you can read: every example renders live beside the exact Razor source that produced it. [Browse it in the live demo](https://jonathanperis.github.io/blazor-mudblazor-starter/demo/components), or at `/components` when you run either host.

## How a component page works

Each page lives in [`src/WebClient.Shared/Components/Gallery/<Group>/`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Gallery/Actions/ButtonPage.razor) and declares its route and metadata:

```razor
@page "/components/button"
@attribute [ComponentPage("Button", GalleryGroup.Actions, "Buttons start an action.", MudDocs = "button", Types = ["MudButton"])]

<ComponentPageFrame Page="GetType()">
    <Example Of="typeof(ButtonVariants)" Title="Variants" Description="Emphasis comes from the variant." />
</ComponentPageFrame>
```

- **Discovery.** [`GalleryCatalog`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Gallery/GalleryCatalog.cs) finds pages by their `[ComponentPage]` attribute and `@page` route. The navigation, the `/components` index and the documentation landing page all read it, so adding a page never means editing a shared list.
- **One file per example.** Each `<Example>` renders a small component such as `ButtonVariants.razor`. The project embeds every gallery `.razor` file as a resource, and [`ExampleSources`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Gallery/ExampleSources.cs) shows that embedded file as the code. What you read is exactly what rendered.
- **Highlighting without markup.** [`SourceHighlighter`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Gallery/SourceHighlighter.cs) classifies Razor and C# tokens; `CodeBlock` renders them as text spans, never as raw HTML.
- **Combinations.** `EnumMatrix<TRow, TColumn>` renders every combination of two enum parameters, such as `Variant × Color`, from `Enum.GetValues`. Read it as a specimen table.
- **Playgrounds.** `Playground` puts a live component beside its controls; [`Snippet`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Gallery/Snippet.cs) writes the markup, leaving out values equal to the component's defaults. Pass `Owner="this"` and the playground can copy a link to its current settings and reset them. [`PlaygroundState`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Gallery/PlaygroundState.cs) treats every private, writable `_field` of text, number, switch, enum or color type as a setting. A link carries only the settings that differ from the defaults, and values from a link must parse and stay in range (text up to 200 characters, numbers 0–1000).
- **Quick search.** <kbd>Ctrl</kbd> <kbd>K</kbd> (<kbd>⌘</kbd> <kbd>K</kbd> on macOS) or the app bar's search button opens [`SiteSearch`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Gallery/SiteSearch.cs) in a dialog. It indexes the labs, the gallery pages, the page samples, and every `<Example>` read from the embedded page sources, so a new example is searchable without registration. Example results link to the example's anchor.
- **Isolation.** Each example renders inside an error boundary, so one failing example cannot break the page.

## Page samples

Samples under [`Components/Samples/`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Components/Samples/SamplesIndex.razor) are whole screens — a dashboard, a sign-in flow, a storefront, a kanban board and more — composed only from MudBlazor components and synthetic data. Each declares `[PageSample]` and renders inside `SamplePageFrame`. They run in both hosts, so they never call a server.

## Add a component page

1. Pick the group folder and create `<Name>Page.razor` with `@page "/components/<slug>"` and `[ComponentPage]`.
2. Add one file per example, prefixed with the component name, in the same folder. Write examples to be read: short, idiomatic, meaningful synthetic data, accessible names on icon-only controls, no `h1`.
3. Show the progression: basic usage, appearance, every combination of appearance enums, a realistic scenario, states (disabled, read-only, loading, errors), and a playground for main components.
4. Use base-relative links and browser-only data so the page works in the WebAssembly demo.
5. Give every control an accessible name: labels on inputs, `aria-label` on icon-only buttons, `MudList` and `MudNavMenu`, and `AdornmentAriaLabel` on adornment buttons. `npm run check:demo` audits every page with axe and fails on violations our markup controls.
6. Run `dotnet test --filter GalleryTests`. It renders every page with bUnit, fails if any example hits its error boundary, and checks that every example's source is embedded. The integration tests prerender every gallery route over HTTP.

## The visual identity

The app and this guide share one design: a printed lab manual with warm paper, ink and a vermilion accent, Fraunces for display type and IBM Plex for text and code, all self-hosted. The palette is defined once per surface, in [`LearningTheme.cs`](https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/src/WebClient.Shared/Features/Learning/LearningTheme.cs) and the guide's `site.css`, and checked against WCAG AA contrast. See `DESIGN.md` for the rules.
