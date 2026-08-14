# 00 · Teacher Workspace

> All endpoints in this module require an authenticated `Teacher` token (`Authorization: Bearer <accessToken>`). Every assignment and submission query is scoped to the signed-in teacher. A teacher can create assignments only for section-subject links assigned to them by an admin.

<details>
<summary><b>GET → My Section Subjects</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/teacher/section-subjects` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns the section-subject pairings assigned to the signed-in teacher. These are the only targets available when creating an assignment.

## Successful Response `200 OK`

```json
{
  "items": [
    {
      "sectionId": "f7a1...",
      "sectionName": "Section A",
      "gradeId": "d3b4...",
      "gradeName": "Grade 1",
      "subjectId": "a1b2...",
      "subjectName": "Mathematics",
      "subjectCode": null
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
| `401` | (no body) | Not authenticated |
| `403` | (no body) | Authenticated but not a `Teacher` |

</details>

---

<details>
<summary><b>GET → My Students</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/teacher/students` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns the students enrolled in the sections the signed-in teacher teaches, as a cursor-paginated page ordered by `(SectionName, FullName, Id)` ascending (by section, then alphabetically by student). A student who takes two subjects from the same teacher still appears once, since this describes class membership, not a subject link.

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
      "id": "9a8b...",
      "fullName": "John Smith",
      "rollNumber": "G1-001",
      "sectionId": "f7a1...",
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
| `401` | (no body) | Not authenticated |
| `403` | (no body) | Authenticated but not a `Teacher` |

</details>

---

<details>
<summary><b>GET → List My Assignments</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/teacher/assignments` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns the signed-in teacher's non-deleted assignments with the target section and subject plus total and graded submission counts, as a cursor-paginated page ordered by `(CreatedAt, Id)` descending (newest first). The submission counts are scoped to the rows on the current page, so they stay correct while paging.

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
      "title": "Geometry Basics",
      "description": "Identify the angle types.",
      "sectionId": "f7a1...",
      "sectionName": "Section A",
      "gradeName": "Grade 1",
      "subjectId": "a1b2...",
      "subjectName": "Mathematics",
      "deadline": "2026-08-18T23:59:00Z",
      "maxMarks": 20,
      "status": "Published",
      "allowLateSubmission": false,
      "submissionsOpen": false,
      "submissionCount": 0,
      "gradedCount": 0
    }
  ],
  "nextCursor": "eyJrIjpb...",
  "hasMore": false
}
```

`nextCursor` is `null` when the last page has been reached. `hasMore` reports whether further pages exist without decoding the cursor. `submissionsOpen` is true while the deadline is in the future, or late submissions are allowed.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Invalid pagination cursor." }` | Malformed, tampered, or non-matching cursor |
| `401` | (no body) | Not authenticated |
| `403` | (no body) | Authenticated but not a `Teacher` |

</details>

---

<details>
<summary><b>GET → Get My Assignment</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/teacher/assignments/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns one assignment authored by the signed-in teacher, with the same shape as the assignment list item and current submission counts.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | Assignment ID |

## Successful Response `200 OK`

Returns one `TeacherAssignmentDto`, as shown in [List My Assignments](#get--list-my-assignments).

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Assignment exists but was authored by another teacher |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment does not exist |

</details>

---

<details>
<summary><b>POST → Create Assignment</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/teacher/assignments` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Creates a draft assignment for a section-subject pairing owned by the signed-in teacher. The section, subject, and author are fixed at creation. A newly created assignment is not visible to students until it is published.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `title` | string | Yes | Assignment title, maximum 200 characters |
| `description` | string | No | Description, maximum 2,000 characters |
| `sectionId` | string (guid) | Yes | Target section from the teacher's assigned section-subjects |
| `subjectId` | string (guid) | Yes | Target subject from the same assigned pairing |
| `deadline` | string (ISO-8601) | Yes | Must be in the future when creating |
| `maxMarks` | number | Yes | Must be greater than zero; fractional marks are supported |
| `allowLateSubmission` | boolean | Yes | Whether submissions remain open after the deadline |

## Example Request Body

```json
{
  "title": "Geometry Basics",
  "description": "Identify the angle types.",
  "sectionId": "f7a1...",
  "subjectId": "a1b2...",
  "deadline": "2026-08-18T23:59:00Z",
  "maxMarks": 20,
  "allowLateSubmission": false
}
```

## Successful Response `200 OK`

Returns the created draft as a `TeacherAssignmentDto`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "title": "..." } }` | Request validation failed |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "You are not assigned to teach this subject in this section." }` | Pairing is not assigned to the caller |

</details>

---

<details>
<summary><b>PUT → Update Assignment</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/teacher/assignments/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Updates an assignment authored by the caller. The target section and subject cannot be changed. Updates may correct a past deadline, and maximum marks cannot be lowered below marks already awarded.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `title` | string | Yes | Assignment title, maximum 200 characters |
| `description` | string | No | Description, maximum 2,000 characters |
| `deadline` | string (ISO-8601) | Yes | New deadline; past values are accepted for corrections or early closure |
| `maxMarks` | number | Yes | Must be greater than zero and not below awarded marks |
| `allowLateSubmission` | boolean | Yes | Whether late submissions are accepted |

## Successful Response `200 OK`

Returns the updated `TeacherAssignmentDto`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "title": "..." } }` | Request validation failed |
| `400` | `{ "error": "Maximum marks cannot be lowered to ... because a submission has already been awarded ..." }` | New maximum is below an awarded mark |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Assignment belongs to another teacher |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment does not exist |

