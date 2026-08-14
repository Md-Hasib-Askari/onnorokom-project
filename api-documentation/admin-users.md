# 00 · Admin User Management

> All endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`). Users created by an admin are approved immediately and can log in right away. Self-registered users start `Pending` and are approved via the approve endpoint. Every user has an auto-provisioned profile (`AdminProfile`/`TeacherProfile`/`StudentProfile`) created in the same transaction as the record that makes it meaningful: the user record itself for admins, teachers, and admin-created students, and the approval for a self-registered student, since that is the first point at which a section is known.

<details>
<summary><b>POST → Approve / Reject User</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/users/approve` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Approves or rejects a pending user account based on the `approve` flag. Approving transitions the account to `Approved` (enabling login); rejecting transitions it to `Rejected`. Only pending users can be approved, and the last usable admin cannot be rejected.

Approving a self-registered student is also an enrolment decision. Sign-up carries no section, so such a student has no profile yet and `studentSectionId` is required here; the student profile is created in the same transaction as the approval. A student an admin created already has a section, and every non-student role never had one, so `studentSectionId` is ignored in both cases.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | string (guid) | Yes | ID of the user to review |
| `approve` | boolean | Yes | `true` to approve, `false` to reject |
| `studentSectionId` | string (guid) | Conditional | Section to enrol the student into. Required when approving a `Student` who has no profile yet; ignored otherwise |

## Example Request Body

```json
{
  "userId": "d3b4...",
  "approve": true,
  "studentSectionId": "9f21..."
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "email": "jane@example.com",
  "fullName": "Jane Doe",
  "role": "Student",
  "status": "Approved"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Only pending users can be approved." }` | User is not `Pending` |
| `400` | `{ "error": "A section is required to approve a student account." }` | Approving a self-registered student without `studentSectionId` |
| `400` | `{ "error": "The last admin account cannot be rejected." }` | Rejecting would leave no usable admin |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "User with id <userId> was not found." }` | User does not exist |
| `404` | `{ "error": "Section with id <studentSectionId> was not found." }` | `studentSectionId` does not match a section |

</details>

---

<details>
<summary><b>GET → List Users</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/users` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns non-deleted users (admins, teachers, and students) with their account status, as a cursor-paginated page ordered by `(CreatedAt, Id)` ascending (oldest first). For `Student` users with a provisioned profile, `studentSectionId`/`sectionName` resolve their current section and `gradeName` resolves that section's grade; all three are `null` for every other role.

The pending-approval queue is this endpoint with `?status=Pending`; there is no separate pending endpoint anymore. The `?role` filter is used by the teacher-picker in the admin UI and by clients that only need one role.

## Query Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `limit` | int | No | Page size. Default 20, hard cap 100 |
| `cursor` | string | No | Opaque cursor from the previous page's `nextCursor`; omit for the first page |
| `status` | string | No | Filter by `Pending`, `Approved`, or `Rejected` |
| `role` | string | No | Filter by `Admin`, `Teacher`, or `Student` |

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "id": "d3b4...",
      "fullName": "Jane Doe",
      "email": "jane@example.com",
      "role": "Student",
      "status": "Approved",
      "createdAt": "2026-08-07T09:00:00Z",
      "isActive": true,
      "studentSectionId": "a1b2...",
      "sectionName": "Section A",
      "gradeName": "Grade 1"
    }
  ],
  "nextCursor": "eyJrIjpb...",
  "hasMore": false
}
```

`nextCursor` is `null` when the last page has been reached. `hasMore` reports whether further pages exist without decoding the cursor.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Invalid pagination cursor." }` | Malformed, tampered, or non-matching cursor |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |

</details>

---

