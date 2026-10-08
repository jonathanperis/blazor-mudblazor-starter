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
    projects = {path.relative_to(ROOT).as_posix(): ET.fromstring(path.read_text(encoding="utf-8")) for path in sorted((ROOT / "src").glob("*/*.csproj"))}
    require(set(projects) == {"src/WebClient/WebClient.csproj", "src/WebClient.Shared/WebClient.Shared.csproj", "src/WebClient.Wasm/WebClient.Wasm.csproj", "src/WebClient.Prerender/WebClient.Prerender.csproj"}, "update the docs for the changed project set")
    for path, project in projects.items():
        for package in project.findall(".//PackageReference"):
            if package.get("PrivateAssets") == "all":
                continue
            require(f"| {package.get('Include')} | {package.get('Version')} |" in readme, f"README package differs: {package.get('Include')} ({path})")
        require(f"`{path.split('/')[1]}`" in read("README.md") or f"`src/{path.split('/')[1]}/`" in readme, f"README structure misses {path}")
    target_major = projects["src/WebClient/WebClient.csproj"].findtext(".//TargetFramework", default="").removeprefix("net").split(".")[0]
    mud_major = projects["src/WebClient.Shared/WebClient.Shared.csproj"].findall(".//PackageReference[@Include='MudBlazor']")[0].attrib["Version"].split(".")[0]
    masthead = read("docs/src/components/Masthead.astro")
    for label in [f".NET {target_major}", f"MudBlazor {mud_major}"]:
        require(label in masthead, f"site masthead stack differs: {label}")
    require("dotnet restore --locked-mode" in read("docs/src/pages/index.astro"), "landing quickstart missing locked restore")
    for path in [*(project.replace(Path(project).name, "packages.lock.json") for project in projects), "tests/WebClient.Tests/packages.lock.json", "docs/bun.lock"]:
        require((ROOT / path).is_file(), f"missing lockfile: {path}")

    catalog = read("src/WebClient.Shared/Features/Learning/LabCatalog.cs")
    labs = re.findall(r'new\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)",\s*"[^"]+",\s*(\d+),.*?"(src/[^"]+\.razor)"\)', catalog, re.S)
    require(len(labs) == catalog.count('new("') and labs, "could not read every lab catalog entry")
    learning_path = read("docs/wiki/learning-path.md")
    for slug, title, route, minutes, source in labs:
        path = ROOT / source
        require(path.is_file() and f'@page "{route}"' in path.read_text(), f"catalog route/source differs: {slug}")
        require(f"`{route}`" in readme, f"README missing lab: {route}")
        require(f'"{route}": "{title}"' in read("scripts/smoke-http.py"), f"HTTP smoke missing lab heading: {route}")
        require(f"| {title} (`{route}`) | {minutes} min |" in learning_path, f"learning path differs from catalog: {route}")
        require(f'"{title}"' in read("docs/scripts/check-demo.mjs") or f"'{title}'" in read("docs/scripts/check-demo.mjs") or slug == "observability", f"demo browser check misses lab: {title}")

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
    release_workflow = read(".github/workflows/main-release.yml")
    build_checks = read(".github/workflows/build-check.yml")
    require("actions/upload-pages-artifact@" in build_checks and "actions/deploy-pages@" in release_workflow, "Pages artifact/deploy steps moved; update the deployment guide")
    require(not (ROOT / ".github/workflows/deploy.yml").exists(), "a separate Pages workflow returned; update the deployment guide")
    for step in ["prepare-pages-demo.py", "npm run check:rendered", "npm run check:demo"]:
        require(step in build_checks and step in read("docs/wiki/deployment.md") + read("docs/wiki/testing.md"), f"Pages pipeline step undocumented or missing: {step}")
    require("Renovate" in readme and (ROOT / "renovate.json").is_file(), "dependency management docs/config differ")

    package = json.loads(read("docs/package.json"))
    node = read("docs/.node-version").strip()
    bun = package["packageManager"].removeprefix("bun@")
    require("node-version-file: docs/.node-version" in build_checks, "CI must use the docs Node pin")
    require("bun-version-file: docs/package.json" in build_checks, "CI must use the Bun version from packageManager")
    for path in ["docs/README.md", "docs/wiki/documentation.md"]:
        guide = read(path)
        require(f"Node.js {node}" in guide and f"Bun {bun}" in guide and "Python 3" in guide, f"docs toolchain differs: {path}")
    for path in ["README.md", "docs/README.md", "docs/wiki/documentation.md", "AGENTS.md", ".github/workflows/build-check.yml"]:
        for command in ["check:drift", "check:types", "build", "check:rendered", "check:demo"]:
            require(f"npm run {command}" in read(path), f"missing docs command {command}: {path}")
    require(package["dependencies"]["astro"].startswith("^7."), "update the Astro major-version guide")
    require(package["engines"]["node"] == ">=22.12.0", "update documented Node requirements")
    require(all(option in build_checks for option in ["aquasec/trivy:", "--scanners vuln", "--severity HIGH,CRITICAL", "--ignore-unfixed", "--exit-code 1"]), "container scan claim differs from CI")
    require("--ignore-unfixed" in read("docs/wiki/deployment.md"), "deployment guide misses the Trivy fix policy")
    print(f"Source-backed docs checks passed ({len(labs)} labs, {len(wiki)} guide pages)")


if __name__ == "__main__":
    main()
