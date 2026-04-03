#!/usr/bin/env python3
from __future__ import annotations

import json
import re
import sys
import time
import urllib.request
from pathlib import Path


USER_AGENT = "DiplomaRealtyDataset/1.0"
DELAY_SECONDS = 2.0

CURATED_IMAGES = {
    "apartments": [
        {
            "title": "Apartment tower block exterior",
            "image_url": "https://c.pxhere.com/photos/eb/11/apartment_building_property_real_estate_condominium_structure_architecture_exterior-973420.jpg!d",
            "source_url": "https://pxhere.com/en/photo/973420",
            "address_hint": "apartment tower block exterior",
        },
        {
            "title": "Modern residential apartments",
            "image_url": "https://c.pxhere.com/photos/7f/0a/home_house_building_residential_architecture_property_construction_real-711468.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/7f/0a/home_house_building_residential_architecture_property_construction_real-711468.jpg!s1",
            "address_hint": "modern residential apartments",
        },
        {
            "title": "Condos residential modern urban estate",
            "image_url": "https://c.pxhere.com/photos/31/10/building_apartments_condos_real_estate_residential_modern_urban_estate-975119.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/31/10/building_apartments_condos_real_estate_residential_modern_urban_estate-975119.jpg!s1",
            "address_hint": "modern condos exterior",
        },
        {
            "title": "Apartment building modern architecture",
            "image_url": "https://c.pxhere.com/photos/8b/12/apartment_building_modern_architecture_residential_home_urban_estate_city-711459.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/8b/12/apartment_building_modern_architecture_residential_home_urban_estate_city-711459.jpg!s1",
            "address_hint": "apartment building exterior",
        },
        {
            "title": "Residential urban facade",
            "image_url": "https://c.pxhere.com/photos/27/fa/building_house_architecture_city_tel_aviv_design_residential_real-1253384.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/27/fa/building_house_architecture_city_tel_aviv_design_residential_real-1253384.jpg!s1",
            "address_hint": "residential urban facade",
        },
    ],
    "houses": [
        {
            "title": "House museum exterior",
            "image_url": "https://c.pxhere.com/photos/2e/41/house_museum_japanese_design_high_exterior_traditional_style-445951.jpg!d",
            "source_url": "https://pxhere.com/en/photo/445951",
            "address_hint": "private house exterior",
        },
        {
            "title": "Villa house exterior",
            "image_url": "https://c.pxhere.com/photos/73/7f/house_home_japanese_design_high_exterior_traditional_style-445363.jpg!d",
            "source_url": "https://pxhere.com/en/photo/445363",
            "address_hint": "villa exterior",
        },
        {
            "title": "Tudor house exterior",
            "image_url": "https://c.pxhere.com/photos/44/7c/house_tudor_home_architecture_uk_exterior_english_england-648027.jpg!d",
            "source_url": "https://pxhere.com/en/photo/648027",
            "address_hint": "tudor style house",
        },
        {
            "title": "Traditional house with pergola",
            "image_url": "https://c.pxhere.com/photos/86/f0/house_museum_japanese_design_high_exterior_traditional_style-578897.jpg!d",
            "source_url": "https://pxhere.com/en/photo/578897",
            "address_hint": "traditional house exterior",
        },
        {
            "title": "Villa cottage exterior",
            "image_url": "https://c.pxhere.com/photos/be/1f/house_home_japanese_design_high_exterior_traditional_style-367997.jpg!d",
            "source_url": "https://pxhere.com/en/photo/367997",
            "address_hint": "villa cottage exterior",
        },
        {
            "title": "Green front door house",
            "image_url": "https://c.pxhere.com/photos/9c/22/house_front_green_door_window_entrance_home_residential-725511.jpg!d",
            "source_url": "https://pxhere.com/en/photo/725511",
            "address_hint": "front door house facade",
        },
        {
            "title": "Brick house entrance",
            "image_url": "https://c.pxhere.com/photos/da/51/door_bricks_brick_house_old_house_home_architecture_exterior_entrance-1341403.jpg!d",
            "source_url": "https://pxhere.com/en/photo/1341403",
            "address_hint": "brick house exterior",
        },
        {
            "title": "Front house entrance",
            "image_url": "https://c.pxhere.com/photos/ec/3c/door_entrance_home_house_wood_opening_door_front_door_building-1056956.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/ec/3c/door_entrance_home_house_wood_opening_door_front_door_building-1056956.jpg!s1",
            "address_hint": "front entrance house",
        },
        {
            "title": "Double door house",
            "image_url": "https://c.pxhere.com/photos/be/fc/doors_double_entrance_home_house_architecture_design_estate-780214.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/be/fc/doors_double_entrance_home_house_architecture_design_estate-780214.jpg!s1",
            "address_hint": "double door house facade",
        },
        {
            "title": "Residential front door",
            "image_url": "https://c.pxhere.com/photos/23/da/front_door_building_architecture_front_house_entrance_home_residential-533910.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/23/da/front_door_building_architecture_front_house_entrance_home_residential-533910.jpg!s1",
            "address_hint": "residential front facade",
        },
    ],
    "offices": [
        {
            "title": "Corporate office tower",
            "image_url": "https://c.pxhere.com/photos/08/28/office_inside_business_modern_interior_corporate_indoor_building-747386.jpg!d",
            "source_url": "https://pxhere.com/en/photo/747386",
            "address_hint": "office tower facade",
        },
        {
            "title": "Office building exterior",
            "image_url": "https://c.pxhere.com/photos/b0/f5/building_exterior_office_building_office_windows_office_business_architecture_modern-570879.jpg!d",
            "source_url": "https://pxhere.com/en/photo/570879",
            "address_hint": "office building exterior",
        },
        {
            "title": "Urban office skyscraper",
            "image_url": "https://c.pxhere.com/photos/5f/e4/building_skyscraper_urban_modern_office_downtown_business_exterior-985973.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/5f/e4/building_skyscraper_urban_modern_office_downtown_business_exterior-985973.jpg!s1",
            "address_hint": "urban office skyscraper",
        },
        {
            "title": "Glass office building",
            "image_url": "https://c.pxhere.com/photos/6f/1b/office_building_business_architecture_modern_glass_exterior_structure-1069397.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/6f/1b/office_building_business_architecture_modern_glass_exterior_structure-1069397.jpg!s1",
            "address_hint": "glass office facade",
        },
        {
            "title": "Business center exterior",
            "image_url": "https://c.pxhere.com/photos/cc/b7/business_center_exterior_architecture_glass_modern_corporate_building-593375.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/cc/b7/business_center_exterior_architecture_glass_modern_corporate_building-593375.jpg!s1",
            "address_hint": "business center facade",
        },
    ],
    "commercial": [
        {
            "title": "Shopping mall interior",
            "image_url": "https://c.pxhere.com/images/84/e0/3b6debb43e4c4a2c9c7915ca9686-1654545.jpg!d",
            "source_url": "https://pxhere.com/en/photo/1654545",
            "address_hint": "shopping mall / retail",
        },
        {
            "title": "Retail commercial building",
            "image_url": "https://c.pxhere.com/photos/b0/f5/building_exterior_office_building_office_windows_office_business_architecture_modern-570879.jpg!d",
            "source_url": "https://pxhere.com/en/photo/570879",
            "address_hint": "commercial building exterior",
        },
        {
            "title": "Corporate facade commercial",
            "image_url": "https://c.pxhere.com/photos/8f/e1/building_outside_outdoor_architecture_city_business_outdoors_sky-1094025.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/8f/e1/building_outside_outdoor_architecture_city_business_outdoors_sky-1094025.jpg!s1",
            "address_hint": "commercial facade",
        },
        {
            "title": "Modern commercial tower",
            "image_url": "https://c.pxhere.com/photos/1c/29/building_minnesota_city_downtown_urban_architecture_business_modern-943622.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/1c/29/building_minnesota_city_downtown_urban_architecture_business_modern-943622.jpg!s1",
            "address_hint": "commercial tower",
        },
        {
            "title": "Retail business center",
            "image_url": "https://c.pxhere.com/photos/cc/b7/business_center_exterior_architecture_glass_modern_corporate_building-593375.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/cc/b7/business_center_exterior_architecture_glass_modern_corporate_building-593375.jpg!s1",
            "address_hint": "retail / business center",
        },
    ],
    "warehouses": [
        {
            "title": "Warehouse industrial facade",
            "image_url": "https://c.pxhere.com/photos/21/e7/building_construction_architecture_industry_structure_build_business_contractor-709522.jpg!d",
            "source_url": "https://pxhere.com/en/photo/709522",
            "address_hint": "warehouse industrial facade",
        },
        {
            "title": "Industrial warehouse building",
            "image_url": "https://c.pxhere.com/photos/2e/91/building_construction_architecture_industry_structure_build_business_contractor-709518.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/2e/91/building_construction_architecture_industry_structure_build_business_contractor-709518.jpg!s1",
            "address_hint": "warehouse / hangar exterior",
        },
        {
            "title": "Hangar warehouse exterior",
            "image_url": "https://c.pxhere.com/photos/66/a2/building_construction_architecture_industry_structure_build_business_contractor-870387.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/66/a2/building_construction_architecture_industry_structure_build_business_contractor-870387.jpg!s1",
            "address_hint": "hangar warehouse exterior",
        },
        {
            "title": "Abandoned factory warehouse",
            "image_url": "https://c.pxhere.com/photos/c0/00/abandoned_factory_empty_decay_ruin_building_abandoned_broken_factory-585202.jpg!d",
            "source_url": "https://pxhere.com/en/photo/585202",
            "address_hint": "industrial warehouse / factory",
        },
        {
            "title": "Monochrome warehouse ruins",
            "image_url": "https://c.pxhere.com/photos/99/97/building_construction_architecture_industry_structure_build_business_contractor-1070288.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/99/97/building_construction_architecture_industry_structure_build_business_contractor-1070288.jpg!s1",
            "address_hint": "warehouse / industrial building",
        },
    ],
    "land": [
        {
            "title": "Land lot with house",
            "image_url": "https://c.pxhere.com/photos/34/04/photo-107715.jpg!d",
            "source_url": "https://pxhere.com/en/photo/107715",
            "address_hint": "land lot / suburban plot",
        },
        {
            "title": "Pasture landscape",
            "image_url": "https://c.pxhere.com/photos/98/75/horses_grazing_pasture_rural_landscape_green_scenic_fences-801692.jpg!d",
            "source_url": "https://pxhere.com/en/photo/801692",
            "address_hint": "land / field landscape",
        },
        {
            "title": "New home for sale land lot",
            "image_url": "https://c.pxhere.com/photos/08/7b/new_home_for_sale_home_house_mortgage_sell_buy_real_estate-569321.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/08/7b/new_home_for_sale_home_house_mortgage_sell_buy_real_estate-569321.jpg!s1",
            "address_hint": "land lot / property for sale",
        },
        {
            "title": "Land lot nature reserve",
            "image_url": "https://c.pxhere.com/images/01/2d/d82edd6cb3365158140da8b39595-1594051.jpg!s1",
            "source_url": "https://c.pxhere.com/images/01/2d/d82edd6cb3365158140da8b39595-1594051.jpg!s1",
            "address_hint": "land lot greenery",
        },
        {
            "title": "Property yard landscape",
            "image_url": "https://c.pxhere.com/images/f0/66/f753751c0e9b7d1ab0655d6aa2b4-1450935.jpg!s1",
            "source_url": "https://c.pxhere.com/images/f0/66/f753751c0e9b7d1ab0655d6aa2b4-1450935.jpg!s1",
            "address_hint": "yard / land parcel",
        },
        {
            "title": "Horse pasture meadow",
            "image_url": "https://c.pxhere.com/photos/0d/63/horse_pasture_spring_meadow_horses_farm_pasture_animals_landscape_nature-634046.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/0d/63/horse_pasture_spring_meadow_horses_farm_pasture_animals_landscape_nature-634046.jpg!s1",
            "address_hint": "field / meadow",
        },
        {
            "title": "Horse grazing field",
            "image_url": "https://c.pxhere.com/photos/85/63/horse_grazing_field_pasture_grass_meadow_mammal_farm-992889.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/85/63/horse_grazing_field_pasture_grass_meadow_mammal_farm-992889.jpg!s1",
            "address_hint": "grazing field",
        },
        {
            "title": "Fields countryside horses",
            "image_url": "https://c.pxhere.com/photos/7d/fd/netherlands_landscape_scenic_horses_cattle_farm_rural_fields-1135571.jpg!s1",
            "source_url": "https://c.pxhere.com/photos/7d/fd/netherlands_landscape_scenic_horses_cattle_farm_rural_fields-1135571.jpg!s1",
            "address_hint": "rural fields",
        },
    ],
}


