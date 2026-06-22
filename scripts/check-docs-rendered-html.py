#!/usr/bin/env python3
"""Smoke-check generated Astro docs HTML.

This guards the Markdown processor switch by asserting the small Pages site still
renders the route set and Markdown features the docs rely on: headings/anchors,
tables, fenced code blocks, and important internal links.
"""

from __future__ import annotations

from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import urldefrag, urlparse

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "out"


class FeatureParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.tags: set[str] = set()
        self.ids: set[str] = set()
        self.hrefs: set[str] = set()
        self.link_texts: set[str] = set()
        self._current_link_parts: list[str] | None = None
        self.text_parts: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        self.tags.add(tag)
        attr = dict(attrs)
        if attr.get("id"):
            self.ids.add(attr["id"] or "")
        if tag == "a" and attr.get("href"):
            self.hrefs.add(attr["href"] or "")
            self._current_link_parts = []

    def handle_endtag(self, tag: str) -> None:
        if tag == "a" and self._current_link_parts is not None:
            text = " ".join(part for part in self._current_link_parts if part).strip()
            if text:
                self.link_texts.add(text)
            self._current_link_parts = None

    def handle_data(self, data: str) -> None:
        if data.strip():
            stripped = data.strip()
            self.text_parts.append(stripped)
            if self._current_link_parts is not None:
                self._current_link_parts.append(stripped)

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



def route_for_href(current_route: str, href: str) -> tuple[str, str | None] | None:
    href, fragment = urldefrag(href)
    parsed = urlparse(href)
    if parsed.scheme or parsed.netloc or href.startswith(("mailto:", "tel:")):
        return None
    if href.startswith("/blazor-mudblazor-starter/"):
        href = href[len("/blazor-mudblazor-starter/"):].strip("/")
    elif href.startswith("/"):
        href = href.strip("/")
    else:
        base_parts = current_route.split("/") if current_route else []
        if base_parts and base_parts[-1] != "":
            base_parts = base_parts[:-1] if current_route.endswith(".html") else base_parts
        parts: list[str] = base_parts.copy()
        for part in href.split("/"):
            if part in ("", "."):
                continue
            if part == "..":
                if parts:
                    parts.pop()
            else:
                parts.append(part)
        href = "/".join(parts)
    href = href.removesuffix("index.html").strip("/")
    return href, fragment or None


def check_internal_links(parsed: dict[str, FeatureParser]) -> None:
    routes = set(parsed)
    for route, parser in parsed.items():
        for href in parser.hrefs:
            target = route_for_href(route, href)
            if target is None:
                continue
            target_route, fragment = target
            if target_route.startswith("_astro/") or target_route in {"favicon.ico", "favicon.png", "robots.txt", "sitemap-index.xml", "sitemap-0.xml"}:
                continue
            require(target_route in routes, f"{route or '/'} has broken internal link {href!r} -> {target_route!r}")
            if fragment:
                require(fragment in parsed[target_route].ids, f"{route or '/'} links to missing fragment {href!r}")

def main() -> None:
    expected_routes = [
        "",
        "docs",
        "docs/components",
        "docs/configuration",
        "docs/getting-started",
        "docs/deployment",
        "docs/documentation",
        "docs/project-structure",
    ]
    parsed = {route: parse(route) for route in expected_routes}

    docs = parsed["docs"]
    for section in ["home", "components", "configuration", "getting-started", "deployment", "documentation", "project-structure"]:
        require(section in docs.ids, f"/docs/ is missing section anchor #{section}")

    require("table" in docs.tags, "/docs/ no longer renders Markdown tables")
    require("pre" in docs.tags and "code" in docs.tags, "/docs/ no longer renders fenced code blocks")
    require(any(href.endswith("/docs/#configuration") for href in docs.hrefs), "/docs/ is missing configuration hash link")
    require(any("documentation" in href for href in docs.hrefs), "/docs/ is missing documentation page link")
    require("Production-ready Blazor Server starter" in docs.text, "/docs/ missing overview text")

    getting_started = parsed["docs/getting-started"]
    require("pre" in getting_started.tags and "code" in getting_started.tags, "/docs/getting-started/ no longer renders fenced code")
    require("http://localhost:5000" in getting_started.text, "/docs/getting-started/ missing local URL table content")
    require(any(href.endswith("/docs/configuration/") or href.endswith("../configuration/") for href in getting_started.hrefs), "/docs/getting-started/ missing working configuration link")

    deployment = parsed["docs/deployment"]
    require("ghcr.io/jonathanperis/blazor-mudblazor-starter:latest" in deployment.text, "/docs/deployment/ missing GHCR image text")
    require("APPLICATIONINSIGHTS_CONNECTION_STRING" in deployment.text, "/docs/deployment/ missing Application Insights settings")

    documentation = parsed["docs/documentation"]
    require("Documentation" in documentation.link_texts, "/docs/documentation/ sidebar is missing Documentation link label")
    require("Astro 7" in documentation.text, "/docs/documentation/ missing Astro 7 note")
    require("Vite 8" in documentation.text, "/docs/documentation/ missing Vite 8 note")
    require("Sätteri" in documentation.text, "/docs/documentation/ missing Sätteri note")
    require("npm run check:rendered" in documentation.text, "/docs/documentation/ missing rendered check command")

    check_internal_links(parsed)

    print("rendered docs smoke checks passed")


if __name__ == "__main__":
    main()
