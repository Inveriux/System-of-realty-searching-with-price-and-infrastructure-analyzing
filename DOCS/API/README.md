# API Documentation

This project now exposes standard ASP.NET Core OpenAPI documents for both backend services.

Available runtime documents:

- `UserService`: `/openapi/v1.json`
- `ListingService`: `/openapi/v1.json`

Generated files in this folder:

- `user-service.openapi.json`
- `UserService.md`
- `ListingService.md`

How to refresh the generated OpenAPI files:

1. Run the target service.
2. Request `/openapi/v1.json`.
3. Save the response into the corresponding file in `DOCS/API`.

Notes:

- `UserService` uses JWT and Google authentication flows.
- `ListingService` uses minimal API endpoints for listings and lookup dictionaries.
- The markdown files are hand-curated summaries.
- The `.openapi.json` files are machine-readable documents for tooling.
- `ListingService` currently also has a hand-curated markdown description because the service restore/build is failing in the current workspace state.
