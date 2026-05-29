#!/usr/bin/env python3
"""Source-backed drift checks for README and wiki docs.

The checks intentionally target facts that have drifted before: package versions,
release workflow topology, deploy workflow implementation, and route/sidebar
coverage for the Markdown wiki.
"""

from __future__ import annotations

import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = ROOT / "README.md"
AGENTS = ROOT / "AGENTS.md"
AGENT_MEMORY_DIR = ROOT / ".agents" / "memory"
WIKI_DIR = ROOT / "docs" / "wiki"
DOC_FILES = [README, *sorted(WIKI_DIR.glob("*.md"))]
AGENT_FILES = [AGENTS, *sorted(AGENT_MEMORY_DIR.glob("*.md"))]
PUBLIC_COPY_FILES = [
    ROOT / "src" / "WebClient" / "Components" / "Pages" / "Home.razor",
    ROOT / "docs" / "src" / "components" / "home" / "Hero.astro",
    ROOT / "docs" / "src" / "components" / "home" / "Dashboard.astro",
]
TEXT_FILES = [*DOC_FILES, *AGENT_FILES, *PUBLIC_COPY_FILES]


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def fail(message: str) -> None:
    print(f"docs drift: {message}", file=sys.stderr)
    raise SystemExit(1)


def require(condition: bool, message: str) -> None:
    if not condition:
        fail(message)


def require_contains(path: Path, needle: str) -> None:
    require(needle in read(path), f"{path.relative_to(ROOT)} is missing {needle!r}")


def require_absent(pattern: str, flags: int = 0) -> None:
    regex = re.compile(pattern, flags)
    for path in TEXT_FILES:
        for line_no, line in enumerate(read(path).splitlines(), 1):
            if regex.search(line):
                fail(f"stale phrase in {path.relative_to(ROOT)}:{line_no}: {line}")


def package_version(package_name: str) -> str:
    project = ET.parse(ROOT / "src" / "WebClient" / "WebClient.csproj")
    for item in project.findall(".//PackageReference"):
        if item.attrib.get("Include") == package_name:
            return item.attrib["Version"]
    fail(f"PackageReference {package_name!r} not found")


def workflow_jobs(workflow: str) -> set[str]:
    text = read(ROOT / ".github" / "workflows" / workflow)
    jobs_index = text.find("\njobs:\n")
    require(jobs_index >= 0, f"could not find jobs section in {workflow}")
    jobs_body = text[jobs_index:]
    return set(re.findall(r"^  ([A-Za-z0-9_-]+):\n    ", jobs_body, re.M))


def main() -> None:
    mudblazor = package_version("MudBlazor")
    translations = package_version("MudBlazor.Translations")
    app_insights = package_version("Microsoft.ApplicationInsights.AspNetCore")
    sdk = json.loads(read(ROOT / "global.json"))["sdk"]["version"]

    require_contains(README, f"MudBlazor | {mudblazor}")
    require_contains(ROOT / "docs" / "wiki" / "project-structure.md", f"MudBlazor {mudblazor}")
    require_contains(ROOT / "src" / "WebClient" / "Components" / "Pages" / "Home.razor", f"MudBlazor {mudblazor.rsplit('.', 1)[0]}")
    require_contains(ROOT / "docs" / "src" / "components" / "home" / "Hero.astro", f"MudBlazor {mudblazor.rsplit('.', 1)[0]}")
    require_contains(README, f"SDK {sdk}")
    require_contains(ROOT / "docs" / "wiki" / "configuration.md", f'"version": "{sdk}"')
    require(AGENTS.exists(), "AGENTS.md must exist for standardized harness instructions")
    require(AGENT_MEMORY_DIR.exists(), ".agents/memory must exist for standardized agent memory")
    legacy_word = "cla" + "ude"
    legacy_root_file = ROOT / (legacy_word.upper() + ".md")
    legacy_dir = ROOT / ("." + legacy_word)
    require(not legacy_root_file.exists(), "legacy root harness file must be removed after AGENTS.md migration")
    require(not legacy_dir.exists(), "legacy dot-directory must be removed after .agents migration")

    require_contains(AGENTS, f"MudBlazor {mudblazor}")
    require_contains(AGENTS, f"MudBlazor.Translations {translations}")
    require_contains(AGENTS, f"Microsoft.ApplicationInsights.AspNetCore {app_insights}")
    require_absent(rf"\.?{legacy_word}", flags=re.I)

    require_absent(r"MudBlazor (?:9\.2(?:\.0)?)")
    if not (ROOT / ".github" / "dependabot.yml").exists():
        require_absent(r"Dependabot", flags=re.I)
    if not (ROOT / "docker-compose.yml").exists():
        require_absent(r"docker-compose", flags=re.I)
    require_absent(r"dependency review|container scanning", flags=re.I)
    require_contains(ROOT / "README.md", "Renovate")
    require_contains(ROOT / "docs" / "wiki" / "home.md", "Renovate")
    require_contains(ROOT / "docs" / "wiki" / "project-structure.md", "renovate.json")
    require_contains(ROOT / "docs" / "wiki" / "configuration.md", "APPLICATIONINSIGHTS_CONNECTION_STRING")
    require_contains(ROOT / "docs" / "wiki" / "deployment.md", "App Service Plan: `github-jonathanperis` (`B1`, Linux)")
    require_contains(ROOT / "infra" / "main.bicep", "module appServicePlan 'modules/appServicePlan.bicep'")
    require_contains(ROOT / "docs" / "wiki" / "documentation.md", "Sätteri")
    require_absent(r"Production-optimized builds with AOT(?: compilation)?[, ]")
    require_absent(r"Runs three sequential jobs|build-push-image|deploy-image-azure")
    require_absent(r"actions/configure-pages|actions/upload-pages-artifact")

    release_jobs = workflow_jobs("main-release.yml")
    for job in [
        "setup-build-test",
        "build-push-amd64",
        "deploy-infra",
        "deploy-image",
        "build-push-arm64",
        "merge-manifest",
    ]:
        require(job in release_jobs, f"main-release.yml no longer has job {job!r}")
        require_contains(ROOT / "docs" / "wiki" / "deployment.md", f"**{job}**")

    deploy_workflow = read(ROOT / ".github" / "workflows" / "deploy.yml")
    require("pages-docs-deploy.yml@main" in deploy_workflow, "deploy.yml no longer delegates to reusable Pages workflow")
    require_contains(ROOT / "docs" / "wiki" / "deployment.md", "pages-docs-deploy.yml@main")

    sidebar = read(ROOT / "docs" / "src" / "lib" / "sidebar.config.ts")
    sidebar_ids = set(re.findall(r"[\"']([a-z0-9-]+)[\"']", sidebar))
    wiki_slugs = {path.stem for path in WIKI_DIR.glob("*.md")}
    require(wiki_slugs <= sidebar_ids, f"wiki files missing from sidebar: {sorted(wiki_slugs - sidebar_ids)}")

    print("README/wiki drift checks passed")


if __name__ == "__main__":
    main()
