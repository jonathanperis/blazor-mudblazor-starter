export const PAGE_METADATA: Record<string, { label: string; description: string }> = {
  home: { label: 'Overview', description: 'Explore the Blazor learning sandbox: local-first MudBlazor labs, guided exercises, working source, and explicit application boundaries.' },
  'getting-started': { label: 'Getting started', description: 'Run the Blazor and MudBlazor labs locally with .NET or Docker, choose a launch profile, and complete your first state experiment.' },
  'learning-path': { label: 'Learning paths', description: 'Follow nine hands-on lessons from component state and forms to APIs, SQLite, authentication, files, and observability.' },
  components: { label: 'Lab reference', description: 'Understand the lab implementations: draft editing, DataGrid state, bounded HTTP requests, workspace isolation, CSV validation, and cancellation.' },
  configuration: { label: 'Configuration', description: 'Configure the SDK, internal API address, SQLite storage, demo authentication, culture, publishing options, and optional telemetry.' },
  testing: { label: 'Testing and experiments', description: 'Run component and integration tests, check published routes over HTTP, and measure Blazor experiments with clear verification boundaries.' },
  deployment: { label: 'Docker and hosting', description: 'Build the learning container, understand validation and image publishing, and review the decisions needed for the pending Hostinger setup.' },
  documentation: { label: 'Documentation site', description: 'Maintain the Astro and Sätteri guide: toolchain setup, page metadata, authoring links, freshness checks, and GitHub Pages publishing.' },
  'project-structure': { label: 'Project structure', description: 'Find the Blazor host, shared learning components, feature modules, tests, and guide sources; learn how to add a lab or SQLite migration.' },
};

export const SECTION_CATEGORIES: { label: string; ids: string[] }[] = [
  { label: 'Start here', ids: ['home', 'getting-started', 'learning-path'] },
  { label: 'Understand the code', ids: ['components', 'configuration', 'testing'] },
  { label: 'Extend and deploy', ids: ['deployment', 'documentation', 'project-structure'] },
];
export const SECTION_ORDER = SECTION_CATEGORIES.flatMap(({ ids }) => ids);
