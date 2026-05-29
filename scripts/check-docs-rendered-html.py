#!/usr/bin/env python3
"""Smoke-check generated Astro docs HTML.

This guards the Markdown processor switch by asserting the small Pages site still
renders the route set and Markdown features the docs rely on: headings/anchors,
tables, fenced code blocks, and important internal links.
"""

from __future__ import annotations

from html.parser import HTMLParser
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "out"


class FeatureParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.tags: set[str] = set()
        self.ids: set[str] = set()
        self.hrefs: set[str] = set()
        self.text_parts: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        self.tags.add(tag)
        attr = dict(attrs)
        if attr.get("id"):
            self.ids.add(attr["id"] or "")
        if tag == "a" and attr.get("href"):
            self.hrefs.add(attr["href"] or "")

    def handle_data(self, data: str) -> None:
        if data.strip():
            self.text_parts.append(data.strip())

    @property
    def text(self) -> str:
        return "\n".join(self.text_parts)


def parse(route: str) -> FeatureParser:
    path = OUT / route / "index.html" if route else OUT / "index.html"
    if not path.exists():
        raise SystemExit(f"missing generated route: {path.relative_to(ROOT)}")
    parser = FeatureParser()
    parser.feed(path.read_text(encoding="utf-8"))
    return parser


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"rendered docs smoke failed: {message}")


def main() -> None:
    expected_routes = [
        "",
        "docs",
        "docs/components",
        "docs/configuration",
        "docs/getting-started",
        "docs/deployment",
        "docs/project-structure",
    ]
    parsed = {route: parse(route) for route in expected_routes}

    docs = parsed["docs"]
    for section in ["home", "components", "configuration", "getting-started", "deployment", "project-structure"]:
        require(section in docs.ids, f"/docs/ is missing section anchor #{section}")

    require("table" in docs.tags, "/docs/ no longer renders Markdown tables")
    require("pre" in docs.tags and "code" in docs.tags, "/docs/ no longer renders fenced code blocks")
    require(any(href.endswith("/docs/#configuration") for href in docs.hrefs), "/docs/ is missing configuration hash link")
    require("Production-ready Blazor Server starter" in docs.text, "/docs/ missing overview text")

    getting_started = parsed["docs/getting-started"]
    require("pre" in getting_started.tags and "code" in getting_started.tags, "/docs/getting-started/ no longer renders fenced code")
    require("http://localhost:5000" in getting_started.text, "/docs/getting-started/ missing local URL table content")

    deployment = parsed["docs/deployment"]
    require("ghcr.io/jonathanperis/blazor-mudblazor-starter:latest" in deployment.text, "/docs/deployment/ missing GHCR image text")

    print("rendered docs smoke checks passed")


if __name__ == "__main__":
    main()
