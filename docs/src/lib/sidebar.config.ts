export const SECTION_CATEGORIES = [
  { label: "", ids: ["home"] },
  { label: "Reference", ids: ["components", "configuration"] },
  { label: "Operations", ids: ["getting-started", "deployment", "project-structure"] },
] as const;

export const SECTION_ORDER = SECTION_CATEGORIES.flatMap(({ ids }) => ids);
