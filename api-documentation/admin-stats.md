# 00 · Admin Overview Stats

> All endpoints in this module require the caller to be authenticated as an `Admin` (`Authorization: Bearer <accessToken>`). These are read-only counts backing the admin overview page.

<details>
<summary><b>GET → Overview Stats</b></summary>

| Field | Value |
| --- | --- |
| 🟢 Method | `GET` |
| 🔗 URL | `/api/admin/stats/overview` |
| ✅ Status | Completed |
| 📦 Auth | Required (role: `Admin`) |
| 📁 Content-Type | `application/json` |

## Description

Returns the counts backing the admin overview page: users broken down by role and pending status, the academic structure (grades, sections, subjects), assignment activity (total, drafts, published), submission progress (total, graded, ungraded), and a preview of the most recently registered users still waiting on a decision. Counts cover the full data set, not just the paginated pages a client has loaded so far.

## Successful Response `200 OK`

```json
{
  "students": 120,
  "teachers": 18,
  "admins": 2,
  "pending": 3,
  "grades": 12,
  "sections": 24,
  "subjects": 15,
  "assignments": 40,
  "drafts": 5,
  "published": 35,
  "submissions": 180,
  "graded": 150,
  "ungraded": 30,
  "recentPending": [
    {
      "id": "d3b4...",
      "fullName": "Jane Doe",
      "role": "Student",
      "createdAt": "2026-08-12T09:00:00Z"
    }
  ]
}
```

`role` is `Admin`, `Teacher`, or `Student`. `pending` counts users whose account status is `Pending`; the `recentPending` preview lists the newest ones, ordered newest first.

## Error Responses

| Status | Body | Reason |
| --- | --- | --- |
| `401` | (no body) | Not authenticated (missing/invalid access token) |
| `403` | (no body) | Authenticated but not an `Admin` |
</details>
