# 00 · Admin Subject Management

> All endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`). A subject belongs to exactly one grade; its name must be unique within that grade. The `code` is optional and not part of any uniqueness rule.
>
> A subject is a grade-level catalog entry and carries no teacher of its own: the teacher varies per section, so teacher assignment lives in [Admin Sections](admin-sections.md) (`/api/admin/sections/:sectionId/subjects/:subjectId/teacher`).

<details>
<summary><b>GET → List Subjects</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/subjects` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all non-deleted subjects, including the resolved grade name and `teacherCount`, the number of distinct teachers assigned to the subject across every section-subject link.

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "id": "a1b2...",
      "name": "Mathematics",
      "code": null,
      "gradeId": "d3b4...",
      "gradeName": "Grade 1",
      "teacherCount": 2
    }
  ],
  "nextCursor": null,
  "hasMore": false
}
```

This list is small enough to return in one page, so `nextCursor` is always `null` and `hasMore` is always `false`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |

</details>

---

<details>
<summary><b>POST → Create Subject</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/subjects` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Creates a new subject for a grade, with an optional code. The subject name must be unique within the grade.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Subject name, e.g. `Mathematics` (max 100 chars) |
| `gradeId` | string (guid) | Yes | ID of the grade the subject belongs to |
| `code` | string | No | Optional subject code, e.g. `MATH-101` (max 20 chars) |

## Example Request Body

```json
{
  "name": "Mathematics",
  "gradeId": "d3b4...",
  "code": "MATH-101"
}
```

## Successful Response `200 OK`

```json
{
  "id": "a1b2...",
  "name": "Mathematics",
  "code": "MATH-101",
  "gradeId": "d3b4...",
  "gradeName": "Grade 1"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "name": "..." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Grade with id <gradeId> was not found." }` | Grade does not exist |
| `409` | `{ "error": "Subject 'Mathematics' in Grade 1 already exists." }` | Subject name already used in this grade |

</details>

---

<details>
<summary><b>PUT → Update Subject</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/admin/subjects/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Updates the name, code, and grade of an existing subject. The updated name must remain unique within the target grade.

Moving a subject to a different grade clears every section-teacher assignment it holds. Those links tie the subject to sections of the grade it just left, a pairing the section subject list rejects outright: left in place they would be invisible in every subject list yet still count as "teacher assigned", and would reappear if the subject were moved back.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the subject to update |

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Subject name (max 100 chars) |
| `gradeId` | string (guid) | Yes | ID of the grade the subject belongs to |
| `code` | string | No | Optional subject code (max 20 chars) |

## Example Request Body

```json
{
  "name": "Mathematics",
  "gradeId": "d3b4...",
  "code": null
}
```

## Successful Response `200 OK`

```json
{
  "id": "a1b2...",
  "name": "Mathematics",
  "code": null,
  "gradeId": "d3b4...",
  "gradeName": "Grade 1"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "name": "..." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Subject with id <id> was not found." }` | Subject does not exist |
| `404` | `{ "error": "Grade with id <gradeId> was not found." }` | Grade does not exist |
| `409` | `{ "error": "Subject 'Mathematics' in Grade 1 already exists." }` | Another non-deleted subject in this grade uses the name |

</details>

---

<details>
<summary><b>DELETE → Delete Subject</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `DELETE` |
| 🔗 URL | `/api/admin/subjects/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Soft-deletes a subject along with its section-teacher assignments. Deleting is blocked while the subject still has assignments.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the subject to delete |

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Subject with id <id> was not found." }` | Subject does not exist |
| `409` | `{ "error": "Subject 'Mathematics' cannot be deleted because it has assignments." }` | Subject has assignments |
</details>
