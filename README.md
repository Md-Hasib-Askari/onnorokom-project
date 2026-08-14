<div align="center">

# Onnorokom Assignment & Submission Management System

A role-based assignment and submission management system for schools and
colleges. Teachers create assignments for their classes, students submit work
and get marks and feedback, and admins run the users, the academic structure,
and the application settings.

![.NET 10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=white) ![Next.js 16](https://img.shields.io/badge/Next.js%2016-000000?logo=next.js&logoColor=white) ![React 19](https://img.shields.io/badge/React%2019-61DAFB?logo=react&logoColor=black) ![TypeScript 5](https://img.shields.io/badge/TypeScript%205-3178C6?logo=typescript&logoColor=white) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14-4169E1?logo=postgresql&logoColor=white)

</div>

## Main features

**Authentication and accounts**

- Email/password login with JWT access tokens (15 min) and rotating refresh
  tokens (7 days) with a short grace window so a request that races a rotation
  is not logged out. Auth endpoints are rate limited per client IP.
- Self-registered users start `Pending` and cannot sign in until an admin
  approves them; admin-created users are approved immediately.
- Password reset via emailed codes, temporary passwords for new accounts, and
  a forced change on first sign-in. Passwords are hashed with BCrypt.
- The last usable admin account cannot be rejected, deactivated, or deleted,
  so the system can never lock itself out.

**Admin workspace**

- Full user management: create, edit, approve/reject, activate/deactivate,
  delete, and reset passwords.
- Academic structure: grades (classes), sections within grades, and a subject
  catalog per grade, each fully manageable.
- Per-section teacher assignment: a subject is catalogued on a grade, and the
  teacher for that subject varies per section (`SectionSubject` join).
- Read-only views of every assignment and submission in the system.
- System settings (for example, which roles may self-register) without a
  redeploy.

**Teacher workspace**

- Create, edit, publish, unpublish, and delete assignments for the
  section-subject slots assigned to them, with title, description, deadline,
  and maximum marks.
- View student submissions, assign marks and feedback, and return graded work
  for revision. Submissions can be closed or reopened.

**Student workspace**

- See only published assignments for their own section, with the deadline and
  late-submission policy.
- Submit text and/or an attachment link, and revise ungraded work while
  submissions are open. A graded submission must first be returned by the
  teacher before it can be resubmitted.
- View submission status, marks, and teacher feedback.

## Technology stack

| Area | Technology |
| --- | --- |
| Backend | ASP.NET Core (net10.0), C# REST API, EF Core + Npgsql, JWT bearer auth, BCrypt, AutoMapper, FluentValidation, xunit |
| Frontend | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, TanStack Table, react-hook-form + zod, sonner, axios, bun |
| Database | PostgreSQL 14+, schema fully managed by EF Core migrations, demo data by idempotent SQL seed scripts |

## Project structure

```
backend/
  AssignmentSystem.slnx              new .slnx solution format
  .env.example                       environment variable reference for the API
  src/
    AssignmentSystem.Domain/         entities, enums, BaseEntity + audit interfaces
    AssignmentSystem.Application/    DTOs, services, interfaces, validators, AutoMapper
    AssignmentSystem.Infrastructure/ AppDbContext, repositories, migrations, JWT/email services
    AssignmentSystem.Api/            controllers, middleware, Program.cs
  tests/
    AssignmentSystem.Tests/          xunit unit tests (reference Application)
frontend/
  .env.example
  src/
    app/                             App Router routes: (auth), admin/*, teacher/*, student/*
    components/                      role workspaces, auth forms, ui primitives
    lib/                             api clients, zod schemas, queries, mutations, server actions
    proxy.ts                         route guards and silent access-token refresh (middleware)
database/
  seed/                              idempotent SQL seed scripts, applied in order with psql
docs/
  api/                               one markdown file per API module
  testing/                           manual end-to-end walkthroughs per role
  ASSUMPTIONS.md                     design decisions and spec gaps
  REQUIREMENTS.md                    the original spec
```

Layering runs Domain → Application → Infrastructure → Api. Domain entities use
private constructors, static factories, and behaviour methods, so invalid
states are not representable. Deletion is soft everywhere, and audit fields are
stamped automatically by `AppDbContext.SaveChanges`.

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 10.0 |
| PostgreSQL | 14 or newer, or Docker with Compose |
| Node.js + bun | 20+ / 1.x (`curl -fsSL https://bun.sh/install \| bash`) |

PostgreSQL is needed by the backend. The easiest way to get one is the bundled
`docker compose` service (see below); if you already have PostgreSQL running on
`localhost:5432`, you can skip it.

## Setup

### 1. Configure the backend

Backend configuration lives in `backend/src/AssignmentSystem.Api/appsettings.json`
and already ships with working local defaults. Every key can be overridden from
the environment (`__` is the section separator). The full list is in
[`backend/.env.example`](backend/.env.example).

The committed `Jwt:AccessTokenSecret` is a placeholder and is fine for local
development, but it **must** be replaced before any non-local use:

```bash
export Jwt__AccessTokenSecret="$(openssl rand -base64 48)"
```

### 2. Configure the frontend

```bash
cp frontend/.env.example frontend/.env.local
```

`API_BASE_URL` defaults to `http://localhost:5128`, which matches the backend's
launch profile. It is read server-side only and never reaches the browser.

### 3. Start the database

The repository bundles a `docker-compose.yml` that runs a PostgreSQL 16
container matching the connection string in `appsettings.json`
(`assignment_system` / `postgres` / `postgres` on port `5432`), plus an
optional pgAdmin web UI to inspect the data:

```bash
docker compose up -d db           # PostgreSQL only
docker compose up -d db pgadmin   # also start pgAdmin (http://localhost:5050)
```

- PostgreSQL: `localhost:5432`
- pgAdmin (optional): `http://localhost:5050`, login `admin@onnorokom.com` /
  `admin`; when adding a server, use the connection details above

Skip this step when you already have PostgreSQL running on `localhost:5432`.

## Database setup

The API does nothing to the database at startup: no migrations, no seeding.
All steps below are prepared by hand before the first run and are idempotent.
The database from step 3 above must be running.

### 1. Apply the EF Core migrations

Creates the `assignment_system` database if it is absent, then applies every
pending migration:

```bash
cd backend
dotnet tool install --global dotnet-ef        # once
dotnet ef database update \
  --project src/AssignmentSystem.Infrastructure \
  --startup-project src/AssignmentSystem.Api
```

### 2. Apply the seed scripts

Run the migration step above first so the schema exists, then apply the scripts
in `database/seed/` in order with psql. They are safe to re-run at any time:
each row is guarded on what already exists.

```bash
# Create the database once if it does not exist yet (harmless when it already does).
psql "postgres://postgres:postgres@localhost:5432/postgres" \
  -c "CREATE DATABASE assignment_system" 2>/dev/null || true

for f in database/seed/*.sql; do
  psql "postgres://postgres:postgres@localhost:5432/assignment_system" \
    -v ON_ERROR_STOP=1 --single-transaction -f "$f"
done
```

| Script | Seeds |
| --- | --- |
| `01_system_settings.sql` | Registration policy (teachers may self-register, students may not) |
| `02_admin.sql` | The admin account (only when no admin exists) |
| `03_grades_and_sections.sql` | `Grade 1`-`Grade 12` for the current academic year with their sections (only when the year has no grades) |
| `04_subjects.sql` | Five subjects per grade (Mathematics, English, Science, ICT, Bangla) |
| `05_users.sql` | Demo teacher and student, 9 bulk teachers, 129 bulk students, 2 pending accounts (only when those emails are free) |
| `06_section_subjects.sql` | Section-subject links for every section, with the demo teacher assigned to Mathematics in Grade 1 Section A |
| `07_assignments_and_submissions.sql` | 150 assignments and 163 submissions, including the demo worked example with a graded submission (only when the assignments table is empty) |

## Running the stack

Database (skip when already running on `localhost:5432`):

```bash
docker compose up -d db           # PostgreSQL only
docker compose up -d db pgadmin   # also start pgAdmin (http://localhost:5050)
```

Backend (serves `http://localhost:5128`):

```bash
cd backend
dotnet run --project src/AssignmentSystem.Api
```

In Development the Swagger UI is available at `http://localhost:5128/swagger`.
The handwritten endpoint docs in [`docs/api/README.md`](docs/api/README.md) are
the primary reference.

Frontend (serves `http://localhost:3000`):

```bash
cd frontend
bun install
bun dev
```

Sign in at `http://localhost:3000/login`.

## Demo credentials

All three accounts are seeded by the scripts in `database/seed/` and are
approved and active. The demo student is enrolled in Grade 1 Section A, and the
demo teacher teaches Mathematics there. A fresh database also contains
`Algebra Worksheet 1` with a graded submission and `Geometry Basics` with none.

| Role | Email | Password | Lands on |
| --- | --- | --- | --- |
| Admin | `admin@onnorokom.com` | `Admin@123` | `/admin` |
| Teacher | `teacher@onnorokom.com` | `Teacher@123` | `/teacher` |
| Student | `student@onnorokom.com` | `Student@123` | `/student` |

## Running the tests

Backend (xunit, Application layer with mocked repositories, no database needed):

```bash
cd backend
dotnet test AssignmentSystem.slnx

# a single test or class
dotnet test AssignmentSystem.slnx --filter "FullyQualifiedName~SectionServiceTests"
```

Frontend checks:

```bash
cd frontend
bun run lint          # eslint
bunx tsc --noEmit     # typecheck
bun run build         # production build
```

## API documentation

One markdown file per module in [`api-documentation/README.md`](api-documentation/README.md),
each listing request bodies, success responses, and the error payload for every
failure case: authentication, profile, admin users, grades, sections, subjects,
assignments/submissions queries, settings, overview stats, teacher workspace,
and student workspace.

All errors follow one contract produced by `ExceptionHandlingMiddleware`:

| Status | Body | Meaning |
| --- | --- | --- |
| `400` | `{ "error": "Validation failed.", "errors": { "field": "message" } }` | Request failed validation |
| `400` | `{ "error": "..." }` | Domain rule violated |
| `401` | (no body) | Missing or invalid access token |
| `403` | (no body) | Authenticated as the wrong role |
| `404` | `{ "error": "<Entity> with id <id> was not found." }` | Entity does not exist |
| `409` | `{ "error": "..." }` | Duplicate, or entity still in use |
| `429` | `{ "error": "Too many requests. Try again later." }` | Auth endpoint rate limit exceeded |

## Assumptions

The spec deliberately leaves several areas open. The most important ones:

- Entity naming: the spec speaks of "classes/courses" but never prescribes
  entity names. The identity entity is `AuthUser`, the spec's class/course is
  the `Grade` entity, and enrolment follows a Grade → Section → Student
  hierarchy (students enrol into a section, never directly into a grade).
- Teachers are assigned per section, not per subject: a subject is a grade
  catalog entry, and the `SectionSubject` join holds the teacher for that
  subject in a specific section.
- Self-registration is not a fixed rule: an admin decides per role whether
  teachers and students may register themselves. A self-registered student
  cannot pick a section at registration; the approving admin assigns one.
- Submission lateness is derived from `SubmittedAt` and the assignment
  deadline.
- Deletion is soft everywhere, and audit fields are stamped automatically by
  EF Core rather than in services.

## Known limitations

- **No file storage.** `Submission.AttachmentUrl` is a plain string. There is
  no upload pipeline or blob storage.
- **No in-app notifications.** Transactional email exists (password reset
  codes, temporary passwords), but there is no notification center or activity
  feed.
- **Seed data is not environment-gated.** The demo accounts and data are
  seeded in every environment whenever their guards allow.
- **Not deployed.** The project runs locally only; no hosting or CI pipeline is
  configured.

## License

[Apache License 2.0](LICENSE).
