# ListingService API

Main routes:

- `/api/listings`
- lookup routes such as `/api/building_types`, `/api/layout_types`, `/api/heating_types`

## Listings

### `GET /api/listings`

Returns paged listings.

Query parameters:

- `page`
- `pageSize`

Response body:

- `Result<PagedResult<GetListingDto>>`

### `GET /api/listings/{id}`

Returns a listing summary by id.

Route parameters:

- `id`: listing id as `Guid`

Response body:

- `Result<GetListingDto>`

### `GET /api/listings/details/{id}`

Returns a listing with expanded details, media and price history.

Route parameters:

- `id`: listing id as `Guid`

Response body:

- `Result<GetDetailedListingDto>`

### `POST /api/listings`

Creates a listing.

Request body shape:

```json
{
  "listingDto": {
    "agentId": "00000000-0000-0000-0000-000000000001",
    "addressText": "Minsk, Example st. 1",
    "title": "Office for rent",
    "description": "Near metro",
    "priceAmount": 1500,
    "priceCurrency": "USD",
    "pricePeriod": "month",
    "areaTotal": 80,
    "buildYear": 2020,
    "publishedDate": "2026-03-12T10:00:00+03:00",
    "addressId": "00000000-0000-0000-0000-000000000002",
    "buildingType": "Business center",
    "listingType": "Rent",
    "status": "Published",
    "mediaUrls": [
      "https://example.com/1.jpg"
    ],
    "listingDetailsType": "Office",
    "listingDetails": {
      "plotArea": 80,
      "floor": 5,
      "floorsTotal": 10,
      "levels": 1,
      "parkingSpaces": 2,
      "kitchenette": true,
      "officeClass": "A",
      "layoutType": "Open space",
      "renovationType": "Modern",
      "parkingType": "Underground"
    }
  }
}
```

Important validation rules:

- `title` is required, max 100 chars
- `description` max 1000 chars
- `priceAmount` must be greater than `0`
- `listingDetailsType` selects the expected `listingDetails` shape
- lookup-based string values must exist in the corresponding lookup table

Supported `listingDetailsType` values:

- `Apartment`
- `House`
- `Land`
- `Warehouse`
- `Office`

Response body:

- success: empty `200 OK`
- validation failure: `ApiProblemDetails`

## Lookup endpoints

All lookup routes follow the same pattern:

- `GET /api/{lookup}`: get all values
- `POST /api/{lookup}`: create value
- `DELETE /api/{lookup}/{name}`: delete value by name

Request body for lookup creation:

```json
{
  "name": "Example"
}
```

Lookup routes discovered in the backend:

- `/api/bathroom_types`
- `/api/building_materials`
- `/api/building_types`
- `/api/class_types`
- `/api/gas_types`
- `/api/heating_types`
- `/api/land_categories`
- `/api/layout_types`
- `/api/listing_types`
- `/api/office_classes`
- `/api/renovation_types`
- `/api/road_access_types`
- `/api/sewer_types`
- `/api/wall_materials`
- `/api/water_supply_types`

Typical responses:

- `GET`: `Result<List<LookupNameDto>>`
- `POST`: usually empty `200 OK`
- `DELETE`: usually empty `200 OK`

## DTO summary

### `GetListingDto`

```json
{
  "id": "guid",
  "addressText": "Minsk, Example st. 1",
  "title": "Office for rent",
  "description": "Near metro",
  "priceAmount": 1500,
  "priceCurrency": "USD",
  "pricePeriod": "month",
  "areaTotal": 80,
  "buildYear": 2020,
  "mainMedia": "https://example.com/1.jpg",
  "publishedDate": "2026-03-12T10:00:00+03:00",
  "buildingType": "Business center",
  "listingType": "Rent"
}
```

### `LookupNameDto`

```json
{
  "name": "Example"
}
```
