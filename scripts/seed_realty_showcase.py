#!/usr/bin/env python3
import atexit
import json
import os
import socket
import subprocess
import sys
import time
import urllib.error
import urllib.request
import uuid
from datetime import datetime, timedelta, timezone
from pathlib import Path

API_BASE = os.environ.get("API_BASE", "http://diploma.com:5102")
OUTPUT_FILE = Path(os.environ.get("OUTPUT_FILE", "/home/Admin/Projects/Diploma/tmp/minsk_seed_results.jsonl"))
PHOTO_ROOT = Path(os.environ.get("PHOTO_ROOT", "/home/Admin/Houses/DIploma"))
PHOTO_DATASET_ROOT = PHOTO_ROOT / "Realty_Datasets"
PHOTO_WEB_ROOT = Path(os.environ.get("PHOTO_WEB_ROOT", "/tmp/diploma_photo_web"))
PHOTO_BASE_URL = os.environ.get("PHOTO_BASE_URL", "/seed-images")
MARKER = "[AUTO-SEED]"
SEED_PROFILE = os.environ.get("SEED_PROFILE", "showcase").strip().lower()
PHOTO_SERVER_PORT = int(os.environ.get("PHOTO_SERVER_PORT", "9001"))
PHOTO_SERVER_HOST = os.environ.get("PHOTO_SERVER_HOST", "127.0.0.1")

OLD_TITLES = {
    "2-комнатная квартира у метро Восток",
    "Дом в зеленой части Минска",
    "Офис в бизнес-центре на Победителей",
    "Складской комплекс в промышленной зоне Минска",
    "Участок под коммерческий проект в Минске",
    "3-комнатная квартира рядом с парком Челюскинцев",
    "Семейный дом с террасой в тихом районе",
    "Логистический склад с рампой и офисным блоком",
    "Офис с шоурумом возле метро Академия наук",
}

PORT_FORWARD_PROCESS = None
PORT_FORWARD_LOG = Path("/tmp/diploma_gateway_port_forward.log")
PHOTO_SERVER_PROCESS = None
PHOTO_SERVER_LOG = Path("/tmp/diploma_photo_server.log")
PHOTO_URLS: dict[Path, str] = {}
USED_PHOTOS: set[Path] = set()


def log_output(entry: dict) -> None:
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    with OUTPUT_FILE.open("a", encoding="utf-8") as handle:
        handle.write(json.dumps(entry, ensure_ascii=False) + "\n")


def is_port_open(host: str, port: int) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.settimeout(0.5)
        return sock.connect_ex((host, port)) == 0


def wait_for_http(url: str, timeout_seconds: int = 30) -> bool:
    deadline = time.time() + timeout_seconds
    while time.time() < deadline:
        try:
            urllib.request.urlopen(url, timeout=2)
            return True
        except Exception:
            time.sleep(1)
    return False


def api_request(method: str, path: str, *, token: str | None = None, body=None, content_type: str = "application/json"):
    url = f"{API_BASE}{path}"
    headers = {}
    data = None

    if token:
        headers["Authorization"] = f"Bearer {token}"

    if body is not None:
        headers["Content-Type"] = content_type
        if isinstance(body, (dict, list)):
            data = json.dumps(body, ensure_ascii=False).encode("utf-8")
        elif isinstance(body, str):
            data = body.encode("utf-8")
        else:
            data = body

    request = urllib.request.Request(url, data=data, headers=headers, method=method)
    with urllib.request.urlopen(request, timeout=60) as response:
        payload = response.read().decode("utf-8")
        return response.status, payload


def safe_api_request(method: str, path: str, *, token: str | None = None, body=None):
    try:
        status, payload = api_request(method, path, token=token, body=body)
        return status, payload
    except urllib.error.HTTPError as error:
        payload = error.read().decode("utf-8", errors="replace")
        return error.code, payload


def discover_gateway_service() -> str:
    result = subprocess.run(
        ["kubectl", "get", "svc", "-n", "envoy-gateway-system", "-o", "json", "--context", "kind-local-diploma"],
        check=True,
        capture_output=True,
        text=True,
    )
    services = json.loads(result.stdout)["items"]
    for service in services:
        name = service["metadata"]["name"]
        if name.startswith("envoy-diploma-diploma-gateway-"):
            return name
    raise RuntimeError("Envoy Gateway service was not found.")


