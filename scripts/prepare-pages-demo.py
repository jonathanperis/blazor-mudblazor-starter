#!/usr/bin/env python3
"""Place the published WebAssembly demo inside the built docs site for GitHub Pages.

Copies the publish output's wwwroot to <site>/demo/, rewrites <base href> for the Pages path, drops
precompressed copies (GitHub Pages compresses responses itself), and verifies the boot assets exist.
"""
import argparse
import re
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def require(condition, message):
    if not condition:
        raise SystemExit(f"pages demo: {message}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", default=str(ROOT / "artifacts/wasm/wwwroot"), help="published WebAssembly wwwroot")
    parser.add_argument("--site", default=str(ROOT / "docs/out"), help="built docs site")
    parser.add_argument("--base", default="/blazor-mudblazor-starter/demo/", help="URL path the demo is served from")
    args = parser.parse_args()

    source, site = Path(args.source), Path(args.site)
    require(args.base.startswith("/") and args.base.endswith("/"), "--base must start and end with /")
    require((source / "index.html").is_file(), f"missing published index.html in {source}; run dotnet publish first")
    require((site / "index.html").is_file(), f"missing built site in {site}; run the docs build first")
    target = site / "demo"
    if target.exists():
        shutil.rmtree(target)
    shutil.copytree(source, target, ignore=shutil.ignore_patterns("*.br", "*.gz"))

    index = target / "index.html"
    html = index.read_text(encoding="utf-8")
    html, count = re.subn(r'<base href="/" />', f'<base href="{args.base}" />', html)
    require(count == 1, 'expected exactly one <base href="/" /> in the published index.html')
    index.write_text(html, encoding="utf-8")

    scripts = re.findall(r'<script src="([^"]+)"', html)
    require(any(re.fullmatch(r"_framework/blazor\.webassembly\.[a-z0-9]+\.js", src) for src in scripts), "boot script is not fingerprinted")
    for src in scripts + re.findall(r'<link rel="stylesheet" href="([^"]+)"', html):
        require((target / src).is_file(), f"index.html references missing {src}")
    require(any((target / "_framework").glob("dotnet.*.js")), "missing .NET runtime loader")
    require(any((target / "_framework").glob("WebClient.Shared.*.wasm")), "missing shared labs assembly")
    files = [path for path in target.rglob("*") if path.is_file()]
    size = sum(path.stat().st_size for path in files) / 1_048_576
    print(f"WebAssembly demo prepared at {target} for {args.base} ({len(files)} files, {size:.1f} MiB)")


if __name__ == "__main__":
    main()
