#!/usr/bin/env python3
from __future__ import annotations

import json
import re
import sys
import time
import urllib.parse
import urllib.request
from pathlib import Path


API_URL = "https://commons.wikimedia.org/w/api.php"
USER_AGENT = "DiplomaRealtyDataset/1.0"
TARGET_COUNT_PER_CATEGORY = 10
DOWNLOAD_DELAY_SECONDS = 4.0

CATEGORY_QUERIES = {
    "apartments": [
        "apartment building Minsk Belarus",
        "residential building Minsk Belarus",
        "panel house Minsk Belarus",
        "apartment block Belarus",
    ],
    "houses": [
        "house Minsk Belarus",
        "house Belarus",
        "cottage Belarus",
        "wooden house Belarus",
    ],
    "commercial": [
        "shopping center Minsk Belarus",
        "commercial building Minsk Belarus",
        "retail building Belarus",
    ],
    "offices": [
        "office building Minsk Belarus",
        "business center Minsk Belarus",
        "office building Belarus",
    ],
    "warehouses": [
        "warehouse Minsk Belarus",
        "industrial building Belarus",
        "logistics center Belarus",
    ],
    "land": [
        "suburb Minsk Belarus",
        "residential land Belarus",
        "field Belarus",
        "village landscape Belarus",
    ],
}


def api_get(params: dict[str, str]) -> dict:
    query = urllib.parse.urlencode(params)
    req = urllib.request.Request(
        f"{API_URL}?{query}",
        headers={"User-Agent": USER_AGENT},
    )
    with urllib.request.urlopen(req, timeout=60) as response:
        return json.loads(response.read().decode("utf-8"))


def search_files(query: str, limit: int = 15) -> list[str]:
    payload = api_get(
        {
            "action": "query",
            "list": "search",
            "srsearch": query,
            "srnamespace": "6",
            "srlimit": str(limit),
            "format": "json",
        }
    )
    return [item["title"] for item in payload.get("query", {}).get("search", [])]


def get_file_details(titles: list[str]) -> list[dict]:
    if not titles:
        return []

    payload = api_get(
        {
            "action": "query",
            "prop": "imageinfo|info",
            "titles": "|".join(titles),
            "iiprop": "url|extmetadata",
            "iiurlwidth": "1200",
            "format": "json",
        }
    )

    pages = payload.get("query", {}).get("pages", {})
    results: list[dict] = []
    for page in pages.values():
        imageinfo = page.get("imageinfo", [])
        if not imageinfo:
            continue

        info = imageinfo[0]
        url = info.get("thumburl") or info.get("url")
        if not url:
            continue

        title = page.get("title")
        if not is_supported_image_title(title):
            continue

        extmetadata = info.get("extmetadata", {})
        license_short = extmetadata.get("LicenseShortName", {}).get("value")
        artist = extmetadata.get("Artist", {}).get("value")
        image_desc = extmetadata.get("ImageDescription", {}).get("value")

        results.append(
            {
                "title": page.get("title"),
                "url": url,
                "license": strip_html(license_short),
                "artist": strip_html(artist),
                "description": strip_html(image_desc),
                "source": f"https://commons.wikimedia.org/wiki/{urllib.parse.quote(page.get('title', '').replace(' ', '_'))}",
            }
        )
    return results


def strip_html(value: str | None) -> str | None:
    if not value:
        return None
    no_tags = re.sub(r"<[^>]+>", " ", value)
    return re.sub(r"\s+", " ", no_tags).strip()


def slugify(value: str) -> str:
    value = value.lower()
    value = re.sub(r"[^a-z0-9]+", "-", value)
    return value.strip("-") or "image"


def download_file(url: str, destination: Path) -> None:
    req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(req, timeout=120) as response:
        destination.write_bytes(response.read())


def choose_extension(url: str) -> str:
    parsed = urllib.parse.urlparse(url)
    suffix = Path(parsed.path).suffix.lower()
    return suffix if suffix in {".jpg", ".jpeg", ".png", ".webp"} else ".jpg"


def is_supported_image_title(title: str | None) -> bool:
    if not title:
        return False

    normalized = title.lower()
    return normalized.endswith((".jpg", ".jpeg", ".png", ".webp"))


def load_manifest(path: Path) -> list[dict]:
    if not path.exists():
        return []

    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except Exception:  # noqa: BLE001
        return []


def main() -> int:
    if len(sys.argv) not in {2, 3}:
        print("usage: fetch_realty_dataset.py <target_dir> [category]", file=sys.stderr)
        return 1

    target_dir = Path(sys.argv[1]).expanduser().resolve()
    requested_category = sys.argv[2] if len(sys.argv) == 3 else None
    if requested_category and requested_category not in CATEGORY_QUERIES:
        print(f"unknown category: {requested_category}", file=sys.stderr)
        return 1

    target_dir.mkdir(parents=True, exist_ok=True)
    (target_dir / "metadata").mkdir(exist_ok=True)

    manifest_path = target_dir / "metadata" / "manifest.json"
    manifest: list[dict] = load_manifest(manifest_path)
    categories = (
        {requested_category: CATEGORY_QUERIES[requested_category]}
        if requested_category
        else CATEGORY_QUERIES
    )

    for category, queries in categories.items():
        category_dir = target_dir / category
        category_dir.mkdir(exist_ok=True)

        existing_files = sorted(path.name for path in category_dir.iterdir() if path.is_file())
        seen_urls = {
            entry["imageUrl"]
            for entry in manifest
            if entry.get("category") == category and entry.get("imageUrl")
        }
        saved = len(existing_files)

        for query in queries:
            if saved >= TARGET_COUNT_PER_CATEGORY:
                break

            titles = search_files(query, limit=20)
            details = get_file_details(titles)

            for item in details:
                if saved >= TARGET_COUNT_PER_CATEGORY:
                    break

                url = item["url"]
                if url in seen_urls:
                    continue

                seen_urls.add(url)
                ext = choose_extension(url)
                filename = f"{saved + 1:02d}-{slugify(item['title'] or query)[:80]}{ext}"
                destination = category_dir / filename
                if destination.exists():
                    saved += 1
                    continue

                try:
                    download_file(url, destination)
                except Exception as exc:  # noqa: BLE001
                    print(f"[warn] failed to download {url}: {exc}", file=sys.stderr)
                    continue

                manifest.append(
                    {
                        "category": category,
                        "fileName": filename,
                        "query": query,
                        "title": item["title"],
                        "sourceUrl": item["source"],
                        "imageUrl": url,
                        "license": item["license"],
                        "artist": item["artist"],
                        "description": item["description"],
                        "addressHint": None,
                    }
                )
                saved += 1
                manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
                time.sleep(DOWNLOAD_DELAY_SECONDS)

        print(f"{category}: {saved} files")

    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    notes_path = target_dir / "metadata" / "notes.txt"
    notes_path.write_text(
        "Dataset is illustrative, not exact-by-address.\n"
        "Use manifest.json to map images to listings during DB seeding.\n"
        "Fill addressHint later if you want manual matching for specific Minsk addresses.\n",
        encoding="utf-8",
    )

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
