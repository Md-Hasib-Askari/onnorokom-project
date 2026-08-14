# 00 · Admin Assignment & Submission Queries

> All endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`). These are read-only overview queries. Teachers create and manage assignments through [Teacher Workspace](teacher.md), and students submit work through [Student Workspace](student.md).

<details>
<summary><b>GET → List Assignments</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/assignments` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all non-deleted assignments with the resolved subject, grade, and teacher names, as a cursor-paginated page ordered by `(CreatedAt, Id)` descending (newest first). The submission count is scoped to the rows on the current page, so it stays correct while paging.

## Query Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `limit` | int | No | Page size. Default 20, hard cap 100 |
| `cursor` | string | No | Opaque cursor from the previous page's `nextCursor`; omit for the first page |

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "id": "f6e7...",
      "title": "Chapter 1 Exercise",
      "description": "Solve problems 1-10",
      "sectionId": "f7a1...",
      "sectionName": "Section A",
      "subjectId": "a1b2...",
      "subjectName": "Mathematics",
      "gradeName": "Grade 1",
      "teacherId": "c5d6...",
      "teacherName": "Jane Doe",
      "deadline": "2026-08-15T23:59:00Z",
      "maxMarks": 100,
      "status": "Published",
      "allowLateSubmission": true,
      "submissionCount": 4
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
<summary><b>GET → List Submissions</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/submissions` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all submissions with the resolved assignment title and student name, as a cursor-paginated page ordered by `(SubmittedAt, Id)` descending (newest first). Grading data (`marks`, `feedback`) is `null` until a teacher grades the submission. `status` is one of `Submitted`, `Resubmitted`, `Returned`, or `Graded`; lateness is derived from `submittedAt` and is not a separate status.

## Query Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `limit` | int | No | Page size. Default 20, hard cap 100 |
| `cursor` | string | No | Opaque cursor from the previous page's `nextCursor`; omit for the first page |

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "id": "b2c3...",
      "assignmentId": "f6e7...",
      "assignmentTitle": "Chapter 1 Exercise",
      "studentId": "9a8b...",
      "studentName": "John Smith",
      "content": "1. a=4 2. b=9",
      "attachmentUrl": null,
      "status": "Submitted",
      "marks": null,
      "feedback": null,
      "submittedAt": "2026-08-10T14:32:00Z"
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
