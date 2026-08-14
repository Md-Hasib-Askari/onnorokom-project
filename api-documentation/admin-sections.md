# 00 · Admin Section Management

> All endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`). A section is a class group inside a grade (e.g., Grade 10 Section A / Section B). Students belong to a section, not directly to a grade, so a student's grade is derived through `Section.GradeId`. Subject-teacher assignment also lives here: a subject is a grade-level catalog entry, but the teacher who teaches it is chosen per section.

<details>
<summary><b>GET → List Sections</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/sections` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all non-deleted sections across every grade, each enriched with its parent grade's name. A section name is unique within its grade only, so the same name (e.g. `Section A`) recurs across grades. Each section carries live counts: `teacherCount` is the number of distinct teachers assigned to the section's subject links, and `studentCount` is the number of students enrolled in it.

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "id": "f7a1...",
      "name": "Section A",
      "gradeId": "d3b4...",
      "gradeName": "Grade 10",
      "teacherCount": 5,
      "studentCount": 28
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
<summary><b>POST → Create Section</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/sections` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Creates a section under an existing grade. The `name` must be unique among non-deleted sections of that grade.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Section name, e.g. `Section A` (max 100 chars) |
| `gradeId` | string (guid) | Yes | Grade the section belongs to |

## Example Request Body

```json
{
  "name": "Section A",
  "gradeId": "d3b4..."
}
```

## Successful Response `200 OK`

```json
{
  "id": "f7a1...",
  "name": "Section A",
  "gradeId": "d3b4...",
  "gradeName": "Grade 10"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "name": "Section name is required." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Grade with id <gradeId> was not found." }` | Grade does not exist |
| `409` | `{ "error": "Section 'Section A' in Grade 10 already exists." }` | Same name already exists in that grade |

</details>

---

<details>
<summary><b>PUT → Update Section</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/admin/sections/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Renames a section and/or moves it to another grade. The updated `name` must remain unique within the target grade.

Moving a section to a different grade clears all of its subject-teacher assignments. Those links point at the old grade's subjects, which the section no longer teaches: left in place they would be invisible in the section subject list (which only lists the new grade's subjects) yet still count as "teacher assigned", and would reappear if the section were moved back.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the section to update |

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Section name, e.g. `Section A` (max 100 chars) |
| `gradeId` | string (guid) | Yes | Grade the section belongs to |

## Example Request Body

```json
{
  "name": "Section B",
  "gradeId": "d3b4..."
}
```

## Successful Response `200 OK`

```json
{
  "id": "f7a1...",
  "name": "Section B",
  "gradeId": "d3b4...",
  "gradeName": "Grade 10"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "name": "Section name is required." } }` | Request fails validation |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Section with id <id> was not found." }` | Section does not exist |
| `404` | `{ "error": "Grade with id <gradeId> was not found." }` | Target grade does not exist |
| `409` | `{ "error": "Section 'Section B' in Grade 10 already exists." }` | Another non-deleted section in that grade uses the name |

</details>

---

<details>
<summary><b>DELETE → Delete Section</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `DELETE` |
| 🔗 URL | `/api/admin/sections/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Soft-deletes a section along with its subject-teacher assignments. Deleting is blocked while students are still enrolled in the section; reassign or remove those students first.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | ID of the section to delete |

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Section with id <id> was not found." }` | Section does not exist |
| `409` | `{ "error": "Section 'Section A' cannot be deleted because it has enrolled students." }` | Section has enrolled students |

</details>

---

<details>
<summary><b>GET → List Section Subjects</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/sections/:sectionId/subjects` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns one row for every subject of the section's grade, ordered by subject name, with the teacher assigned to that subject *in this section*. `teacherId`/`teacherName` are `null` where no teacher has been assigned yet, so the response is the complete subject list rather than only the filled slots.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `sectionId` | string (guid) | Yes | ID of the section |

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "subjectId": "b2c3...",
      "subjectName": "Mathematics",
      "subjectCode": "MATH-10",
      "teacherId": "c5d6...",
      "teacherName": "Jane Doe"
    },
    {
      "subjectId": "e4f5...",
      "subjectName": "Physics",
      "subjectCode": null,
      "teacherId": null,
      "teacherName": null
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
| `404` | `{ "error": "Section with id <sectionId> was not found." }` | Section does not exist |

</details>

---

<details>
<summary><b>POST → Assign Subject Teacher</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/admin/sections/:sectionId/subjects/:subjectId/teacher` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Assigns a teacher to one subject within one section. The call is an upsert: it creates the section-subject link on first assignment and replaces the teacher on later calls, so there is no separate "reassign" endpoint.

The subject must belong to the section's grade. The teacher must be an existing user with role `Teacher` whose account is approved and active. A teacher may hold assignments in any number of sections; there is no cross-section exclusivity.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `sectionId` | string (guid) | Yes | ID of the section |
| `subjectId` | string (guid) | Yes | ID of the subject, which must belong to the section's grade |

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `teacherId` | string (guid) | Yes | `AuthUser` ID of the teacher to assign |

## Example Request Body

```json
{
  "teacherId": "c5d6..."
}
```

## Successful Response `200 OK`

```json
{
  "subjectId": "b2c3...",
  "subjectName": "Mathematics",
  "subjectCode": "MATH-10",
  "teacherId": "c5d6...",
  "teacherName": "Jane Doe"
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "teacherId": "Teacher is required." } }` | Request fails validation |
| `400` | `{ "error": "User with id <teacherId> is not an approved active teacher." }` | Teacher does not exist, is not a `Teacher`, or is not approved/active |
| `400` | `{ "error": "Subject 'Mathematics' does not belong to this section's grade." }` | Subject belongs to a different grade than the section |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Section with id <sectionId> was not found." }` | Section does not exist |
| `404` | `{ "error": "Subject with id <subjectId> was not found." }` | Subject does not exist |

</details>

---

<details>
<summary><b>DELETE → Unassign Subject Teacher</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `DELETE` |
| 🔗 URL | `/api/admin/sections/:sectionId/subjects/:subjectId/teacher` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Clears the teacher for one subject within one section, leaving the subject itself untouched. The call is idempotent: unassigning a slot that has no teacher succeeds and returns the same empty subject row.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `sectionId` | string (guid) | Yes | ID of the section |
| `subjectId` | string (guid) | Yes | ID of the subject, which must belong to the section's grade |

## Successful Response `200 OK`

```json
{
  "subjectId": "b2c3...",
  "subjectName": "Mathematics",
  "subjectCode": "MATH-10",
  "teacherId": null,
  "teacherName": null
}
```

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Subject 'Mathematics' does not belong to this section's grade." }` | Subject belongs to a different grade than the section |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
| `404` | `{ "error": "Section with id <sectionId> was not found." }` | Section does not exist |
| `404` | `{ "error": "Subject with id <subjectId> was not found." }` | Subject does not exist |
</details>