def ensure_port_forward() -> None:
    global PORT_FORWARD_PROCESS

    if is_port_open("127.0.0.1", 5102):
        return

    service_name = discover_gateway_service()
    PORT_FORWARD_LOG.parent.mkdir(parents=True, exist_ok=True)
    log_handle = PORT_FORWARD_LOG.open("w", encoding="utf-8")
    PORT_FORWARD_PROCESS = subprocess.Popen(
        [
            "kubectl",
            "port-forward",
            "-n",
            "envoy-gateway-system",
            f"service/{service_name}",
            "5102:8080",
            "--context",
            "kind-local-diploma",
        ],
        stdout=log_handle,
        stderr=subprocess.STDOUT,
        text=True,
    )

    def cleanup_port_forward() -> None:
        if PORT_FORWARD_PROCESS and PORT_FORWARD_PROCESS.poll() is None:
            PORT_FORWARD_PROCESS.terminate()
            try:
                PORT_FORWARD_PROCESS.wait(timeout=5)
            except subprocess.TimeoutExpired:
                PORT_FORWARD_PROCESS.kill()

    atexit.register(cleanup_port_forward)

    deadline = time.time() + 20
    while time.time() < deadline:
        if is_port_open("127.0.0.1", 5102):
            return
        if PORT_FORWARD_PROCESS.poll() is not None:
            break
        time.sleep(1)

    raise RuntimeError(f"Failed to start port-forward on 5102. See {PORT_FORWARD_LOG}.")


def ensure_photo_server() -> None:
    global PHOTO_SERVER_PROCESS

    prepare_web_photos()

    if is_port_open(PHOTO_SERVER_HOST, PHOTO_SERVER_PORT):
        return

    PHOTO_SERVER_LOG.parent.mkdir(parents=True, exist_ok=True)
    log_handle = PHOTO_SERVER_LOG.open("w", encoding="utf-8")
    PHOTO_SERVER_PROCESS = subprocess.Popen(
        [
            "python3",
            "-m",
            "http.server",
            str(PHOTO_SERVER_PORT),
            "--bind",
            PHOTO_SERVER_HOST,
            "--directory",
            str(PHOTO_WEB_ROOT),
        ],
        stdout=log_handle,
        stderr=subprocess.STDOUT,
        text=True,
        start_new_session=True,
    )

    deadline = time.time() + 10
    while time.time() < deadline:
        if is_port_open(PHOTO_SERVER_HOST, PHOTO_SERVER_PORT):
            return
        if PHOTO_SERVER_PROCESS.poll() is not None:
            break
        time.sleep(1)

    raise RuntimeError(f"Failed to start photo server on {PHOTO_SERVER_PORT}. See {PHOTO_SERVER_LOG}.")


def sign_in() -> str:
    status, payload = safe_api_request(
        "POST",
        "/api/users/signin",
        body={"email": "gromahacer@gmail.com", "password": "12345678"},
    )
    parsed = json.loads(payload)
    log_output({"step": "signin", "httpStatus": status, "response": parsed})
    token = parsed.get("data", {}).get("token")
    if not token:
        raise RuntimeError(f"Sign-in failed: {payload}")
    return token


def get_all_listings(path: str) -> list[dict]:
    listings: list[dict] = []
    page = 0
    while True:
        status, payload = safe_api_request("GET", f"{path}?page={page}&pageSize=100")
        if status != 200:
            raise RuntimeError(f"Failed to fetch listings from {path}: {payload}")
        parsed = json.loads(payload)
        items = parsed.get("data", {}).get("items", [])
        if not items:
            break
        listings.extend(items)
        page += 1
    return listings


def get_all_detailed_listings() -> list[dict]:
    try:
        return get_all_listings("/api/listings/details")
    except RuntimeError as error:
        message = str(error)
        if "connection refused" not in message.lower():
            raise
        return get_all_listings("/api/listings")


def cleanup_old_listings(token: str) -> None:
    deleted: list[dict] = []
    for listing in get_all_detailed_listings():
        title = listing.get("title", "")
        description = listing.get("description", "") or ""
        if title in OLD_TITLES or MARKER in description:
            listing_id = listing["id"]
            status, payload = safe_api_request("DELETE", f"/api/listings/{listing_id}", token=token)
            deleted.append({"id": listing_id, "title": title, "httpStatus": status, "response": try_parse_json(payload)})
    log_output({"step": "cleanup_old", "deleted": deleted})


def try_parse_json(payload: str):
    try:
        return json.loads(payload)
    except Exception:
        return payload


