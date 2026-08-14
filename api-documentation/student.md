# 00 · Student Workspace

> All endpoints in this module require an authenticated `Student` token (`Authorization: Bearer <accessToken>`). Assignment lists and details are scoped to the student's enrolled section and include published assignments only. A student without a provisioned section profile receives a `403` until an admin enrols them.

<details>
<summary><b>GET → List My Assignments</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/student/assignments` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Student`) |
| 📁 Content-Type | `application/json` |

## Description

Returns published assignments for the signed-in student's section, as a cursor-paginated page ordered by `(CreatedAt, Id)` descending (newest first). Each item includes the deadline, maximum marks, late-submission policy, and the student's current submission state. `submissionStatus` and `marks` are `null` when the student has not submitted.

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
      "subjectName": "Mathematics",
      "teacherName": "Demo Teacher",
      "deadline": "2026-08-18T23:59:00Z",
      "maxMarks": 20,
      "allowLateSubmission": false,
      "isPastDeadline": false,
      "submissionsOpen": false,
      "submissionStatus": null,
      "isLate": false,
      "marks": null
    }
  ],
  "nextCursor": "eyJrIjpb...",
  "hasMore": false
}
```

`nextCursor` is `null` when the last page has been reached. `hasMore` reports whether further pages exist without decoding the cursor. `submissionStatus` is one of `Submitted`, `Resubmitted`, `Returned`, or `Graded` when a submission exists. `isLate` is derived from the submission timestamp and assignment deadline.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Invalid pagination cursor." }` | Malformed, tampered, or non-matching cursor |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "Your student profile is not set up yet. Ask an admin to place you in a section." }` | Student is not enrolled |

</details>

---

<details>
<summary><b>GET → Get Assignment Detail</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/student/assignments/:id` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Student`) |
| 📁 Content-Type | `application/json` |

## Description

Returns a published assignment in the student's section and the student's current submission, including content, attachment, feedback, marks, and server-computed `canSubmit` and `canEdit` flags. Drafts and assignments from other sections are reported as not found.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | Assignment ID |

## Successful Response `200 OK`

```json
{
  "id": "f6e7...",
  "title": "Geometry Basics",
  "subjectName": "Mathematics",
  "teacherName": "Demo Teacher",
  "deadline": "2026-08-18T23:59:00Z",
  "maxMarks": 20,
  "allowLateSubmission": false,
  "isPastDeadline": false,
  "submissionsOpen": false,
  "submissionStatus": null,
  "isLate": false,
  "marks": null,
  "description": "Identify the angle types.",
  "feedback": null,
  "attachmentUrl": null,
  "content": null,
  "submittedAt": null,
  "canSubmit": true,
  "canEdit": false
}
```

`canSubmit` is true only for an open assignment with no submission. `canEdit` is true only for an open assignment with an existing non-graded submission.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated |
| `403` | `{ "error": "Your student profile is not set up yet. Ask an admin to place you in a section." }` | Student is not enrolled |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment is missing, a draft, or belongs to another section |

</details>

---

<details>
<summary><b>POST → Submit Assignment</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/student/assignments/:id/submission` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Student`) |
| 📁 Content-Type | `application/json` |

## Description

Creates the student's first submission for a published assignment in their section. If a current submission already exists, the same endpoint revises that row, although the frontend uses the `PUT` endpoint for that case. The assignment must be before its deadline, or have late submissions enabled.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | Assignment ID |

## Request Body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `content` | string | Conditional | Text answer, maximum 10,000 characters |
| `attachmentUrl` | string | Conditional | Absolute `http` or `https` URL, maximum 2,000 characters |

At least one of `content` or `attachmentUrl` must contain a non-whitespace value.

## Example Request Body

```json
{
  "content": "My answers are attached and explained below.",
  "attachmentUrl": "https://files.example.com/work.pdf"
}
```

## Successful Response `200 OK`

Returns the updated `StudentAssignmentDetailDto` with `submissionStatus` set to `Submitted` or `Resubmitted` as applicable.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "content": "Enter your work or attach a link." } }` | Empty payload, invalid link, or field too long |
| `400` | `{ "error": "The deadline for this assignment has passed and late submissions are not allowed." }` | Assignment is closed for submissions |
| `401` | (no body) | Not authenticated |
| `403` | (no body) | Authenticated but not a `Student` |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment is missing, a draft, or belongs to another section |

</details>

---

<details>
<summary><b>PUT → Update Submission</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `PUT` |
| 🔗 URL | `/api/student/assignments/:id/submission` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Student`) |
| 📁 Content-Type | `application/json` |

## Description

Replaces the student's existing submission while the assignment accepts submissions. The row remains tied to the same assignment and student, status becomes `Resubmitted`, and `submittedAt` is refreshed. A graded submission must first be returned by the teacher.

## Path Params

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string (guid) | Yes | Assignment ID |

## Request Body

Uses the same `content` and `attachmentUrl` payload as [Submit Assignment](#post--submit-assignment). At least one field must be non-empty.

## Successful Response `200 OK`

Returns the updated `StudentAssignmentDetailDto` with `submissionStatus` set to `Resubmitted`.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "content": "Enter your work or attach a link." } }` | Empty payload, invalid link, or field too long |
| `400` | `{ "error": "The deadline for this assignment has passed and late submissions are not allowed." }` | Assignment is closed for submissions |
| `400` | `{ "error": "This submission has been graded and can no longer be changed. Ask your teacher to return it for revision." }` | Current submission is graded |
| `401` | (no body) | Not authenticated |
| `403` | (no body) | Authenticated but not a `Student` |
| `404` | `{ "error": "You have not submitted to this assignment yet." }` | No existing submission for the assignment |
| `404` | `{ "error": "Assignment with id <id> was not found." }` | Assignment is missing, a draft, or belongs to another section |
</details>
