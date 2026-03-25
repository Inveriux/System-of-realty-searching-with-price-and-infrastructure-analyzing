#!/usr/bin/env bash
set -euo pipefail

API_BASE="${API_BASE:-http://diploma.com:5102}"
OUTPUT_FILE="${OUTPUT_FILE:-/home/Admin/Projects/Diploma/tmp/minsk_seed_results.jsonl}"
HOUSES_DIR="${HOUSES_DIR:-/home/Admin/Houses}"
MAX_IMAGES_PER_LISTING="${MAX_IMAGES_PER_LISTING:-10}"

mkdir -p "$(dirname "$OUTPUT_FILE")"

signin_response="$(curl -sS -X POST "${API_BASE}/api/users/signin" \
  -H 'Content-Type: application/json' \
  -d '{"email":"roman@gmail.com","password":"12345678"}')"

token="$(printf '%s' "$signin_response" | python3 -c 'import json,sys; print(json.load(sys.stdin)["data"]["token"])')"

printf '{"step":"signin","response":%s}\n' "$signin_response" > "$OUTPUT_FILE"

COMMON_IMAGE_CURSOR=0

image_exists() {
  local path="$1"
  [[ -n "$path" && -f "$path" ]]
}

collect_images_in_dir() {
  local dir="$1"

  if [[ ! -d "$dir" ]]; then
    return 0
  fi

  find "$dir" -type f \
    \( -iname '*.jpg' -o -iname '*.jpeg' -o -iname '*.png' -o -iname '*.webp' -o -iname '*.gif' \) \
    | sort \
    | head -n "$MAX_IMAGES_PER_LISTING"
}

collect_images_by_keywords() {
  local -n keywords_ref=$1

  if [[ ! -d "$HOUSES_DIR" ]]; then
    return 0
  fi

  python3 - "$HOUSES_DIR" "$MAX_IMAGES_PER_LISTING" "${keywords_ref[@]}" <<'PY'
import os
import re
import sys

root = sys.argv[1]
limit = int(sys.argv[2])
keywords = [arg.lower() for arg in sys.argv[3:] if arg]
allowed_exts = {".jpg", ".jpeg", ".png", ".webp", ".gif"}
matches = []

for current_root, dirnames, filenames in os.walk(root):
    dirnames.sort()
    filenames.sort()
    for filename in filenames:
        ext = os.path.splitext(filename)[1].lower()
        if ext not in allowed_exts:
            continue

        path = os.path.join(current_root, filename)
        normalized = path.lower()
        stem = os.path.splitext(filename)[0].lower()
        tokens = [token for token in re.split(r"[^0-9a-zа-яё]+", stem) if token]
        if any(keyword == token or keyword == stem for keyword in keywords for token in tokens):
            matches.append(path)

for path in matches[:limit]:
    print(path)
PY
}

