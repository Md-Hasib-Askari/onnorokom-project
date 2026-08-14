# API Documentation

Every endpoint of the Assignment System API is documented here, one markdown
file per feature module (authentication, admin users, teacher workspace, and
so on). All files in this folder follow the same conventions described below.

## Conventions

### Base URL and authentication

- All endpoints live under the `/api` prefix, for example
  `POST /api/auth/login`.
- Protected endpoints require a JWT access token sent as a header:

  ```
  Authorization: Bearer <accessToken>
  ```

### Endpoint blocks

Each endpoint is written as a collapsible `<details>` block. The summary line
always reads `POST → Name of the Endpoint`, and the block starts with a small
field table:

| Field | Value |
| --- | --- |
| 🟢 Method | `POST` |
| 🔗 URL | `/api/example` |
| ✅ Status | Completed |
| 📦 Auth | Required |
| 📁 Content-Type | `application/json` |

Then the block explains the endpoint in four short sections:

1. **Description** - what the endpoint does, in one or two sentences.
2. **Request Body** - a table of the accepted fields (`Field`, `Type`,
   `Required`, `Description`).
3. **Successful Response** - the exact JSON returned, with the status code in
   the heading (for example `200 OK`).
4. **Error Responses** - a table of the status codes the endpoint can fail
   with and why.

### Error responses

Every endpoint fails with the same error shapes, produced centrally by
`ExceptionHandlingMiddleware`:

| Status | Body | Meaning |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "field": "message" } }` | Validation failed; one message per field |
| `400` | `{ "error": "..." }` | A domain rule was violated |
| `401` | (no body) | Missing or invalid access token |
| `403` | (no body) | Wrong role for the endpoint |
| `404` | `{ "error": "..." }` | Resource not found |
| `409` | `{ "error": "..." }` | Duplicate resource or still in use |
| `429` | `{ "error": "Too many requests. Try again later." }` | Rate limit hit |
| `500` | `{ "error": "An unexpected error occurred." }` | Unhandled exception |

### Other rules

- All `/api/auth/*` endpoints are rate limited per client IP.
- Dates are ISO-8601 (`DateTimeOffset`).