def category_dir(category: str) -> Path:
    mapping = {
        "apartment": PHOTO_DATASET_ROOT / "apartments",
        "house": PHOTO_DATASET_ROOT / "houses",
        "office": PHOTO_DATASET_ROOT / "offices",
        "warehouse": PHOTO_DATASET_ROOT / "warehouses",
        "land": PHOTO_DATASET_ROOT / "land",
    }
    return mapping[category]


def category_photo_dirs(category: str) -> list[Path]:
    primary = category_dir(category)
    shared_commercial = PHOTO_DATASET_ROOT / "commercial"
    uncategorized = PHOTO_DATASET_ROOT / "not_category"

    if category in {"office", "warehouse", "land"}:
        return [primary, shared_commercial, uncategorized]

    return [primary]


def photo_url(path: Path) -> str:
    return PHOTO_URLS[path]


def build_photo_url(file_name: str) -> str:
    return f"{PHOTO_BASE_URL.rstrip('/')}/{file_name}"


def prepare_web_photos() -> None:
    PHOTO_WEB_ROOT.mkdir(parents=True, exist_ok=True)
    PHOTO_URLS.clear()

    prefixes = {
        "apartments": "a",
        "commercial": "c",
        "houses": "h",
        "offices": "o",
        "warehouses": "w",
        "land": "l",
        "not_category": "n",
    }

    for category_name, prefix in prefixes.items():
        files = sorted((PHOTO_DATASET_ROOT / category_name).iterdir())
        counter = 1
        for source in files:
            if not source.is_file():
                continue
            extension = source.suffix.lower() or ".jpg"
            link_name = f"{prefix}{counter:02d}{extension}"
            target = PHOTO_WEB_ROOT / link_name
            if target.exists() or target.is_symlink():
                target.unlink()
            target.symlink_to(source)
            PHOTO_URLS[source] = build_photo_url(link_name)
            counter += 1


def rotate_take(items: list[Path], start: int, count: int) -> list[Path]:
    if not items:
        return []
    selected = []
    for offset in range(len(items)):
        candidate = items[(start + offset) % len(items)]
        if candidate not in selected:
            selected.append(candidate)
        if len(selected) == count:
            break
    return selected


def ordered_unique(items: list[Path], start: int) -> list[Path]:
    return rotate_take(items, start, len(items))


def pick_images(category: str, city: str, listing_index: int, count: int = 4) -> list[str]:
    files: list[Path] = []
    for directory in category_photo_dirs(category):
        files.extend(sorted(path for path in directory.iterdir() if path.is_file()))

    lowered_city = city.lower()
    minsk_files = [path for path in files if "minsk" in path.name.lower()]
    region_files = [path for path in files if "regions" in path.name.lower()]
    generic_files = [path for path in files if path not in minsk_files and path not in region_files]

    if lowered_city == "минск":
        primary = minsk_files or generic_files or files
        secondary = generic_files or region_files or files
    else:
        primary = region_files or generic_files or files
        secondary = generic_files or minsk_files or files

    selection_order: list[Path] = []
    for pool in (
        ordered_unique(primary, listing_index * 2),
        ordered_unique(secondary, listing_index * 3),
        ordered_unique(files, listing_index * 5),
    ):
        for candidate in pool:
            if candidate not in selection_order:
                selection_order.append(candidate)

    available = [candidate for candidate in selection_order if candidate not in USED_PHOTOS]
    if len(available) < count:
        raise RuntimeError(
            f"Not enough unique photos for category '{category}'. "
            f"Need {count}, but only {len(available)} unused files remain."
        )

    selected = available[:count]
    USED_PHOTOS.update(selected)

    return [photo_url(path) for path in selected[:count]]


def apartment_payload(
    city: str,
    address: str,
    title: str,
    description: str,
    media_urls: list[str],
    price_amount_byn: int,
    published_date: str,
) -> dict:
    return {
        "listingDto": {
            "agentId": "00000000-0000-0000-0000-000000000000",
            "addressText": address,
            "title": title,
            "description": description,
            "priceAmountByn": price_amount_byn,
            "priceCurrency": "BYN",
            "pricePeriod": "OneTime",
            "areaTotal": 61.4,
            "buildYear": 2018,
            "publishedDate": published_date,
            "addressId": str(uuid.uuid4()),
            "buildingType": "Apartment Building (Multi-family)",
            "listingType": "Sale",
            "status": "Active",
            "listingDetailsType": "Apartment",
            "mediaUrls": media_urls,
            "listingDetails": {
                "rooms": 2,
                "bedrooms": 1,
                "floorsTotal": 16,
                "levels": 1,
                "floor": 7,
                "areaLiving": 33.0,
                "areaKitchen": 10.2,
                "hasBalcony": True,
                "hasLoggia": False,
                "hasElevator": True,
                "ceilingHeight": 2.7,
                "bathroomType": "Separate",
                "renovationType": "Finished / Turnkey",
                "buildingMaterial": "Monolith",
            },
        }
    }


