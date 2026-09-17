#!/usr/bin/env python3
"""Check rendered Markdown features, unique IDs, internal links, and sitemap."""
from collections import Counter
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urljoin, urlsplit

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs/out"
BASE = "https://jonathanperis.github.io/blazor-mudblazor-starter/"


class Page(HTMLParser):
    def __init__(self):
        super().__init__()
        self.ids = Counter()
        self.tags = Counter()
        self.links = []

    def handle_starttag(self, tag, attrs):
        self.tags[tag] += 1
        attributes = dict(attrs)
        if attributes.get("id"):
            self.ids[attributes["id"]] += 1
        if tag == "a" and attributes.get("href"):
            self.links.append(attributes["href"])


def require(condition, message):
    if not condition:
        raise SystemExit(f"rendered docs: {message}")


def main():
    routes = ["", "docs", *(f"docs/{path.stem}" for path in sorted((ROOT / "docs/wiki").glob("*.md")) if path.stem != "home")]
    pages = {}
    for route in routes:
        path = OUT / route / "index.html"
        require(path.is_file(), f"missing route {route}")
        page = Page()
        page.feed(path.read_text(encoding="utf-8"))
        require(page.tags["h1"] == 1, f"{route or '/'} must have one page heading")
        require("main-content" in page.ids, f"{route or '/'} missing skip-link target")
        require(all(count == 1 for count in page.ids.values()), f"duplicate IDs in {route or '/'}")
        pages[route] = page

    for route, page in pages.items():
        for href in page.links:
            target = urlsplit(urljoin(BASE + (route + "/" if route else ""), href))
            if target.netloc != "jonathanperis.github.io" or not target.path.startswith("/blazor-mudblazor-starter/"):
                continue
            relative = unquote(target.path.removeprefix("/blazor-mudblazor-starter/")).removesuffix("index.html").strip("/")
            if relative not in pages:
                require((OUT / relative).is_file(), f"{route or '/'} links to missing {href}")
            elif target.fragment:
                require(unquote(target.fragment) in pages[relative].ids, f"{route or '/'} links to missing anchor {href}")

    for route in ["docs/getting-started", "docs/configuration", "docs/deployment"]:
        require(pages[route].tags["pre"] and pages[route].tags["code"] and pages[route].tags["table"], f"Markdown features missing in {route}")
    robots = (OUT / "robots.txt").read_text()
    require(f"Sitemap: {BASE}sitemap-index.xml" in robots and (OUT / "sitemap-index.xml").is_file(), "robots sitemap differs from generated artifact")
    print(f"Rendered docs checks passed ({len(pages)} routes, links, unique IDs, Markdown, sitemap)")


if __name__ == "__main__":
    main()
