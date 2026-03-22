#!/usr/bin/env bash
set -euo pipefail

API_BASE="${API_BASE:-http://diploma.com:5102}"
OUTPUT_FILE="${OUTPUT_FILE:-/home/Admin/Projects/Diploma/tmp/minsk_seed_results.jsonl}"

mkdir -p "$(dirname "$OUTPUT_FILE")"

signin_response="$(curl -sS -X POST "${API_BASE}/api/users/signin" \
  -H 'Content-Type: application/json' \
  -d '{"email":"roman@gmail.com","password":"12345678"}')"

token="$(printf '%s' "$signin_response" | python3 -c 'import json,sys; print(json.load(sys.stdin)["data"]["token"])')"

printf '{"step":"signin","response":%s}\n' "$signin_response" > "$OUTPUT_FILE"

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

create_listing "apartment" '{
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

create_listing "house" '{
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

create_listing "office" '{
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
    "listingDetails": {}
  }
}'

create_listing "warehouse" '{
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

create_listing "land" '{
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

search_response="$(curl -sS -X POST "${API_BASE}/api/search/listings" \
  -H 'Content-Type: application/json' \
  -d '{"page":0,"pageSize":20}')"

printf '{"step":"search_check","response":%s}\n' "$search_response" >> "$OUTPUT_FILE"

printf 'Saved results to %s\n' "$OUTPUT_FILE"