def house_payload(
    address: str,
    title: str,
    description: str,
    media_urls: list[str],
    price_amount_byn: int,
    published_date: str,
) -> dict:
    return {
        "listingDto": {
            "agentId": "00000000-0000-0000-0000-000000000000",
            "addressText": address,
            "title": title,
            "description": description,
            "priceAmountByn": price_amount_byn,
            "priceCurrency": "BYN",
            "pricePeriod": "OneTime",
            "areaTotal": 154.0,
            "buildYear": 2016,
            "publishedDate": published_date,
            "addressId": str(uuid.uuid4()),
            "buildingType": "Detached House",
            "listingType": "Sale",
            "status": "Active",
            "listingDetailsType": "House",
            "mediaUrls": media_urls,
            "listingDetails": {
                "rooms": 5,
                "bedrooms": 3,
                "floors": 2,
                "houseArea": 154.0,
                "plotArea": 8.4,
                "hasElectricity": True,
                "hasGarage": True,
                "hasBasement": False,
                "hasFence": True,
                "wallMaterial": "Brick",
                "heatingType": "Individual gas boiler",
                "sewerType": "Septic tank",
                "gasType": "Central gas",
                "waterSupplyType": "Borehole",
            },
        }
    }


def office_payload(
    address: str,
    title: str,
    description: str,
    media_urls: list[str],
    price_amount_byn: int,
    published_date: str,
) -> dict:
    return {
        "listingDto": {
            "agentId": "00000000-0000-0000-0000-000000000000",
            "addressText": address,
            "title": title,
            "description": description,
            "priceAmountByn": price_amount_byn,
            "priceCurrency": "BYN",
            "pricePeriod": "Monthly",
            "areaTotal": 156.0,
            "buildYear": 2021,
            "publishedDate": published_date,
            "addressId": str(uuid.uuid4()),
            "buildingType": "Business Center",
            "listingType": "Rent",
            "status": "Active",
            "listingDetailsType": "Office",
            "mediaUrls": media_urls,
            "listingDetails": {
                "plotArea": 156.0,
                "floor": 4,
                "floorsTotal": 10,
                "levels": 1,
                "parkingSpaces": 4,
                "kitchenette": True,
                "officeClass": "B+",
                "layoutType": "Mixed",
                "renovationType": "Finished / Turnkey",
                "parkingType": "Cabinet / rooms",
            },
        }
    }


def warehouse_payload(
    address: str,
    title: str,
    description: str,
    media_urls: list[str],
    price_amount_byn: int,
    published_date: str,
) -> dict:
    return {
        "listingDto": {
            "agentId": "00000000-0000-0000-0000-000000000000",
            "addressText": address,
            "title": title,
            "description": description,
            "priceAmountByn": price_amount_byn,
            "priceCurrency": "BYN",
            "pricePeriod": "OneTime",
            "areaTotal": 2200.0,
            "buildYear": 2019,
            "publishedDate": published_date,
            "addressId": str(uuid.uuid4()),
            "buildingType": "Warehouse Complex",
            "listingType": "Sale",
            "status": "Active",
            "listingDetailsType": "Warehouse",
            "mediaUrls": media_urls,
            "listingDetails": {
                "plotArea": 3800.0,
                "ceilingHeight": 10.8,
                "floorLoadTonsPerSquareMeter": 5.8,
                "gateCount": 4,
                "hasDockLevelers": True,
                "dockCount": 2,
                "hasRamps": True,
                "powerKw": 240.0,
                "hasOffice": True,
                "officeArea": 120.0,
                "classType": "A",
                "heatingType": "Central",
            },
        }
    }


def land_payload(
    address: str,
    title: str,
    description: str,
    media_urls: list[str],
    price_amount_byn: int,
    published_date: str,
) -> dict:
    return {
        "listingDto": {
            "agentId": "00000000-0000-0000-0000-000000000000",
            "addressText": address,
            "title": title,
            "description": description,
            "priceAmountByn": price_amount_byn,
            "priceCurrency": "BYN",
            "pricePeriod": "OneTime",
            "areaTotal": 14.2,
            "publishedDate": published_date,
            "addressId": str(uuid.uuid4()),
            "buildingType": "Other",
            "listingType": "Sale",
            "status": "Active",
            "listingDetailsType": "Land",
            "mediaUrls": media_urls,
            "listingDetails": {
                "plotArea": 14.2,
                "allowedUse": "Mixed-use development",
                "hasElectricity": True,
                "hasGas": True,
                "hasWater": True,
                "hasSewer": True,
                "hasBuildings": False,
                "landCategory": "Industrial / commercial",
                "roadAccessType": "Asphalt",
            },
        }
    }


