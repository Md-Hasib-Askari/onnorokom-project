# 00 · Profile

> All endpoints require an authenticated session (any role). Operate on the caller's own account; there is no `:id` in the URL because the user is resolved from the access token.

<details>
<summary><b>GET → Get Profile</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/profile` |
| ✅ Status | Completed |
| 📦 Auth | Required (any role) |
| 📁 Content-Type | `application/json` |

## Description

Returns the caller's own account details, including whether they are still required to change their password (see [Admin → Reset Password](admin-users.md)).

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "role": "Student",
  "mustChangePassword": false
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |

</details>

---

<details>
<summary><b>PUT → Update Profile</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/profile` |
| ✅ Status | Completed |
| 📦 Auth | Required (any role) |
| 📁 Content-Type | `application/json` |

## Description

Updates the caller's own display name. Email cannot be changed through this endpoint.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `fullName` | string | Yes | New display name, max 100 chars |

## Example Request Body

```json
{
  "fullName": "Jane A. Doe"
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "fullName": "Jane A. Doe",
  "email": "jane@example.com",
  "role": "Student",
  "mustChangePassword": false
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "fullName": "Full name is required." } }` | `fullName` missing, empty, or over 100 chars |
| `401` | (no body) | Not authenticated (missing/invalid access token) |

</details>

---

<details>
<summary><b>POST → Change Password</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/profile/change-password` |
| ✅ Status | Completed |
| 📦 Auth | Required (any role) |
| 📁 Content-Type | `application/json` |

## Description

Verifies `currentPassword`, sets `newPassword`, and clears `mustChangePassword`. This is the endpoint the frontend forces the user through when their session reports `mustChangePassword: true` (e.g. after an admin reset their password), and it also powers the regular "change my password" action from the profile page. Reissues an access and refresh token pair for the current session so it stays logged in; the previous refresh token is revoked, so any other active session is signed out.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `currentPassword` | string | Yes | Caller's current password |
| `newPassword` | string | Yes | New password. Min 8 chars, must contain uppercase, lowercase, digit, and special character, and must differ from `currentPassword` |

## Example Request Body

```json
{
  "currentPassword": "••••••••",
  "newPassword": "••••••••••"
}
```

## Successful Response `200 OK`

```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "a1b2c3...",
  "accessTokenExpiresAt": "2026-08-07T10:00:00Z",
  "userId": "d3b4...",
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "role": "Student",
  "status": "Approved",
  "mustChangePassword": false
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "newPassword": "New password must be different from the current password." } }` | Request fails validation |
| `400` | `{ "error": "Current password is incorrect." }` | `currentPassword` does not match |
| `401` | (no body) | Not authenticated (missing/invalid access token) |</details>
