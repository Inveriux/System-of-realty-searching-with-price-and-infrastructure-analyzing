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

Scoring logic lives in:

- [InfrastructureScoringService.cs](/home/Admin/Projects/Diploma/src/Backend/Services/InfrastructureService/Services/Scoring/InfrastructureScoringService.cs)

Practical formula:

- each category gets `0..100`
- nearest POI contributes up to `60`
- category density contributes up to `40`
- total score is a weighted average of category scores

This is simple enough for a diploma explanation and realistic enough for a production-like MVP.

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
    "DefaultRadiusMeters": 1500,
    "MaxRadiusMeters": 3000,
    "MaxPoiCount": 100,
    "CacheMinutes": 10
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
        "transport_count": 10,
        "transport_nearest_m": 95,
        "transport_score": 90
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

## Frontend Coexistence With Yandex

No changes are required for frontend map rendering:

- Yandex Maps can still be used for map display
- Yandex geocoding can still be used for address resolution
- Infrastructure analytics now use Overpass under the hood

This means the user still sees Yandex maps in UI, but score and nearby infrastructure come from open-data POI search.