SHOWCASE_DEFINITIONS = [
    {"category": "apartment", "city": "Минск", "address": "Минск, улица Петра Мстиславца, 18", "title": "Квартира у метро Восток", "published_date": "2026-04-02T09:00:00Z", "price_amount_byn": 289000},
    {"category": "apartment", "city": "Минск", "address": "Минск, проспект Дзержинского, 24", "title": "Квартира у Грушевки", "published_date": "2026-03-09T09:00:00Z", "price_amount_byn": 271500},
    {"category": "apartment", "city": "Брест", "address": "Брест, улица Московская, 273В", "title": "Квартира в современном квартале Бреста", "published_date": "2026-02-11T09:00:00Z", "price_amount_byn": 238000},
    {"category": "house", "city": "Минск", "address": "Минск, улица Тикоцкого, 34", "title": "Дом в зеленой части Минска", "published_date": "2026-01-16T09:05:00Z", "price_amount_byn": 512000},
    {"category": "house", "city": "Гродно", "address": "Гродно, улица Тавлая, 46", "title": "Семейный дом в Гродно", "published_date": "2025-12-13T09:05:00Z", "price_amount_byn": 436000},
    {"category": "house", "city": "Брест", "address": "Брест, улица Янки Купалы, 112", "title": "Коттедж с участком в Бресте", "published_date": "2025-11-15T09:05:00Z", "price_amount_byn": 421000},
    {"category": "office", "city": "Минск", "address": "Минск, проспект Победителей, 7А", "title": "Офис в бизнес-центре на Победителей", "published_date": "2025-10-18T09:10:00Z", "price_amount_byn": 13400},
    {"category": "office", "city": "Гомель", "address": "Гомель, улица Советская, 97", "title": "Офис в деловом центре Гомеля", "published_date": "2025-09-12T09:10:00Z", "price_amount_byn": 9800},
    {"category": "warehouse", "city": "Минск", "address": "Минск, улица Бабушкина, 64", "title": "Складской комплекс в Минске", "published_date": "2025-08-14T09:15:00Z", "price_amount_byn": 1490000},
    {"category": "warehouse", "city": "Могилев", "address": "Могилев, Гомельское шоссе, 55", "title": "Складской блок в Могилеве", "published_date": "2025-07-17T09:15:00Z", "price_amount_byn": 1180000},
    {"category": "land", "city": "Минск", "address": "Минск, улица Тимирязева, 74А", "title": "Участок под коммерческий проект в Минске", "published_date": "2025-06-19T09:20:00Z", "price_amount_byn": 214000},
    {"category": "land", "city": "Брест", "address": "Брест, улица Лейтенанта Рябцева, 126", "title": "Коммерческий участок в Бресте", "published_date": "2025-05-21T09:20:00Z", "price_amount_byn": 176000},
]