</details>

---

<details>
<summary><b>POST → Publish Assignment</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/teacher/assignments/:id/publish` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Publishes a draft assignment. Publishing is one-way and makes the assignment visible to students in its section.

## Successful Response `200 OK`

Returns the published `TeacherAssignmentDto`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "This assignment is already published." }` | Assignment has already been published |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Assignment belongs to another teacher |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment does not exist |

</details>

---

<details>
<summary><b>DELETE → Delete Assignment</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `DELETE` |
| 🔗 URL | `/api/teacher/assignments/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Soft-deletes an assignment authored by the caller. An assignment cannot be deleted after students have submitted to it.

## Successful Response `204 No Content`

(no body)

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Assignment belongs to another teacher |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment does not exist |
| `409` | `{ "error": "This assignment cannot be deleted because students have already submitted to it." }` | Assignment has submissions |

</details>

---

<details>
<summary><b>GET → List Assignment Submissions</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/teacher/assignments/:id/submissions` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns all submissions for an assignment owned by the caller, as a cursor-paginated page ordered by `(FullName, Id)` ascending (alphabetical by student, then by submission). Each item includes the student, optional roll number, answer content, attachment link, workflow status, derived lateness, marks, feedback, and timestamps.

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
      "studentId": "9a8b...",
      "studentName": "John Smith",
      "rollNumber": "G1-001",
      "content": "My answer...",
      "attachmentUrl": null,
      "status": "Submitted",
      "isLate": false,
      "marks": null,
      "feedback": null,
      "submittedAt": "2026-08-10T14:32:00Z",
      "gradedAt": null
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
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Assignment belongs to another teacher |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment does not exist |

</details>

---

<details>
<summary><b>PUT → Grade Submission</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/teacher/submissions/:id/grade` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Grades a submission belonging to one of the caller's assignments. Marks may be zero, cannot be negative, and cannot exceed the parent assignment's maximum. Grading changes the status to `Graded` and records the grading teacher and timestamp.

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `marks` | number | Yes | Zero or greater, and no greater than the assignment's `maxMarks` |
| `feedback` | string | No | Teacher feedback, maximum 2,000 characters |

## Example Request Body

```json
{
  "marks": 17,
  "feedback": "Solid work overall. Review factorisation in question 6."
}
```

## Successful Response `200 OK`

Returns the updated `TeacherSubmissionDto`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "marks": "..." } }` | Marks or feedback validation failed |
| `400` | `{ "error": "Marks cannot exceed the assignment maximum of ..." }` | Marks exceed the assignment maximum |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Parent assignment belongs to another teacher |
| `404` | `{ "error": "Submission with id <id> was not found." }` | Submission does not exist |

</details>

---

<details>
<summary><b>POST → Return Submission for Revision</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/teacher/submissions/:id/return` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns a graded submission for revision. The existing marks and feedback are cleared, and the status becomes `Returned`, allowing the student to submit a revision while the assignment accepts submissions.

## Successful Response `200 OK`

Returns the updated `TeacherSubmissionDto`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Only a graded submission can be returned for revision." }` | Submission is not currently graded |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "This assignment belongs to another teacher." }` | Parent assignment belongs to another teacher |
| `404` | `{ "error": "Submission with id <id> was not found." }` | Submission does not exist |

</details>

---

<details>
<summary><b>GET → Overview Stats</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/teacher/stats/overview` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Teacher`) |
| 📁 Content-Type | `application/json` |

## Description

Returns the counts backing the teacher's overview page: assignments broken down by status, submissions awaiting grading across the teacher's published assignments, the number of students they teach, and a preview of their most recently set assignments. Counts cover the full data set, not just the paginated pages a client has loaded so far.

## Successful Response `200 OK`

```json
{
  "assignments": 3,
  "drafts": 1,
  "published": 2,
  "awaitingGrading": 1,
  "students": 28,
  "recentAssignments": [
    {
      "id": "f6e7...",
      "title": "Geometry Basics",
      "sectionName": "Section A",
      "gradeName": "Grade 1",
      "subjectName": "Mathematics",
      "deadline": "2026-08-18T23:59:00Z",
      "status": "Published",
      "submissionCount": 2,
      "gradedCount": 1
    }
  ]
}
```

`status` is `Draft` or `Published`. `awaitingGrading` counts non-graded submissions on the teacher's published assignments.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated |
| `403` | (no body) | Authenticated but not a `Teacher` |
</details>
