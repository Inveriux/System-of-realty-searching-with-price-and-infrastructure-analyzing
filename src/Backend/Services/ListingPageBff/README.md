# ListingPageBff

## Purpose

`ListingPageBff` aggregates a production-like page DTO for the frontend by calling:

- `ListingService` as the critical source of truth
- `InfrastructureService` for location-based enrichment
- `AgentService` for public seller profile data
- `SearchService` for optional enrichment/fallback

Primary endpoint:

- `GET /api/listing-pages/{listingId}`

## Structure

```text
ListingPageBff/
  Application/
    ListingPages/
      GetListingPageQuery.cs
      ListingPageAssembler.cs
  Contracts/
    ListingPages/
      ListingPageDto.cs
  Infrastructure/
    Clients/
      Abstractions/
      Contracts/
      *.cs
    Options/
      *.cs
  Presentation/
    Endpoints/
      ListingPageEndpoints.cs
  Program.cs
  appsettings*.json
```

## Downstream endpoints expected by the BFF

Already available in the current solution:

- `ListingService`: `GET /api/listings/details/{listingId}`
- `InfrastructureService`: `GET /api/infrastructure/{locationId}/summary`
- `SearchService`: `GET /api/search/listings/{listingId}`

Recommended to add in `User/Agent Service`:

- `GET /api/agents/{agentId}/public-profile`

Recommended response for the public-profile endpoint:

```json
{
  "isSuccess": true,
  "data": {
    "id": "1d44f333-74e0-41d5-8bc7-2ab57f4ddcc1",
    "fullName": "Анна Соколова",
    "avatarUrl": "https://cdn.example.com/avatars/anna.jpg",
    "phone": "+375291234567",
    "email": "anna@example.com",
    "rating": 4.8,
    "reviewsCount": 37,
    "isVerified": true,
    "agencyName": "Urban Realty"
  }
}
```

## Graceful degradation model

- `ListingService` failure: request fails, because listing data is critical
- `InfrastructureService` failure: page still returns with `infrastructure = null`
- `AgentService` failure: fallback seller block is returned
- `SearchService` failure: only optional enrichment is skipped

Warnings are returned in:

- `warnings[]` as structured items
- `derived.warnings[]` as frontend-friendly messages

Availability flags are returned in:

- `availableBlocks`

## Example response

```json
{
  "isSuccess": true,
  "message": null,
  "statusCode": 0,
  "data": {
    "listing": {
      "id": "88ce3f23-94b5-4472-9066-5d745e2f380d",
      "agentId": "0f8184a5-6a6e-48c3-9b56-2d36d17b61f7",
      "locationId": "8b6dfd62-0679-4f17-8a29-9a4b6a0d63ad",
      "title": "3-комнатная квартира рядом с метро",
      "description": "Светлая квартира с хорошим ремонтом.",
      "addressText": "Минск, ул. Петра Мстиславца, 8",
      "latitude": 53.9324,
      "longitude": 27.6513,
      "priceAmount": 389000,
      "priceCurrency": "BYN",
      "priceAmountByn": 389000,
      "priceAmountUsd": 119000,
      "pricePeriod": "OneTime",
      "areaTotal": 87.4,
      "buildYear": 2018,
      "publishedDate": "2026-03-20T09:30:00+00:00",
      "buildingType": "Apartment Building (Multi-family)",
      "listingType": "Sale",
      "mainMedia": "https://cdn.example.com/listings/88ce3f23/main.jpg",
      "media": [
        {
          "url": "https://cdn.example.com/listings/88ce3f23/main.jpg"
        }
      ],
      "listingDetailsType": "Apartment",
      "listingDetails": {
        "rooms": 3,
        "floor": 12,
        "floorsTotal": 19,
        "hasBalcony": true,
        "bathroomType": "Separate"
      },
      "priceHistory": [
        {
          "changedAt": "2026-02-10T08:00:00+00:00",
          "priceAmount": 395000,
          "priceCurrency": "BYN",
          "priceAmountByn": 395000,
          "priceAmountUsd": 121000
        }
      ]
    },
    "infrastructure": {
      "score": 82.4,
      "totalScore": 82.4,
      "educationScore": 75.0,
      "healthcareScore": 71.3,
      "transportScore": 91.1,
      "commerceScore": 84.2,
      "leisureScore": 73.5,
      "poiCount": 46,
      "summary": "Ближайшие объекты: Green Market (180 м), Metro Station (250 м), School 24 (410 м).",
      "groupedPois": [
        {
          "category": "Transport",
          "count": 7,
          "pois": []
        }
      ],
      "nearbyPois": [],
      "highlights": [
        "Сильная инфраструктура рядом с объектом.",
        "Хорошая транспортная доступность."
      ],
      "calculatedAtUtc": "2026-03-21T06:40:00"
    },
    "seller": {
      "id": "0f8184a5-6a6e-48c3-9b56-2d36d17b61f7",
      "fullName": "Анна Соколова",
      "avatarUrl": "https://cdn.example.com/avatars/anna.jpg",
      "phone": "+375291234567",
      "email": "anna@example.com",
      "rating": 4.8,
      "reviewsCount": 37,
      "isVerified": true,
      "agencyName": "Urban Realty",
      "isFallback": false
    },
    "derived": {
      "quickStats": [
        {
          "key": "area",
          "label": "Площадь",
          "value": "87.4",
          "unit": "м²"
        }
      ],
      "featureTags": [
        {
          "key": "verified_seller",
          "label": "Проверенный продавец",
          "tone": "positive"
        }
      ],
      "warnings": [],
      "lastUpdatedUtc": "2026-03-21T09:15:00+00:00"
    },
    "availableBlocks": {
      "listing": true,
      "infrastructure": true,
      "seller": true,
      "priceHistory": true,
      "map": true
    },
    "warnings": []
  },
  "errors": []
}
```

## Caching recommendations

Good BFF cache candidates:

- Listing page aggregate by `listingId` for `30-120 sec`
- Seller public profile by `agentId` for `5-15 min`
- Infrastructure summary by `locationId` for `10-30 min`

Suggested strategy:

- small in-memory cache for single-instance local development
- distributed cache for production-like deployment
- cache seller and infrastructure independently to avoid recomputing the full page

Do not aggressively cache:

- draft/private listing data
- highly dynamic seller contact preferences
- hot price changes without invalidation hooks

## Future extensions

- add recommendation widgets from `SearchService`
- add mortgage calculator block
- add contact CTA experiments in the BFF
- add feature flags to suppress expensive blocks
- add ETag / `If-None-Match` for page payloads
- add circuit-breaker and retry policies through Polly

## What to load on-demand from the frontend

Keep inside the initial BFF payload:

- listing core data
- seller block
- infrastructure summary and highlights
- hero media and quick stats

Load on-demand:

- long related listings carousels
- large seller reviews pages
- detailed POI lists beyond the nearest subset
- analytics/heavy recommendation widgets

## Next.js integration

Use one server-side fetch from the page route:

```ts
const response = await fetch(`${process.env.BFF_URL}/api/listing-pages/${listingId}`, {
  next: { revalidate: 60 },
});
```

Recommended frontend flow:

1. fetch BFF page DTO in the route segment
2. render listing hero, media, seller card, map and infrastructure from the single payload
3. show `warnings` only in debug/admin mode or when a block is degraded