MULTICITY_DEFINITIONS = [
    {"category": "apartment", "city": "Минск", "address": "Минск, улица Мстиславца, 8", "title": "2-комнатная квартира у Востока", "published_date": "2026-04-02T09:00:00Z", "price_amount_byn": 289000, "photo_count": 3},
    {"category": "apartment", "city": "Минск", "address": "Минск, улица Притыцкого, 39", "title": "Квартира рядом с метро Спортивная", "published_date": "2026-03-19T09:00:00Z", "price_amount_byn": 266000, "photo_count": 5},
    {"category": "apartment", "city": "Брест", "address": "Брест, улица Московская, 267", "title": "Квартира в новом квартале Бреста", "published_date": "2026-02-27T09:00:00Z", "price_amount_byn": 231000, "photo_count": 2},
    {"category": "apartment", "city": "Гродно", "address": "Гродно, улица Ожешко, 44", "title": "Квартира возле центра Гродно", "published_date": "2026-01-30T09:00:00Z", "price_amount_byn": 224000, "photo_count": 4},

    {"category": "house", "city": "Минск", "address": "Минск, улица Тикоцкого, 34", "title": "Семейный дом в зеленой части Минска", "published_date": "2026-01-16T09:05:00Z", "price_amount_byn": 512000, "photo_count": 4},
    {"category": "house", "city": "Минск", "address": "Минск, переулок Чижевских, 11", "title": "Коттедж с террасой в Минске", "published_date": "2025-12-28T09:05:00Z", "price_amount_byn": 548000, "photo_count": 3},
    {"category": "house", "city": "Брест", "address": "Брест, улица Янки Купалы, 112", "title": "Дом с участком в Бресте", "published_date": "2025-12-02T09:05:00Z", "price_amount_byn": 421000, "photo_count": 5},
    {"category": "house", "city": "Гомель", "address": "Гомель, улица Барыкина, 88", "title": "Просторный дом в Гомеле", "published_date": "2025-11-14T09:05:00Z", "price_amount_byn": 398000, "photo_count": 2},

    {"category": "office", "city": "Минск", "address": "Минск, проспект Победителей, 7А", "title": "Офис в бизнес-центре на Победителей", "published_date": "2025-10-18T09:10:00Z", "price_amount_byn": 13400, "photo_count": 3},
    {"category": "office", "city": "Минск", "address": "Минск, улица Сурганова, 57Б", "title": "Офис рядом с Академией наук", "published_date": "2025-09-30T09:10:00Z", "price_amount_byn": 12100, "photo_count": 4},
    {"category": "office", "city": "Гомель", "address": "Гомель, улица Советская, 97", "title": "Офис в деловом центре Гомеля", "published_date": "2025-09-12T09:10:00Z", "price_amount_byn": 9800, "photo_count": 2},
    {"category": "office", "city": "Могилев", "address": "Могилев, улица Ленинская, 29", "title": "Офис с переговорной в Могилеве", "published_date": "2025-08-22T09:10:00Z", "price_amount_byn": 9100, "photo_count": 5},

    {"category": "warehouse", "city": "Минск", "address": "Минск, улица Бабушкина, 64", "title": "Складской комплекс в Минске", "published_date": "2025-08-14T09:15:00Z", "price_amount_byn": 1490000, "photo_count": 4},
    {"category": "warehouse", "city": "Минск", "address": "Минск, улица Инженерная, 12", "title": "Склад с рампой в промышленной зоне Минска", "published_date": "2025-07-25T09:15:00Z", "price_amount_byn": 1325000, "photo_count": 3},
    {"category": "warehouse", "city": "Могилев", "address": "Могилев, Гомельское шоссе, 55", "title": "Складской блок в Могилеве", "published_date": "2025-07-17T09:15:00Z", "price_amount_byn": 1180000, "photo_count": 5},
    {"category": "warehouse", "city": "Витебск", "address": "Витебск, улица Гагарина, 222", "title": "Склад под логистику в Витебске", "published_date": "2025-06-29T09:15:00Z", "price_amount_byn": 1090000, "photo_count": 2},

    {"category": "land", "city": "Минск", "address": "Минск, улица Тимирязева, 74А", "title": "Участок под коммерческий проект в Минске", "published_date": "2025-06-19T09:20:00Z", "price_amount_byn": 214000, "photo_count": 2},
    {"category": "land", "city": "Минск", "address": "Минск, улица Монтажников, 31", "title": "Участок в деловой части Минска", "published_date": "2025-06-01T09:20:00Z", "price_amount_byn": 198000, "photo_count": 4},
    {"category": "land", "city": "Брест", "address": "Брест, улица Лейтенанта Рябцева, 126", "title": "Коммерческий участок в Бресте", "published_date": "2025-05-21T09:20:00Z", "price_amount_byn": 176000, "photo_count": 3},
    {"category": "land", "city": "Гродно", "address": "Гродно, улица Победы, 21", "title": "Участок под проект в Гродно", "published_date": "2025-05-03T09:20:00Z", "price_amount_byn": 169000, "photo_count": 5},
]

