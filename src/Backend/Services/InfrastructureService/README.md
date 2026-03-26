# InfrastructureService

## Architecture

`InfrastructureService` now uses a hybrid approach:

- Yandex remains responsible for geocoding and map-related scenarios.
- A dedicated `IPlacesProvider` abstraction is responsible for nearby POI search.
- `OverpassPlacesProvider` is the active provider for infrastructure analytics.

High-level structure:

- `UseCases/Infrastructure`
  - orchestration for refresh and summary endpoints
- `Services/Places`
  - provider abstraction, provider selection, category normalization
- `Services/Sanitization`
  - address-like garbage filtering before classification and scoring
- `Services/Places/Overpass`
  - Overpass API client and provider implementation
- `Services/Scoring`
  - category and total score calculation
- `Services/Summaries`
  - human-readable infrastructure summary and highlights

## Why Overpass

`Overpass` was selected instead of Yandex Organizations Search because:

- it is based on OSM open data
- it works well for nearby categorical POI search
- it removes dependency on a paid organizations search API
- it fits the diploma MVP scenario well

This keeps Yandex Maps on the frontend and for geocoding, while infrastructure analytics use an independent POI data source.

## Provider Abstraction

Main interface:

- [IPlacesProvider.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Places/Abstractions/IPlacesProvider.cs)

Current implementation:

- [OverpassPlacesProvider.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Places/Overpass/OverpassPlacesProvider.cs)
- [OverpassApiClient.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Places/Overpass/OverpassApiClient.cs)

Future providers can be added without changing application handlers:

- `GeoapifyPlacesProvider`
- `YandexPlacesProvider`

## Category Normalization

Internal categories:

- `education`
- `healthcare`
- `transport`
- `commerce`
- `leisure`

Normalization rules are centralized in:

- [PlaceCategoryNormalizer.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Places/PlaceCategoryNormalizer.cs)

Examples:

- `amenity=school` -> `education`
- `amenity=pharmacy` -> `healthcare`
- `highway=bus_stop` -> `transport`
- `shop=supermarket` -> `commerce`
- `leisure=park` -> `leisure`

## Scoring

Scoring now uses grouped scoring entities and category policies:

- [InfrastructureScoringService.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/InfrastructureScoringService.cs)
- [InfrastructureScoringInputBuilder.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/InfrastructureScoringInputBuilder.cs)
- [PoiSanitizationService.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Sanitization/PoiSanitizationService.cs)
- [MetroApplicabilityService.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/MetroApplicabilityService.cs)
- [TransportCategoryScoringPolicy.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/Policies/TransportCategoryScoringPolicy.cs)
- [HealthcareCategoryScoringPolicy.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/Policies/HealthcareCategoryScoringPolicy.cs)
- [CommerceCategoryScoringPolicy.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/Policies/CommerceCategoryScoringPolicy.cs)
- [EducationCategoryScoringPolicy.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/Policies/EducationCategoryScoringPolicy.cs)
- [LeisureCategoryScoringPolicy.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/Policies/LeisureCategoryScoringPolicy.cs)

Category formula:

- `CategoryScore = Proximity + Density + Diversity + CompletenessBonus`
- each category remains in `0..100`
- grouped entities are used instead of raw POI counts
- per-subcategory useful count caps block score inflation
- single-subcategory and two-subcategory caps block false `90..100` scores
- `100` requires enough different useful subcategories plus completeness

Transport has stricter rules:

- stop platforms are grouped into stop groups before scoring
- railway/platform duplicates do not expand density linearly
- surface transport, metro, railway and parking are scored separately
- metro affects score only where it is applicable, starting with Minsk
- address-like garbage POI are filtered before summary and scoring
- without diversity, transport cannot reach excellent scores

## Summary

Human-readable summary builder:

- [InfrastructureSummaryBuilder.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Summaries/InfrastructureSummaryBuilder.cs)

The response now includes:

- `summary`
- `highlights`

while preserving existing score and POI data.

## Configuration

Example configuration:

```json
{
  "PlacesProvider": {
    "Provider": "Overpass"
  },
  "Overpass": {
    "BaseUrl": "https://overpass-api.de/api/",
    "TimeoutSeconds": 20,
    "QueryTimeoutSeconds": 25,
    "UserAgent": "DiplomaInfrastructureService/1.0"
  },
  "InfrastructureScoring": {
    "ActiveProfile": "residential",
    "DefaultRadiusMeters": 1500,
    "MaxRadiusMeters": 3000,
    "MaxPoiCount": 100,
    "CacheMinutes": 10,
    "ResidentialProfile": {
      "Transport": {
        "SingleSubcategoryCap": 45,
        "TwoSubcategoryCap": 70,
        "ThreeSubcategoryCap": 85
      },
      "Healthcare": {
        "SingleSubcategoryCap": 45
      },
      "Commerce": {
        "SingleSubcategoryCap": 45
      },
      "Education": {
        "SingleSubcategoryCap": 45
      },
      "Leisure": {
        "SingleSubcategoryCap": 45
      }
    }
  },
  "Yandex": {
    "GeocoderBaseUrl": "https://geocode-maps.yandex.ru/",
    "ApiKey": "...",
    "Language": "ru_RU"
  }
}
```

## Endpoints

Existing endpoints remain valid:

- `POST /api/infrastructure/refresh`
- `GET /api/infrastructure/{locationId}`
- `GET /api/infrastructure/{locationId}/pois`
- `GET /api/infrastructure/{locationId}/summary`

## Example Response

```json
{
  "isSuccess": true,
  "message": null,
  "statusCode": 0,
  "data": {
    "locationId": "50014a8c-bf1e-455d-be2c-30979a937268",
    "addressText": "Беларусь, Минск, улица Энгельса",
    "latitude": 53.900501,
    "longitude": 27.562433,
    "score": {
      "locationId": "50014a8c-bf1e-455d-be2c-30979a937268",
      "snapshotId": "175e0a15-4c03-494a-92a1-17c0173a2556",
      "totalScore": 74.8,
      "educationScore": 68.4,
      "healthcareScore": 72.1,
      "transportScore": 90.0,
      "commerceScore": 78.3,
      "leisureScore": 61.0,
      "poiCount": 42,
      "calculatedAtUtc": "2026-03-21T18:29:59Z",
      "searchFeatures": {
        "transport_count": 4,
        "transport_nearest_m": 95,
        "transport_score": 76.4,
        "transport_diversity_score": 26.25,
        "transport_density_score": 11.8,
        "transport_proximity_score": 31.4
      },
      "details": {
        "scoringProfile": "residential",
        "categoryScoreBreakdowns": {
          "transport": {
            "score": 76.4,
            "proximityScore": 31.4,
            "densityScore": 11.8,
            "diversityScore": 26.25,
            "completenessBonus": 7.0,
            "appliedCap": 85.0,
            "appliedCapReason": "three_subcategories_without_full_completeness",
            "presentSubcategories": [
              "bus_stop_group",
              "metro_station",
              "parking"
            ],
            "missingImportantSubcategories": [],
            "groupedCounts": {
              "bus_stop_group": 2,
              "metro_station": 1,
              "parking": 2
            },
            "debugInfo": {
              "found_stop_groups": "2",
              "found_metro": "true",
              "found_parking": "true",
              "found_railway": "false",
              "diversity_ratio": "3/4",
              "completeness_bonus": "yes"
            }
          }
        }
      }
    },
    "summary": "Инфраструктура развита выше среднего. Хорошая транспортная доступность. Рядом есть магазины и повседневная коммерция.",
    "highlights": [
      "Хорошая транспортная доступность.",
      "Рядом есть магазины и повседневная коммерция."
    ],
    "pois": []
  },
  "errors": []
}
```

## Caching and Limits

- provider responses are cached in memory by coordinates, radius and category set
- search radius is bounded by config
- max POI count is bounded by config
- results are deduplicated by external id, name and coordinates
- final scoring also applies grouped-entity deduplication and per-subcategory useful caps

## Frontend Coexistence With Yandex

No changes are required for frontend map rendering:

- Yandex Maps can still be used for map display
- Yandex geocoding can still be used for address resolution
- Infrastructure analytics now use Overpass under the hood

This means the user still sees Yandex maps in UI, but score and nearby infrastructure come from open-data POI search.
