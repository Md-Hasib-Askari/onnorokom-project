# 00 · Admin Settings

> System-wide business rules an admin can change from the UI, stored as rows in `SystemSettings` rather than in configuration files, so a change takes effect without a redeploy. Each setting is a typed key from the `SystemSettingKey` enum paired with a string value; the service layer is what knows how to read that value as a boolean. `database/seed/01_system_settings.sql` seeds any key that is missing, so a fresh database and an upgraded one both end up with a full set. Endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`).
>
> Anonymous visitors read the same registration policy through [`GET /api/auth/registration-policy`](auth.md), which returns only the two booleans and never the settings store itself.

<details>
<summary><b>GET → Get System Settings</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/settings` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all admin-tunable settings in a single payload: the public registration policy and the profile self-editing policy. The admin UI fetches them together so one save writes the full intent.

A registration key that is missing from the database reads as `false`: the fallback is deliberately restrictive, so a settings row that never got seeded closes a role rather than opening it. A profile-edit key that is missing reads as `true`, the opposite direction: self-editing is open by default and an admin must take an explicit action to close it. A value that no boolean parser accepts (only reachable by editing the row by hand) reads as `false` in both cases.

`Admin` is never part of the registration response. Admin accounts are created by other admins, never self-served, so there is no flag to expose.

## Successful Response `200 OK`

```json
{
  "teacherSelfRegistrationEnabled": true,
  "studentSelfRegistrationEnabled": false,
  "teacherProfileSelfEditEnabled": true,
  "studentProfileSelfEditEnabled": true
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |

</details>

---

<details>
<summary><b>PUT → Update System Settings</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/admin/settings` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Writes all four flags at once, so a save records the admin's full intent instead of a delta and both policies commit together or not at all. Rows that already exist are updated in place; rows that are missing are inserted, which means the endpoint works even against a database that predates the seeding.

Closing a role does not touch accounts that already registered under it. Anyone already sitting in the pending queue stays there and can still be approved. Closing profile self-editing only stops future edits; it does not revert anything already changed.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `teacherSelfRegistrationEnabled` | boolean | Yes | Whether teachers may create their own account |
| `studentSelfRegistrationEnabled` | boolean | Yes | Whether students may create their own account |
| `teacherProfileSelfEditEnabled` | boolean | Yes | Whether teachers may edit their own role-specific profile fields |
| `studentProfileSelfEditEnabled` | boolean | Yes | Whether students may edit their own role-specific profile fields |

## Example Request Body

```json
{
  "teacherSelfRegistrationEnabled": true,
  "studentSelfRegistrationEnabled": true,
  "teacherProfileSelfEditEnabled": true,
  "studentProfileSelfEditEnabled": true
}
```

## Successful Response `200 OK`

```json
{
  "teacherSelfRegistrationEnabled": true,
  "studentSelfRegistrationEnabled": true,
  "teacherProfileSelfEditEnabled": true,
  "studentProfileSelfEditEnabled": true
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "teacherSelfRegistrationEnabled": "..." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |

</details>

