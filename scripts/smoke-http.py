#!/usr/bin/env python3
"""Check a running published Blazor Server app through HTTP, without a browser."""
import argparse
import json
import time
from html.parser import HTMLParser
from urllib.error import HTTPError, URLError
from urllib.request import urlopen

# Route and the page heading it must prerender. Lab titles come from LabCatalog.
ROUTES = {
    "/": "Learn by changing working examples.",
    "/labs": "Nine experiments, in order",
    "/components": "Every MudBlazor component, working",
    "/samples": "Whole pages, composed",
    "/components/button": "Button",
    "/samples/dashboard": "Analytics dashboard",
    "/counter": "State and lifecycle",
    "/weather": "DataGrid experiments",
    "/labs/forms": "Forms and transactional dialogs",
    "/labs/api": "API and server paging",
    "/labs/persistence": "SQLite notebook",
    "/labs/auth": "Authentication and policies",
    "/labs/localization": "Localization and accessibility",
    "/labs/files": "Files and cancellable work",
    "/labs/observability": "Observability and deployment",
}
ASSETS = [
    "/_framework/blazor.web.js",
    "/_content/MudBlazor/MudBlazor.min.js",
    "/_content/MudBlazor/MudBlazor.min.css",
    "/_content/WebClient.Shared/learning.js",
    "/_content/WebClient.Shared/app.css",
]


class Page(HTMLParser):
    def __init__(self):
        super().__init__()
        self.headings = []
        self._in_heading = False

    def handle_starttag(self, tag, attrs):
        if tag == "h1":
            self._in_heading = True
            self.headings.append("")

    def handle_endtag(self, tag):
        if tag == "h1":
            self._in_heading = False

    def handle_data(self, data):
        if self._in_heading:
            self.headings[-1] += data


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--base-url", default="http://127.0.0.1:5000")
    parser.add_argument("--attempts", type=int, default=20)
    args = parser.parse_args()

    def get(path):
        url = args.base_url.rstrip("/") + path
        try:
            with urlopen(url, timeout=10) as response:
                return response.read().decode("utf-8")
        except HTTPError as error:
            raise SystemExit(f"GET {path} returned HTTP {error.code}") from None

    for attempt in range(args.attempts):
        try:
            if get("/healthz/ready") == "Healthy":
                break
        except (URLError, TimeoutError, ConnectionError, SystemExit):
            pass
        if attempt == args.attempts - 1:
            raise SystemExit("Application readiness timed out")
        time.sleep(2)

    for route, heading in ROUTES.items():
        page = Page()
        page.feed(get(route))
        if [text.strip() for text in page.headings] != [heading]:
            raise SystemExit(f"Expected one prerendered heading {heading!r} at {route}, found {page.headings}")
        print(f"PASS {route}")
    if get("/healthz") != "Healthy":
        raise SystemExit("Liveness check failed")
    for asset in ASSETS:
        if not get(asset):
            raise SystemExit(f"Empty static asset: {asset}")
    forecasts = json.loads(get("/api/forecasts?count=10&pageSize=3"))
    if forecasts.get("total") != 10 or len(forecasts.get("items", [])) != 3:
        raise SystemExit(f"Unexpected forecast API page: {forecasts}")
    print("Published app HTTP checks passed")


if __name__ == "__main__":
    main()