collect_common_images() {
  if [[ ! -d "$HOUSES_DIR" ]]; then
    return 0
  fi

  mapfile -t all_images < <(
    find "$HOUSES_DIR" -type f \
      \( -iname '*.jpg' -o -iname '*.jpeg' -o -iname '*.png' -o -iname '*.webp' -o -iname '*.gif' \) \
      | sort
  )

  if (( ${#all_images[@]} == 0 )); then
    return 0
  fi

  local emitted=0
  while (( COMMON_IMAGE_CURSOR < ${#all_images[@]} && emitted < MAX_IMAGES_PER_LISTING )); do
    printf '%s\n' "${all_images[$COMMON_IMAGE_CURSOR]}"
    COMMON_IMAGE_CURSOR=$((COMMON_IMAGE_CURSOR + 1))
    emitted=$((emitted + 1))
  done
}

get_listing_images() {
  local listing_key="$1"
  local -a keywords
  local -a images=()

  case "$listing_key" in
    apartment)
      keywords=(apartment flat kvartira квартира)
      ;;
    house)
      keywords=(house home cottage dom дом)
      ;;
    office)
      keywords=(office офис)
      ;;
    warehouse)
      keywords=(warehouse sklad склад)
      ;;
    land)
      keywords=(land plot земля участок)
      ;;
    *)
      keywords=("$listing_key")
      ;;
  esac

  mapfile -t images < <(collect_images_in_dir "${HOUSES_DIR}/${listing_key}")
  if (( ${#images[@]} == 0 )); then
    mapfile -t images < <(collect_images_by_keywords keywords)
  fi
  if (( ${#images[@]} == 0 )); then
    mapfile -t images < <(collect_common_images)
  fi

  if (( ${#images[@]} > 0 )); then
    local image
    for image in "${images[@]}"; do
      if image_exists "$image"; then
        printf '%s\n' "$image"
      fi
    done
  fi
}

serialize_lines_to_json_array() {
  python3 -c 'import json,sys; print(json.dumps([line.strip() for line in sys.stdin if line.strip()], ensure_ascii=False))'
}

strip_media_urls() {
  python3 -c 'import json,sys; data=json.load(sys.stdin); dto=data.get("listingDto", data); dto.pop("mediaUrls", None); print(json.dumps(dto, ensure_ascii=False))'
}

create_listing_with_images() {
  local name="$1"
  local payload="$2"
  shift 2

  local response
  local -a image_paths=("$@")
  local -a curl_args=(
    -sS
    -X POST "${API_BASE}/api/listings/with-images"
    -H "Authorization: Bearer ${token}"
    -F "listing=${payload}"
  )

  local image
  for image in "${image_paths[@]}"; do
    if image_exists "$image"; then
      curl_args+=(-F "images=@${image}")
    fi
  done

  response="$(curl "${curl_args[@]}")"

  printf '{"step":"%s_with_images","images":%s,"response":%s}\n' \
    "$name" \
    "$(printf '%s\n' "${image_paths[@]}" | serialize_lines_to_json_array)" \
    "$response" >> "$OUTPUT_FILE"
}

create_listing() {
  local name="$1"
  local payload="$2"
  local response

  response="$(curl -sS -X POST "${API_BASE}/api/listings/" \
    -H "Authorization: Bearer ${token}" \
    -H 'Content-Type: application/json' \
    -d "$payload")"

  printf '{"step":"%s","response":%s}\n' "$name" "$response" >> "$OUTPUT_FILE"
}

seed_listing() {
  local name="$1"
  local payload="$2"
  local image_key="${3:-$name}"
  local payload_without_media
  local -a images=()

  if [[ -d "$HOUSES_DIR" ]]; then
    mapfile -t images < <(get_listing_images "$image_key" | awk '!seen[$0]++')
  fi

  if (( ${#images[@]} > 0 )); then
    payload_without_media="$(printf '%s' "$payload" | strip_media_urls)"
    create_listing_with_images "$name" "$payload_without_media" "${images[@]}"
  else
    create_listing "$name" "$payload"
  fi
}

seed_listing "apartment" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Петра Мстиславца, 18",
    "title": "2-комнатная квартира у метро Восток",
    "description": "Светлая квартира в Минске рядом с Dana Mall.",
    "priceAmountByn": 285000,
    "priceCurrency": "BYN",
    "pricePeriod": "OneTime",
    "areaTotal": 64.2,
    "buildYear": 2019,
    "publishedDate": "2026-03-22T06:00:00Z",
    "addressId": "10000000-0000-0000-0000-000000000001",
    "buildingType": "Apartment Building (Multi-family)",
    "listingType": "Sale",
    "status": "Active",
    "listingDetailsType": "Apartment",
    "mediaUrls": [
      "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80"
    ],
    "listingDetails": {
      "rooms": 2,
      "bedrooms": 1,
      "floorsTotal": 19,
      "levels": 1,
      "floor": 8,
      "areaLiving": 34.8,
      "areaKitchen": 11.1,
      "hasBalcony": true,
      "hasLoggia": false,
      "hasElevator": true,
      "ceilingHeight": 2.72,
      "bathroomType": "Combined (shared)",
      "renovationType": "Finished / Turnkey",
      "buildingMaterial": "Monolith"
    }
  }
}'

seed_listing "house" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Тикоцкого, 34",
    "title": "Дом в зеленой части Минска",
    "description": "Отдельный дом с участком и гаражом в черте Минска.",
    "priceAmountByn": 590000,
    "priceCurrency": "BYN",
    "pricePeriod": "OneTime",
    "areaTotal": 172.0,
    "buildYear": 2014,
    "publishedDate": "2026-03-22T06:05:00Z",
    "addressId": "10000000-0000-0000-0000-000000000002",
    "buildingType": "Detached House",
    "listingType": "Sale",
    "status": "Active",
    "listingDetailsType": "House",
    "mediaUrls": [
      "https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=80"
    ],
    "listingDetails": {
      "rooms": 5,
      "bedrooms": 3,
      "floors": 2,
      "houseArea": 172.0,
      "plotArea": 7.4,
      "hasElectricity": true,
      "hasGarage": true,
      "hasBasement": false,
      "hasFence": true,
      "wallMaterial": "Brick",
      "heatingType": "Individual gas boiler",
      "sewerType": "Septic tank",
      "gasType": "Central gas",
      "waterSupplyType": "Well"
    }
  }
}'

seed_listing "office" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, проспект Победителей, 7А",
    "title": "Офис в бизнес-центре на Победителей",
    "description": "Аренда офиса с open space планировкой в деловом центре Минска.",
    "priceAmountByn": 9800,
    "priceCurrency": "BYN",
    "pricePeriod": "Monthly",
    "areaTotal": 148.0,
    "buildYear": 2020,
    "publishedDate": "2026-03-22T06:10:00Z",
    "addressId": "10000000-0000-0000-0000-000000000003",
    "buildingType": "Business Center",
    "listingType": "Rent",
    "status": "Active",
    "listingDetailsType": "Office",
    "mediaUrls": [
      "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=1200&q=80"
    ],
    "listingDetails": {
      "plotArea": 148.0,
      "floor": 6,
      "floorsTotal": 12,
      "levels": 1,
      "parkingSpaces": 3,
      "kitchenette": true,
      "officeClass": "A",
      "layoutType": "Open space",
      "renovationType": "Finished / Turnkey",
      "parkingType": "Open space"
    }
  }
}'

seed_listing "warehouse" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Бабушкина, 64",
    "title": "Складской комплекс в промышленной зоне Минска",
    "description": "Продажа склада с рампами и офисным блоком.",
    "priceAmountByn": 1260000,
    "priceCurrency": "BYN",
    "pricePeriod": "OneTime",
    "areaTotal": 2400.0,
    "buildYear": 2017,
    "publishedDate": "2026-03-22T06:15:00Z",
    "addressId": "10000000-0000-0000-0000-000000000004",
    "buildingType": "Warehouse Complex",
    "listingType": "Sale",
    "status": "Active",
    "listingDetailsType": "Warehouse",
    "mediaUrls": [
      "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=1200&q=80"
    ],
    "listingDetails": {
      "plotArea": 4100.0,
      "ceilingHeight": 11.5,
      "floorLoadTonsPerSquareMeter": 6.2,
      "gateCount": 5,
      "hasDockLevelers": true,
      "dockCount": 3,
      "hasRamps": true,
      "powerKw": 280.0,
      "hasOffice": true,
      "officeArea": 160.0,
      "classType": "A",
      "heatingType": "Central"
    }
  }
}'

seed_listing "land" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Тимирязева, 74А",
    "title": "Участок под коммерческий проект в Минске",
    "description": "Участок в Минске с хорошим выездом и коммуникациями рядом.",
    "priceAmountByn": 215000,
    "priceCurrency": "BYN",
    "pricePeriod": "OneTime",
    "areaTotal": 12.6,
    "publishedDate": "2026-03-22T06:20:00Z",
    "addressId": "10000000-0000-0000-0000-000000000005",
    "buildingType": "Other",
    "listingType": "Sale",
    "status": "Active",
    "listingDetailsType": "Land",
    "mediaUrls": [
      "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=1200&q=80"
    ],
    "listingDetails": {
      "plotArea": 12.6,
      "allowedUse": "Mixed-use development",
      "hasElectricity": true,
      "hasGas": true,
      "hasWater": true,
      "hasSewer": true,
      "hasBuildings": false,
      "landCategory": "Industrial / commercial",
      "roadAccessType": "Asphalt"
    }
  }
}'

seed_listing "apartment_loft" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Белинского, 23",
    "title": "3-комнатная квартира рядом с парком Челюскинцев",
    "description": "Просторная квартира с отдельной кухней и удобным выездом в центр.",
    "priceAmountByn": 312000,
    "priceCurrency": "BYN",
    "pricePeriod": "OneTime",
    "areaTotal": 78.4,
    "buildYear": 2016,
    "publishedDate": "2026-03-22T06:25:00Z",
    "addressId": "10000000-0000-0000-0000-000000000006",
    "buildingType": "Apartment Building (Multi-family)",
    "listingType": "Sale",
    "status": "Active",
    "listingDetailsType": "Apartment",
    "mediaUrls": [],
    "listingDetails": {
      "rooms": 3,
      "bedrooms": 2,
      "floorsTotal": 14,
      "levels": 1,
      "floor": 6,
      "areaLiving": 46.2,
      "areaKitchen": 12.4,
      "hasBalcony": true,
      "hasLoggia": true,
      "hasElevator": true,
      "ceilingHeight": 2.7,
      "bathroomType": "Separate",
      "renovationType": "Good condition",
      "buildingMaterial": "Panel"
    }
  }
}' "apartment"