def slugify(value: str) -> str:
    value = value.lower()
    value = re.sub(r"[^a-z0-9]+", "-", value)
    return value.strip("-") or "image"


def download(url: str, destination: Path) -> None:
    req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(req, timeout=120) as response:
        destination.write_bytes(response.read())


def load_manifest(path: Path) -> list[dict]:
    if not path.exists():
        return []
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except Exception:  # noqa: BLE001
        return []


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: fetch_realty_dataset_curated.py <target_dir>", file=sys.stderr)
        return 1

    target_dir = Path(sys.argv[1]).expanduser().resolve()
    target_dir.mkdir(parents=True, exist_ok=True)
    metadata_dir = target_dir / "metadata"
    metadata_dir.mkdir(exist_ok=True)
    manifest_path = metadata_dir / "manifest.json"
    manifest = load_manifest(manifest_path)
    existing_urls = {entry.get("imageUrl") for entry in manifest}

    for category, entries in CURATED_IMAGES.items():
        category_dir = target_dir / category
        category_dir.mkdir(exist_ok=True)
        existing_names = {path.name for path in category_dir.iterdir() if path.is_file()}
        counter = len(existing_names)

        for entry in entries:
            if entry["image_url"] in existing_urls:
                continue

            counter += 1
            suffix = ".jpg"
            filename = f"{counter:02d}-{slugify(entry['title'])[:80]}{suffix}"
            destination = category_dir / filename
            try:
                download(entry["image_url"], destination)
            except Exception as exc:  # noqa: BLE001
                print(f"[warn] {category}: {entry['image_url']} -> {exc}", file=sys.stderr)
                counter -= 1
                continue

            manifest.append(
                {
                    "category": category,
                    "fileName": filename,
                    "query": "curated-pxhere",
                    "title": entry["title"],
                    "sourceUrl": entry["source_url"],
                    "imageUrl": entry["image_url"],
                    "license": "CC0 Public Domain",
                    "artist": "PxHere / source page author",
                    "description": entry["title"],
                    "addressHint": entry["address_hint"],
                }
            )
            existing_urls.add(entry["image_url"])
            manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
            print(f"{category}: saved {filename}")
            time.sleep(DELAY_SECONDS)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
