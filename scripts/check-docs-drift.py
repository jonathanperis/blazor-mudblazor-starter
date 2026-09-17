#!/usr/bin/env python3
"""Compare documented stack, lab routes and release steps with their sources."""
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
    for job in jobs:
        require(f"**{job}**" in read("docs/wiki/deployment.md"), f"deployment guide missing release job {job}")
    require("pages-docs-deploy.yml@main" in read(".github/workflows/deploy.yml"), "Pages workflow delegation changed; update docs")
    require("pages-docs-deploy.yml@main" in read("docs/wiki/deployment.md"), "Pages delegation missing from guide")
    require("Renovate" in readme and (ROOT / "renovate.json").is_file(), "dependency management docs/config differ")

    package = json.loads(read("docs/package.json"))
    require(package["dependencies"]["astro"].startswith("^7."), "update the Astro major-version guide")
    require(package["engines"]["node"] == ">=22.12.0", "update documented Node requirements")
    require("trivy-action" in read(".github/workflows/build-check.yml"), "container scan claim differs from CI")
    print(f"Source-backed docs checks passed ({len(labs)} labs, {len(wiki)} guide pages)")


if __name__ == "__main__":
    main()