seed_listing "house_family" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Собинова, 111",
    "title": "Семейный дом с террасой в тихом районе",
    "description": "Дом с ухоженным участком, навесом для авто и жилой мансардой.",
    "priceAmountByn": 648000,
    "priceCurrency": "BYN",
    "pricePeriod": "OneTime",
    "areaTotal": 186.0,
    "buildYear": 2018,
    "publishedDate": "2026-03-22T06:30:00Z",
    "addressId": "10000000-0000-0000-0000-000000000007",
    "buildingType": "Detached House",
    "listingType": "Sale",
    "status": "Active",
    "listingDetailsType": "House",
    "mediaUrls": [],
    "listingDetails": {
      "rooms": 6,
      "bedrooms": 4,
      "floors": 2,
      "houseArea": 186.0,
      "plotArea": 8.1,
      "hasElectricity": true,
      "hasGarage": true,
      "hasBasement": true,
      "hasFence": true,
      "wallMaterial": "Block",
      "heatingType": "Individual gas boiler",
      "sewerType": "Central",
      "gasType": "Central gas",
      "waterSupplyType": "Central water supply"
    }
  }
}' "house"

seed_listing "warehouse_logistics" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, переулок Промышленный, 12",
    "title": "Логистический склад с рампой и офисным блоком",
    "description": "Склад для хранения и логистики с удобным подъездом для грузового транспорта.",
    "priceAmountByn": 15800,
    "priceCurrency": "BYN",
    "pricePeriod": "Monthly",
    "areaTotal": 1850.0,
    "buildYear": 2015,
    "publishedDate": "2026-03-22T06:35:00Z",
    "addressId": "10000000-0000-0000-0000-000000000008",
    "buildingType": "Warehouse Complex",
    "listingType": "Rent",
    "status": "Active",
    "listingDetailsType": "Warehouse",
    "mediaUrls": [],
    "listingDetails": {
      "plotArea": 2900.0,
      "ceilingHeight": 9.8,
      "floorLoadTonsPerSquareMeter": 5.5,
      "gateCount": 4,
      "hasDockLevelers": true,
      "dockCount": 2,
      "hasRamps": true,
      "powerKw": 180.0,
      "hasOffice": true,
      "officeArea": 95.0,
      "classType": "B+",
      "heatingType": "Air heating"
    }
  }
}' "warehouse"

