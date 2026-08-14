# 00 · Authentication

> All auth endpoints are `AllowAnonymous`. Protected resources use a JWT bearer access token (`Authorization: Bearer <accessToken>`). New accounts are created in `Pending` status and cannot log in until an admin approves them. The role-specific profile is provisioned automatically at registration, except for students, whose profile waits until an admin approves them and picks their section.
>
> Every endpoint in this module is rate limited per client IP (fixed window). Exceeding the limit (default `10` requests per `60` seconds, configurable in the `RateLimiting` appsettings section) returns `429` `{ "error": "Too many requests. Try again later." }`.

<details>
<summary><b>GET → Registration Policy</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/auth/registration-policy` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Reports which roles an admin has opened to public sign-up, so the register form can offer only those. Anonymous by necessity, and safe because it returns two policy booleans rather than the settings store. Admins manage these flags through [Admin Settings](admin-settings.md).

## Successful Response `200 OK`

```json
{
  "teacherSelfRegistrationEnabled": true,
  "studentSelfRegistrationEnabled": false
}
```

## Error Responses

None beyond the generic `500`.

</details>

---

<details>
<summary><b>POST → Register</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/auth/register` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Creates a new user account in `Pending` status. The email is trimmed and lowercased before storage, so login is case-insensitive. The user cannot log in until an administrator approves the account. The `Admin` role cannot be chosen during public registration; admins are only created by other admins.

Whether a role may register at all is an admin setting, not a fixed rule. The check runs before the email lookup, so a closed role reports the closure rather than a duplicate-email conflict: otherwise a shut role could be used to probe which addresses already have accounts. See [Admin Settings](admin-settings.md) for the flags and their defaults.

A teacher's profile is provisioned in the same transaction as the account. A student's is not: sign-up carries no section, so there is nothing to enrol them into yet. The profile is created when an admin approves them and picks the section, which also keeps the section roster out of a form anonymous visitors can read.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `fullName` | string | Yes | Full display name |
| `email` | string | Yes | Login email (case-insensitive) |
| `password` | string | Yes | Password, hashed with a password hasher before storage. Min 8 chars, must contain uppercase, lowercase, digit, and special character |
| `role` | string | No | `Teacher` or `Student`. Defaults to `Student`; `Admin` is rejected |

## Example Request Body

```json
{
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "password": "••••••••",
  "role": "Student"
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "email": "jane@example.com",
  "fullName": "Jane Doe",
  "role": "Student",
  "status": "Pending"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "email": "..." } }` | Request fails validation (e.g. invalid email, weak password, or `Admin` role) |
| `403` | `{ "error": "Self-registration is currently closed for the Student role. Contact an administrator." }` | An admin has closed public sign-up for that role |
| `409` | `{ "error": "A user with email 'jane@example.com' already exists." }` | Email already registered |

</details>

---

<details>
<summary><b>POST → Login</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/auth/login` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Authenticates a user and issues a JWT access token plus a refresh token. The account must be approved and active; pending or rejected accounts cannot log in. Tokens are persisted on the user for later refresh.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | string | Yes | Login email (case-insensitive) |
| `password` | string | Yes | Account password |

## Example Request Body

```json
{
  "email": "jane@example.com",
  "password": "••••••••"
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

`mustChangePassword` is `true` when an admin reset this account's password (see [Admin Users](admin-users.md)); the frontend forces the user to `/change-password` before letting them use the rest of the app.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Invalid email or password." }` | Email not found or password mismatch |
| `400` | `{ "error": "Account is pending approval by an administrator." }` | Account is not yet approved |
| `400` | `{ "error": "Account has been rejected by an administrator." }` | Account was rejected |
| `400` | `{ "error": "Account is inactive. Contact an administrator." }` | Account is soft-deleted / inactive |

</details>

---

<details>
<summary><b>POST → Refresh Token</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/auth/refresh` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Exchanges a valid, non-expired refresh token for a fresh access token and a new refresh token. The previous refresh token is rotated. Requires the account to be approved and active.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `refreshToken` | string | Yes | Refresh token issued at login or the last refresh |

## Example Request Body

```json
{
  "refreshToken": "a1b2c3..."
}
```

## Successful Response `200 OK`

```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "d4e5f6...",
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
| `400` | `{ "error": "Refresh token is invalid or has expired." }` | Unknown, revoked, or expired refresh token |

</details>

---

<details>
<summary><b>POST → Logout</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/auth/logout` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Revokes the given refresh token server-side by clearing it (and its expiry) from the user record, so it can no longer be exchanged via `/api/auth/refresh`. An unknown or already-revoked token is treated as a no-op rather than an error, since the caller's intent (be logged out) is already satisfied. The access token is not revoked; it simply expires naturally per its own TTL, and the frontend clears its cookies regardless of whether this call succeeds. `AuthUser` stores a single refresh token per account, so this revokes the account's only active refresh token, not just the calling session's.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `refreshToken` | string | Yes | Refresh token to revoke |

## Example Request Body

```json
{
  "refreshToken": "a1b2c3..."
}
```

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "refreshToken": "Refresh token is required." } }` | `refreshToken` missing or empty |

</details>

---

<details>
<summary><b>POST → Forgot Password</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/auth/forgot-password` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Emails a 6-digit reset code to the given address if an account exists for it. Always responds `204` regardless of whether the email matches an account, so the endpoint cannot be used to enumerate registered addresses. Requests for the same account are rate-limited to one every 60 seconds; each code expires after 10 minutes and is single-use, consumed by the Reset Password endpoint below. Requesting a new code does not invalidate a still-valid one issued moments earlier by a different request, but only the most recently issued code is checked against on reset.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | string | Yes | Account email to send the reset code to |

## Example Request Body

```json
{
  "email": "jane@example.com"
}
```

## Successful Response `204 No Content`

(no body, whether or not the email matched an account)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "email": "A valid email address is required." } }` | `email` missing or malformed |
| `400` | `{ "error": "A code was already sent recently. Please wait before requesting another." }` | A code was requested for this account within the last 60 seconds |

</details>

---

<details>
<summary><b>POST → Reset Password</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/auth/reset-password` |
| ✅ Status | Completed |
| 📦 Auth | Not required |
| 📁 Content-Type | `application/json` |

## Description

Consumes the most recent reset code sent to `email` and sets `newPassword` on the account. The code must be the latest one issued, unexpired, not already used, and have fewer than 5 failed attempts against it; a wrong code registers a failed attempt (the 5th failure locks that code out even if it hasn't expired yet, requiring a fresh [Forgot Password](#post--forgot-password) request). On success the code is marked used, so it cannot be replayed. This does not set `mustChangePassword`, since the user chose the new password themselves; contrast with [Admin → Reset Password](admin-users.md), which forces a change on next login.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | string | Yes | Account email the code was sent to |
| `code` | string | Yes | 6-digit reset code from the email |
| `newPassword` | string | Yes | New password. Min 8 chars, must contain uppercase, lowercase, digit, and special character |

## Example Request Body

```json
{
  "email": "jane@example.com",
  "code": "482913",
  "newPassword": "••••••••"
}
```

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "code": "Code must be 6 digits." } }` | Request fails validation |
| `400` | `{ "error": "The code is invalid or has expired." }` | No code on file, code does not match the latest one, or it has expired/already been used |
</details>