DIPLOMA_LOCAL_DEFINITIONS = [
    {"category": "apartment", "city": "Минск", "address": "Минск, улица Мстиславца, 8", "title": "2-комнатная квартира у Востока", "published_date": "2026-04-02T09:00:00Z", "price_amount_byn": 289000, "photo_count": 3},
    {"category": "apartment", "city": "Минск", "address": "Минск, улица Притыцкого, 39", "title": "Квартира рядом с метро Спортивная", "published_date": "2026-03-19T09:00:00Z", "price_amount_byn": 266000, "photo_count": 5},
    {"category": "apartment", "city": "Брест", "address": "Брест, улица Московская, 267", "title": "Квартира в новом квартале Бреста", "published_date": "2026-02-27T09:00:00Z", "price_amount_byn": 231000, "photo_count": 2},
    {"category": "apartment", "city": "Гродно", "address": "Гродно, улица Ожешко, 44", "title": "Квартира возле центра Гродно", "published_date": "2026-01-30T09:00:00Z", "price_amount_byn": 224000, "photo_count": 4},

    {"category": "house", "city": "Минск", "address": "Минск, улица Тикоцкого, 34", "title": "Семейный дом в зеленой части Минска", "published_date": "2026-01-16T09:05:00Z", "price_amount_byn": 512000, "photo_count": 4},
    {"category": "house", "city": "Брест", "address": "Брест, улица Янки Купалы, 112", "title": "Дом с участком в Бресте", "published_date": "2025-12-02T09:05:00Z", "price_amount_byn": 421000, "photo_count": 5},
    {"category": "house", "city": "Гомель", "address": "Гомель, улица Барыкина, 88", "title": "Просторный дом в Гомеле", "published_date": "2025-11-14T09:05:00Z", "price_amount_byn": 398000, "photo_count": 2},

    {"category": "office", "city": "Минск", "address": "Минск, проспект Победителей, 7А", "title": "Офис в бизнес-центре на Победителей", "published_date": "2025-10-18T09:10:00Z", "price_amount_byn": 13400, "photo_count": 3},
    {"category": "office", "city": "Минск", "address": "Минск, улица Сурганова, 57Б", "title": "Офис рядом с Академией наук", "published_date": "2025-09-30T09:10:00Z", "price_amount_byn": 12100, "photo_count": 4},
    {"category": "office", "city": "Гомель", "address": "Гомель, улица Советская, 97", "title": "Офис в деловом центре Гомеля", "published_date": "2025-09-12T09:10:00Z", "price_amount_byn": 9800, "photo_count": 2},
    {"category": "office", "city": "Могилев", "address": "Могилев, улица Ленинская, 29", "title": "Офис с переговорной в Могилеве", "published_date": "2025-08-22T09:10:00Z", "price_amount_byn": 9100, "photo_count": 5},

    {"category": "warehouse", "city": "Минск", "address": "Минск, улица Бабушкина, 64", "title": "Складской комплекс в Минске", "published_date": "2025-08-14T09:15:00Z", "price_amount_byn": 1490000, "photo_count": 4},
    {"category": "warehouse", "city": "Витебск", "address": "Витебск, улица Гагарина, 222", "title": "Склад под логистику в Витебске", "published_date": "2025-06-29T09:15:00Z", "price_amount_byn": 1090000, "photo_count": 2},
    {"category": "warehouse", "city": "Могилев", "address": "Могилев, Гомельское шоссе, 55", "title": "Складской блок в Могилеве", "published_date": "2025-07-17T09:15:00Z", "price_amount_byn": 1180000, "photo_count": 5},

    {"category": "land", "city": "Минск", "address": "Минск, улица Тимирязева, 74А", "title": "Участок под коммерческий проект в Минске", "published_date": "2025-06-19T09:20:00Z", "price_amount_byn": 214000, "photo_count": 2},
    {"category": "land", "city": "Минск", "address": "Минск, улица Монтажников, 31", "title": "Участок в деловой части Минска", "published_date": "2025-06-01T09:20:00Z", "price_amount_byn": 198000, "photo_count": 4},
    {"category": "land", "city": "Брест", "address": "Брест, улица Лейтенанта Рябцева, 126", "title": "Коммерческий участок в Бресте", "published_date": "2025-05-21T09:20:00Z", "price_amount_byn": 176000, "photo_count": 3},
]


def selected_definitions() -> list[dict]:
    if SEED_PROFILE == "diploma_local":
        return DIPLOMA_LOCAL_DEFINITIONS
    if SEED_PROFILE == "multicity":
        return MULTICITY_DEFINITIONS
    return SHOWCASE_DEFINITIONS


def build_payload(
    category: str,
    city: str,
    address: str,
    title: str,
    description: str,
    media_urls: list[str],
    price_amount_byn: int,
    published_date: str,
) -> dict:
    builders = {
        "apartment": apartment_payload,
        "house": house_payload,
        "office": office_payload,
        "warehouse": warehouse_payload,
        "land": land_payload,
    }
    builder = builders[category]
    if category == "apartment":
        return builder(city, address, title, description, media_urls, price_amount_byn, published_date)
    return builder(address, title, description, media_urls, price_amount_byn, published_date)


