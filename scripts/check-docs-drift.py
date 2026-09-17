#!/usr/bin/env python3
"""Compare documented stack, toolchain, lab routes and release steps with source."""
import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(path):
    return (ROOT / path).read_text(encoding="utf-8")


def require(condition, message):
    if not condition:
        raise SystemExit(f"docs drift: {message}")


def main():
    readme = read("README.md")
    sdk = json.loads(read("global.json"))["sdk"]["version"]
    require(f"SDK {sdk}" in readme, "README SDK version differs from global.json")
    require(f'"version": "{sdk}"' in read("docs/wiki/configuration.md"), "configuration guide SDK version differs")
    require(f"dotnet/sdk:{sdk}" in read("src/WebClient/Dockerfile"), "Docker SDK differs from global.json")
    project = ET.fromstring(read("src/WebClient/WebClient.csproj"))
    for package in project.findall(".//PackageReference"):
        if package.get("PrivateAssets") == "all":
            continue
        require(f"| {package.get('Include')} | {package.get('Version')} |" in readme, f"README package differs: {package.get('Include')}")
    target_major = project.findtext(".//TargetFramework", default="").removeprefix("net").split(".")[0]
    mud_major = project.findall(".//PackageReference[@Include='MudBlazor']")[0].attrib["Version"].split(".")[0]
    hero = read("docs/src/components/home/Hero.astro")
    for label in [f".NET {target_major}", f"MudBlazor {mud_major}"]:
        require(f">{label}</span>" in hero, f"landing-page stack differs: {label}")
    require("dotnet restore --locked-mode" in read("docs/src/components/home/Dashboard.astro"), "landing quickstart missing locked restore")
    for path in ["src/WebClient/packages.lock.json", "tests/WebClient.Tests/packages.lock.json", "docs/bun.lock"]:
        require((ROOT / path).is_file(), f"missing lockfile: {path}")

    catalog = read("src/WebClient/Features/Learning/LabCatalog.cs")
    labs = re.findall(r'new\("([^\"]+)",\s*"[^\"]+",\s*"([^\"]+)".*?,\s*"([^\"]+\.razor)"\)', catalog, re.S)
    require(bool(labs), "could not read lab catalog")
    for slug, route, source in labs:
        path = ROOT / "src/WebClient/Components/Pages" / source
        require(path.is_file() and f'@page "{route}"' in path.read_text(), f"catalog route/source differs: {slug}")
        require(f"`{route}`" in readme, f"README missing lab: {route}")
        require(f'"{route}"' in read("scripts/smoke-http.py"), f"HTTP smoke missing lab: {route}")

    sidebar = read("docs/src/lib/sidebar.config.ts")
    ids = [slug for group in re.findall(r"ids:\s*\[([^\]]+)\]", sidebar) for slug in re.findall(r"'([^']+)'", group)]
    wiki = {path.stem for path in (ROOT / "docs/wiki").glob("*.md")}
    require(set(ids) == wiki and len(ids) == len(wiki), "wiki and sidebar coverage differ")

    release = read(".github/workflows/main-release.yml").split("\njobs:\n", 1)[1]
    jobs = re.findall(r"^  ([A-Za-z0-9_-]+):$", release, re.M)
    deployment = read("docs/wiki/deployment.md")
    release_guide = deployment.split("## Release flow\n", 1)[1].split("\n## ", 1)[0]
    documented_jobs = re.findall(r"^\d+\. \*\*([^*]+)\*\*", release_guide, re.M)
    require(sorted(jobs) == sorted(documented_jobs), "deployment guide release jobs differ (including obsolete jobs)")
    pages_workflow = "pages-docs-deploy.yml@d7e3c753530db86cb01b9510ab045c99b172ba03"
    require(pages_workflow in read(".github/workflows/deploy.yml"), "Pages workflow delegation changed; review the pin and update docs")
    require(pages_workflow in read("docs/wiki/deployment.md"), "Pages delegation missing from guide")
    require("Renovate" in readme and (ROOT / "renovate.json").is_file(), "dependency management docs/config differ")

    package = json.loads(read("docs/package.json"))
    node = read("docs/.node-version").strip()
    bun = package["packageManager"].removeprefix("bun@")
    build_checks = read(".github/workflows/build-check.yml")
    require("node-version-file: docs/.node-version" in build_checks, "PR checks must use the docs Node pin")
    require(f"node-version: '{node}'" in read(".github/workflows/deploy.yml"), "Pages Node version differs from docs pin")
    require(f"bun-version: '{bun}'" in build_checks, "PR Bun version differs from packageManager")
    for path in ["docs/README.md", "docs/wiki/documentation.md"]:
        guide = read(path)
        require(f"Node.js {node}" in guide and f"Bun {bun}" in guide and "Python 3" in guide, f"docs toolchain differs: {path}")
    for path in ["README.md", "docs/README.md", "docs/wiki/documentation.md", "AGENTS.md", ".github/workflows/build-check.yml"]:
        for command in ["check:drift", "check:types", "build", "check:rendered"]:
            require(f"npm run {command}" in read(path), f"missing docs command {command}: {path}")
    require(package["dependencies"]["astro"].startswith("^7."), "update the Astro major-version guide")
    require(package["engines"]["node"] == ">=22.12.0", "update documented Node requirements")
    require("trivy-action" in read(".github/workflows/build-check.yml"), "container scan claim differs from CI")
    print(f"Source-backed docs checks passed ({len(labs)} labs, {len(wiki)} guide pages)")


if __name__ == "__main__":
    main()
