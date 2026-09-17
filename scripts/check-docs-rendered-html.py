#!/usr/bin/env python3
"""Check rendered identity, Markdown, local references and complete sitemap coverage."""
import json
import xml.etree.ElementTree as ET
from collections import Counter
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urljoin, urlsplit

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs/out"
BASE = "https://jonathanperis.github.io/blazor-mudblazor-starter/"
SOURCE = "https://github.com/jonathanperis/blazor-mudblazor-starter/blob/main/"
PRODUCT = "Blazor learning sandbox"


class Page(HTMLParser):
    def __init__(self):
        super().__init__()
        self.ids = Counter()
        self.tags = Counter()
        self.links = []
        self.assets = []
        self.metadata = {}
        self.canonical = None
        self.structured_data = ""
        self._in_structured_data = False

    def handle_starttag(self, tag, attrs):
        self.tags[tag] += 1
        attributes = dict(attrs)
        if attributes.get("id"):
            self.ids[attributes["id"]] += 1
        if tag == "a" and attributes.get("href"):
            self.links.append(attributes["href"])
        if tag == "meta":
            self.metadata[attributes.get("name") or attributes.get("property")] = attributes.get("content")
        if tag == "link":
            if attributes.get("rel") == "canonical":
                self.canonical = attributes.get("href")
            elif attributes.get("href"):
                self.assets.append(attributes["href"])
        if tag in {"script", "img", "source"} and attributes.get("src"):
            self.assets.append(attributes["src"])
        if tag == "script" and attributes.get("type") == "application/ld+json":
            self._in_structured_data = True

    def handle_data(self, data):
        if self._in_structured_data:
            self.structured_data += data

    def handle_endtag(self, tag):
        if tag == "script":
            self._in_structured_data = False


def require(condition, message):
    if not condition:
        raise SystemExit(f"rendered docs: {message}")


def local_reference(href, route=""):
    target = urlsplit(urljoin(BASE + (route + "/" if route else ""), href))
    base = urlsplit(BASE)
    if target.netloc != base.netloc or not target.path.startswith(base.path):
        return None
    relative = unquote(target.path.removeprefix(base.path)).removesuffix("index.html").strip("/")
    return relative, unquote(target.fragment)


def main():
    routes = ["", "docs", *(f"docs/{path.stem}" for path in sorted((ROOT / "docs/wiki").glob("*.md")) if path.stem != "home")]
    pages = {}
    for route in routes:
        path = OUT / route / "index.html"
        require(path.is_file(), f"missing route {route}")
        page = Page()
        html = path.read_text(encoding="utf-8")
        page.feed(html)
        require(page.tags["h1"] == 1, f"{route or '/'} must have one page heading")
        require("main-content" in page.ids, f"{route or '/'} missing skip-link target")
        require(all(count == 1 for count in page.ids.values()), f"duplicate IDs in {route or '/'}")
        require("Blazor MudBlazor Starter" not in html, f"stale product identity in {route or '/'}")
        require(page.metadata.get("og:site_name") == PRODUCT, f"product metadata differs in {route or '/'}")
        require(json.loads(page.structured_data).get("name") == PRODUCT, f"structured product identity differs in {route or '/'}")
        description = page.metadata.get("description")
        require(description and description == page.metadata.get("og:description") == page.metadata.get("twitter:description"), f"description metadata differs in {route or '/'}")
        require(page.canonical == BASE + (route + "/" if route else ""), f"canonical URL differs in {route or '/'}")
        pages[route] = page

    descriptions = [page.metadata["description"] for route, page in pages.items() if route.startswith("docs")]
    require(len(set(descriptions)) == len(descriptions), "guide pages must have distinct topic descriptions")
    for route, page in pages.items():
        for href in page.links:
            if href.startswith(SOURCE):
                source = unquote(urlsplit(href).path.removeprefix(urlsplit(SOURCE).path))
                require((ROOT / source).is_file(), f"{route or '/'} links to missing repository source {href}")
            target = local_reference(href, route)
            if target is None:
                continue
            relative, fragment = target
            if relative not in pages:
                require((OUT / relative).is_file(), f"{route or '/'} links to missing {href}")
            elif fragment:
                require(fragment in pages[relative].ids, f"{route or '/'} links to missing anchor {href}")
        for href in page.assets:
            target = local_reference(href, route)
            if target is not None:
                require((OUT / target[0]).is_file(), f"{route or '/'} references missing asset {href}")

    for route in ["docs/getting-started", "docs/configuration", "docs/deployment"]:
        require(pages[route].tags["pre"] and pages[route].tags["code"] and pages[route].tags["table"], f"Markdown features missing in {route}")
    robots = (OUT / "robots.txt").read_text()
    require(f"Sitemap: {BASE}sitemap-index.xml" in robots and (OUT / "sitemap-index.xml").is_file(), "robots sitemap differs from generated artifact")
    index = ET.parse(OUT / "sitemap-index.xml")
    sitemap_routes = []
    for location in index.findall(".//{*}sitemap/{*}loc"):
        target = local_reference(location.text)
        if target is None or not (OUT / target[0]).is_file():
            raise SystemExit(f"rendered docs: missing sitemap file {location.text}")
        for entry in ET.parse(OUT / target[0]).findall(".//{*}url/{*}loc"):
            page = local_reference(entry.text)
            if page is None or page[0] not in pages:
                raise SystemExit(f"rendered docs: sitemap references missing route {entry.text}")
            sitemap_routes.append(page[0])
    require(sorted(sitemap_routes) == sorted(pages), "sitemap must list every generated route exactly once")
    print(f"Rendered docs checks passed ({len(pages)} routes, identity, descriptions, links/assets, source paths, Markdown, sitemap)")


if __name__ == "__main__":
    main()