seed_listing "office_showroom" '{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000000",
    "addressText": "Минск, улица Сурганова, 57Б",
    "title": "Офис с шоурумом возле метро Академия наук",
    "description": "Помещение под офис и клиентскую зону с готовой отделкой.",
    "priceAmountByn": 12400,
    "priceCurrency": "BYN",
    "pricePeriod": "Monthly",
    "areaTotal": 176.0,
    "buildYear": 2021,
    "publishedDate": "2026-03-22T06:40:00Z",
    "addressId": "10000000-0000-0000-0000-000000000009",
    "buildingType": "Business Center",
    "listingType": "Rent",
    "status": "Active",
    "listingDetailsType": "Office",
    "mediaUrls": [],
    "listingDetails": {
      "plotArea": 176.0,
      "floor": 2,
      "floorsTotal": 9,
      "levels": 1,
      "parkingSpaces": 5,
      "kitchenette": true,
      "officeClass": "B+",
      "layoutType": "Mixed",
      "renovationType": "Designer",
      "parkingType": "Cabinet / rooms"
    }
  }
}' "office"

search_response="$(curl -sS -X POST "${API_BASE}/api/search/listings" \
  -H 'Content-Type: application/json' \
  -d '{"page":0,"pageSize":20}')"

printf '{"step":"search_check","response":%s}\n' "$search_response" >> "$OUTPUT_FILE"

printf 'Saved results to %s\n' "$OUTPUT_FILE"
