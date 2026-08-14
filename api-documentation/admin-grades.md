# 00 · Admin Grade Management

> All endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`). A grade represents a class level (e.g., Grade 1 through Grade 12), not a course or subject name. `database/seed/03_grades_and_sections.sql` pre-seeds `Grade 1` through `Grade 12` for the current academic year, each with a `Section A`, so a fresh deployment can enrol students right away; this module is used to manage other years or extra grades.

<details>
<summary><b>GET → List Grades</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/grades` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all non-deleted grades. The same grade name recurs across academic years (e.g., Grade 1 in 2026 and 2027), so a grade is identified by the combination of `name` and `academicYear`. Each grade carries live counts: `teacherCount` is the number of distinct teachers assigned to any section-subject link in the grade's sections, and `studentCount` is the number of enrolled students in those sections.

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "id": "d3b4...",
      "name": "Grade 1",
      "academicYear": "2026",
      "description": "Primary section",
      "teacherCount": 4,
      "studentCount": 32
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
<summary><b>POST → Create Grade</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/grades` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Creates a new grade for a class level and academic year. The `name` + `academicYear` combination must be unique among non-deleted grades.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Class level, e.g. `Grade 1` (max 100 chars) |
| `academicYear` | string | Yes | Academic year, e.g. `2026` (max 20 chars) |
| `description` | string | No | Optional description (max 500 chars) |

## Example Request Body

```json
{
  "name": "Grade 1",
  "academicYear": "2026",
  "description": "Primary section"
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "name": "Grade 1",
  "academicYear": "2026",
  "description": "Primary section"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "name": "..." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `409` | `{ "error": "Grade 'Grade 1' for academic year 2026 already exists." }` | Same name + academic year already exists |

</details>

---

<details>
<summary><b>PUT → Update Grade</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/admin/grades/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Updates the name, academic year, and/or description of an existing grade. The updated `name` + `academicYear` must remain unique among non-deleted grades.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the grade to update |

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Class level, e.g. `Grade 1` (max 100 chars) |
| `academicYear` | string | Yes | Academic year, e.g. `2026` (max 20 chars) |
| `description` | string | No | Optional description (max 500 chars) |

## Example Request Body

```json
{
  "name": "Grade 2",
  "academicYear": "2026",
  "description": "Primary section"
}
```

## Successful Response `200 OK`

```json
{
  "id": "d3b4...",
  "name": "Grade 2",
  "academicYear": "2026",
  "description": "Primary section"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "name": "..." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Grade with id <id> was not found." }` | Grade does not exist |
| `409` | `{ "error": "Grade 'Grade 2' for academic year 2026 already exists." }` | Another non-deleted grade already uses the name + academic year |

</details>

---

<details>
<summary><b>DELETE → Delete Grade</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `DELETE` |
| 🔗 URL | `/api/admin/grades/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Soft-deletes a grade. Deleting is blocked while the grade still has subjects, sections, or enrolled students. Students reach their grade through a section, so the enrolled-students guard is evaluated transitively via `Section.GradeId`.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the grade to delete |

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Grade with id <id> was not found." }` | Grade does not exist |
| `409` | `{ "error": "Grade 'Grade 1' cannot be deleted because it has subjects assigned." }` | Grade has subjects |
| `409` | `{ "error": "Grade 'Grade 1' cannot be deleted because it has sections." }` | Grade has sections |
| `409` | `{ "error": "Grade 'Grade 1' cannot be deleted because it has enrolled students." }` | Grade has enrolled students |
</details>
