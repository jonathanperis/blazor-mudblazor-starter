#!/usr/bin/env python3
"""Check a running published app through HTTP, without a browser."""
import argparse
import time
from html.parser import HTMLParser
from urllib.error import URLError
from urllib.request import urlopen


class Page(HTMLParser):
    def __init__(self):
        super().__init__()
        self.heading = False

    def handle_starttag(self, tag, attrs):
        self.heading |= tag == "h1"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--base-url", default="http://127.0.0.1:5000")
    parser.add_argument("--attempts", type=int, default=20)
    args = parser.parse_args()

    def get(path):
        with urlopen(args.base_url.rstrip("/") + path, timeout=5) as response:
            return response.read().decode("utf-8")

    for attempt in range(args.attempts):
        try:
            if get("/healthz/ready") == "Healthy":
                break
        except (URLError, TimeoutError, ConnectionError):
            pass
        if attempt == args.attempts - 1:
            raise SystemExit("Application readiness timed out")
        time.sleep(2)

    for route in ["/", "/labs", "/counter", "/weather", "/labs/forms", "/labs/api", "/labs/persistence", "/labs/auth", "/labs/localization", "/labs/files", "/labs/observability"]:
        page = Page()
        page.feed(get(route))
        if not page.heading:
            raise SystemExit(f"Missing prerendered heading: {route}")
        print(f"PASS {route}")
    if get("/healthz") != "Healthy":
        raise SystemExit("Liveness check failed")
    for asset in ["/_framework/blazor.web.js", "/_content/MudBlazor/MudBlazor.min.js", "/_content/MudBlazor/MudBlazor.min.css", "/learning.js"]:
        if not get(asset):
            raise SystemExit(f"Missing static asset: {asset}")
    print("Published app HTTP checks passed")


if __name__ == "__main__":
    main()