<details>
<summary><b>POST → Create User</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/users` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Creates a user and immediately approves the account, so the user can log in. The role-specific profile is provisioned automatically in the same transaction. Student users must supply `studentSectionId`; the student's grade is derived from that section.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `fullName` | string | Yes | Full display name (max 100 chars) |
| `email` | string | Yes | Login email (case-insensitive, max 254 chars) |
| `password` | string | Yes | Password, hashed with a password hasher before storage. Min 8 chars, must contain uppercase, lowercase, digit, and special character |
| `role` | string | Yes | `Admin`, `Teacher`, or `Student` |
| `studentSectionId` | string (guid) | No | Section the student belongs to. Required when `role` is `Student` |

## Example Request Body

```json
{
  "fullName": "John Smith",
  "email": "john@example.com",
  "password": "••••••••",
  "role": "Student",
  "studentSectionId": "d3b4..."
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "fullName": "John Smith",
  "email": "john@example.com",
  "role": "Student",
  "status": "Approved",
  "createdAt": "2026-08-08T10:00:00Z",
  "isActive": true,
  "studentSectionId": "d3b4...",
  "sectionName": "Section A",
  "gradeName": "Grade 1"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "email": "..." } }` | Request fails validation (e.g. weak password). `errors` has one entry per field (first failing rule wins) even if a field fails several rules |
| `400` | `{ "error": "A section is required for student users." }` | Student user without `studentSectionId` |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Section with id <studentSectionId> was not found." }` | Section does not exist |
| `409` | `{ "error": "A user with email 'john@example.com' already exists." }` | Email already registered |

</details>

---

<details>
<summary><b>PUT → Update User</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/admin/users/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Updates a user's details, account status, and activation flag, plus role-specific profile data. The profile is created on first update if it is missing. `Pending` cannot be set here; approval/rejection of pending users goes through the approve endpoint. The last usable admin cannot be rejected or deactivated.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the user to update |

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `fullName` | string | Yes | Full display name |
| `email` | string | Yes | Login email (case-insensitive) |
| `status` | string | Yes | `Approved` or `Rejected` (`Pending` is rejected by the API) |
| `isActive` | boolean | Yes | Whether the account is active (independent of status) |
| `studentSectionId` | string (guid) | No | New section for students. Required for student users |
| `teacherProfile` | object | No | Teacher profile data (see below). Only used for teachers |
| `adminProfile` | object | No | Admin profile data (see below). Only used for admins |

`teacherProfile` fields: `department`, `designation`, `qualification`, `phoneNumber`, `address` (strings), `dateOfJoining` (ISO-8601 date). `adminProfile` fields: `position`, `phoneNumber` (strings).

## Example Request Body

```json
{
  "fullName": "John Smith",
  "email": "john@example.com",
  "status": "Approved",
  "isActive": true,
  "studentSectionId": "d3b4..."
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "fullName": "John Smith",
  "email": "john@example.com",
  "role": "Student",
  "status": "Approved",
  "createdAt": "2026-08-08T10:00:00Z",
  "isActive": true,
  "studentSectionId": "d3b4...",
  "sectionName": "Section A",
  "gradeName": "Grade 1"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "email": "..." } }` | Request fails validation |
| `400` | `{ "error": "Pending cannot be set via user update; use the approval endpoint." }` | `status` is `Pending` |
| `400` | `{ "error": "The last admin account cannot be deactivated or rejected." }` | Update would leave no usable admin |
| `400` | `{ "error": "A section is required for student users." }` | Student user without `studentSectionId` |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "User with id <id> was not found." }` | User does not exist |
| `404` | `{ "error": "Section with id <studentSectionId> was not found." }` | Section does not exist |
| `409` | `{ "error": "A user with email 'john@example.com' already exists." }` | Email already in use by another user |

</details>

---

<details>
<summary><b>DELETE → Delete User</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `DELETE` |
| 🔗 URL | `/api/admin/users/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Soft-deletes a user and their profile. Deleting your own account is not allowed, and the last usable admin cannot be deleted. Users referenced by existing records (e.g., a teacher with assigned subjects) cannot be deleted. The admin frontend also hides the Delete action on the caller's own row so this rule is never hit through the UI; it still applies server-side for any other client.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the user to delete |

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "You cannot delete your own account." }` | Caller is deleting themselves |
| `400` | `{ "error": "The last admin account cannot be deleted." }` | Deleting would leave no usable admin |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "User with id <id> was not found." }` | User does not exist |
| `409` | `{ "error": "User 'Jane Doe' cannot be deleted because they are referenced by existing records." }` | User is referenced by subjects, assignments, submissions, or grades |

</details>

---

<details>
<summary><b>POST → Reset Password</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/users/:id/reset-password` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Generates a new random password for the user, sets `mustChangePassword` on their account, and emails the temporary password to them. The user must set their own password via [Profile → Change Password](profile.md) on next login before they can use the rest of the app; the frontend enforces this by redirecting to `/change-password` whenever the session reports `mustChangePassword: true`. This is separate from the self-service [Forgot / Reset Password](auth.md) flow, which does not force a change since the user chose their own new password.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the user whose password to reset |

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "User with id <id> was not found." }` | User does not exist |
</details>
