export const SLUG_LABEL: Record<string, string> = {
  home: 'Overview', 'getting-started': 'Getting started', 'learning-path': 'Learning paths',
  components: 'Lab reference', configuration: 'Configuration', testing: 'Testing and experiments',
  deployment: 'Docker and Azure', documentation: 'Documentation site', 'project-structure': 'Project structure',
};

export const SECTION_CATEGORIES: { label: string; ids: string[] }[] = [
  { label: 'Start here', ids: ['home', 'getting-started', 'learning-path'] },
  { label: 'Understand the code', ids: ['components', 'configuration', 'testing'] },
  { label: 'Extend and deploy', ids: ['deployment', 'documentation', 'project-structure'] },
];
export const SECTION_ORDER = SECTION_CATEGORIES.flatMap(({ ids }) => ids);