def parse_iso_utc(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00")).astimezone(timezone.utc)


def format_iso_utc(value: datetime) -> str:
    return value.astimezone(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def build_price_history_plan(published_date: str, current_price_byn: int) -> list[dict]:
    published_dt = parse_iso_utc(published_date)
    now = datetime.now(timezone.utc)
    latest_allowed = now - timedelta(days=7)
    candidate_dates = [
        published_dt + timedelta(days=12),
        published_dt + timedelta(days=48),
        published_dt + timedelta(days=96),
    ]
    fallback_offsets = [28, 18, 8]
    normalized_dates: list[datetime] = []

    for index, candidate in enumerate(candidate_dates):
        capped = min(candidate, latest_allowed - timedelta(days=fallback_offsets[index]))
        if normalized_dates:
            minimum_allowed = normalized_dates[-1] + timedelta(days=1)
            capped = max(capped, minimum_allowed)
        normalized_dates.append(capped)

    return [
        {
            "effectiveDateUtc": format_iso_utc(normalized_dates[0]),
            "priceAmountByn": int(round(current_price_byn * 0.87)),
            "comment": "Стартовая цена",
        },
        {
            "effectiveDateUtc": format_iso_utc(normalized_dates[1]),
            "priceAmountByn": int(round(current_price_byn * 0.92)),
            "comment": "Коррекция после первых просмотров",
        },
        {
            "effectiveDateUtc": format_iso_utc(normalized_dates[2]),
            "priceAmountByn": int(round(current_price_byn * 0.97)),
            "comment": "Актуализация перед сезоном",
        },
    ]


def append_price_history(token: str, listing_id: str, published_date: str, current_price_byn: int) -> list[dict]:
    results = []
    for payload in build_price_history_plan(published_date, current_price_byn):
        status, response = safe_api_request(
            "POST",
            f"/api/listings/{listing_id}/price-history",
            token=token,
            body=payload,
        )
        results.append(
            {
                "httpStatus": status,
                "request": payload,
                "response": try_parse_json(response),
            }
        )
    return results


def create_showcase_listings(token: str) -> None:
    created = []
    definitions = selected_definitions()
    for index, definition in enumerate(definitions, start=1):
        category = definition["category"]
        city = definition["city"]
        address = definition["address"]
        short_title = definition["title"]
        published_date = definition["published_date"]
        price_amount_byn = definition["price_amount_byn"]
        photo_count = int(definition.get("photo_count", 4))
        media_urls = pick_images(category, city, index, photo_count)
        title = short_title
        description = f"{MARKER} {city}. Фото DIploma."
        payload = build_payload(
            category,
            city,
            address,
            title,
            description,
            media_urls,
            price_amount_byn,
            published_date,
        )
        status, response = safe_api_request("POST", "/api/listings/", token=token, body=payload)
        parsed_response = try_parse_json(response)
        listing_id = parsed_response.get("data") if isinstance(parsed_response, dict) else None
        history_results = (
            append_price_history(token, listing_id, published_date, price_amount_byn)
            if status == 200 and listing_id
            else []
        )
        created.append(
            {
                "category": category,
                "city": city,
                "title": title,
                "publishedDate": published_date,
                "priceAmountByn": price_amount_byn,
                "httpStatus": status,
                "photoCount": photo_count,
                "mediaUrls": media_urls,
                "response": parsed_response,
                "priceHistory": history_results,
            }
        )
        log_output(
            {
                "step": f"{category}_{city}_{index}",
                "publishedDate": published_date,
                "priceAmountByn": price_amount_byn,
                "httpStatus": status,
                "photoCount": photo_count,
                "mediaUrls": media_urls,
                "response": parsed_response,
                "priceHistory": history_results,
            }
        )

    log_output({"step": "created_summary", "items": created})


def search_check() -> None:
    status, payload = safe_api_request("POST", "/api/search/listings", body={"page": 0, "pageSize": 50})
    log_output({"step": "search_check", "httpStatus": status, "response": try_parse_json(payload)})


def main() -> int:
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    if OUTPUT_FILE.exists():
        OUTPUT_FILE.unlink()

    ensure_port_forward()
    ensure_photo_server()
    token = sign_in()
    cleanup_old_listings(token)
    create_showcase_listings(token)
    search_check()
    print(f"Saved results to {OUTPUT_FILE}")
    print(f"Photo server: http://diploma.com:{PHOTO_SERVER_PORT}/")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(str(exc), file=sys.stderr)
        raise
