# UserService API

Base route: `/api/users`

Authentication:

- `GET /info`
- `GET /info/{email}`
- `POST /change-password`

These endpoints require a bearer token. Google endpoints use the external Google authentication flow.

## Endpoints

### `GET /api/users/info`

Returns the current authenticated user.

Response body:

- `Result<GetUserDto>`

### `GET /api/users/info/{email}`

Returns a user profile by email.

Route parameters:

- `email`: user email

Authorization:

- authenticated user
- owner-or-admin access filter

Response body:

- `Result<GetUserDto>`

### `POST /api/users/signup`

Creates a local account.

Request body:

```json
{
  "name": "Alice",
  "email": "alice@example.com",
  "password": "password123",
  "accountImageUrl": "https://example.com/avatar.jpg"
}
```

Response body:

- `Result<bool>`

Validation highlights:

- `name` is required
- `email` must be valid and unique
- `password` must satisfy ASP.NET Identity password rules

### `POST /api/users/signin`

Signs in a local account.

Request body:

```json
{
  "email": "alice@example.com",
  "password": "password123"
}
```

Response body:

```json
{
  "isSuccess": true,
  "message": null,
  "statusCode": 0,
  "data": {
    "success": true,
    "token": "jwt-access-token",
    "refreshToken": "refresh-token"
  },
  "errors": []
}
```

### `POST /api/users/refresh-token`

Rotates refresh token and returns a new access token.

Request body:

```json
{
  "refreshToken": "refresh-token"
}
```

Response body:

- `Result<RefreshTokenResponseDto>`

### `POST /api/users/change-password`

Validates password change request for the authenticated user.

Request body:

```json
{
  "email": "alice@example.com",
  "currentPassword": "old-password",
  "newPassword": "new-password"
}
```

Response body:

- `Result<bool>`

Validation highlights:

- `email` must be valid
- `currentPassword` is required
- `newPassword` is required
- `newPassword` must differ from `currentPassword`
- `newPassword` is validated by ASP.NET Identity password validators

### `POST /api/users/google/signup`

Starts the Google sign-up redirect flow.

Behavior:

- responds with an authentication challenge redirect to Google

### `GET /api/users/google/result`

Completes Google sign-up after the Google callback.

Response body:

- `Result<bool>`

### `POST /api/users/google/signin`

Signs in an existing Google-linked user after the external auth session exists.

Response body:

- `Result<SignInUserResponseDto>`

## DTOs

### `GetUserDto`

```json
{
  "id": "guid",
  "name": "Alice",
  "email": "alice@example.com",
  "accountImageUrl": "https://example.com/avatar.jpg",
  "role": "User"
}
```

### `SignInUserResponseDto`

```json
{
  "success": true,
  "token": "jwt-access-token",
  "refreshToken": "refresh-token"
}
```

### `RefreshTokenResponseDto`

```json
{
  "refreshToken": "refresh-token",
  "token": "jwt-access-token"
}
```
