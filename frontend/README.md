# Frontend

Next.js frontend for the Assignment Submission Portal, a role-based assignment
workflow for schools (admins, teachers, students). The full project setup,
demo credentials, and architecture live in [`../docs/README.md`](../docs/README.md);
this file covers the frontend only.

## Stack

- Next.js 16 (App Router), React 19, TypeScript
- Tailwind CSS v4 with shadcn/ui components (Radix primitives)
- TanStack Query for server state, TanStack Table for data tables
- react-hook-form + zod for forms, sonner for toasts
- axios for the HTTP client, `server-only` to keep secrets out of the browser
- Package manager is **bun** (never npm/yarn)

## Getting started

```bash
bun install
cp .env.example .env.local   # API_BASE_URL, see below
bun dev
```

Serves `http://localhost:3000`. The API must be running on `API_BASE_URL`
(default `http://localhost:5128`, matching the backend launch profile).

`API_BASE_URL` is read **server-side only** (in `lib/api/client.ts` and the
middleware) and never reaches the browser.

## Environment

| Variable | Default | Purpose |
| --- | --- | --- |
| `API_BASE_URL` | `http://localhost:5128` | Base URL of the ASP.NET Core API |

## Routes

| Path | Audience | Purpose |
| --- | --- | --- |
| `/` | guests and signed-in users | Redirects to the signed-in user's workspace, or `/login` |
| `/login`, `/register`, `/pending-approval` | guests only | Auth flow; `/pending-approval` explains self-registered accounts |
| `/dashboard` | any signed-in user | Forwards to the user's own workspace |
| `/admin/users`, `/admin/grades`, `/admin/sections`, `/admin/subjects`, `/admin/assignments`, `/admin/submissions`, `/admin/settings` | Admin | User and academic administration, read-only assignment/submission views, system settings |
| `/teacher`, `/teacher/subjects`, `/teacher/assignments`, `/teacher/assignments/[id]` | Teacher | Overview, section-subject slots, assignment management, submissions and grading |
| `/student`, `/student/assignments`, `/student/assignments/[id]` | Student | Overview, published assignments, submission and feedback |

All paths are centralized in `src/lib/routes.ts` (`ROUTES` plus
`ROUTE_BUILDERS` for the parameterized detail pages), so a rename is one edit.
The middleware's `config.matcher` in `src/proxy.ts` is an intentional
exception: Next.js statically analyses it and cannot follow imports.

## Structure

```
src/
  app/                    App Router routes: (auth), admin/*, teacher/*, student/*
  components/
    workspace/            shared workspace shell, sidebar, data table
    admin/                user, grade, section, subject, assignment, submission, settings views
    teacher/              assignment and submission management components
    student/              assignment list/detail and submission form
    ui/                   shadcn/ui primitives
    auth/                 login and register forms
  lib/
    api/                  axios clients, zod response schemas, http status helpers
    queries/              TanStack Query hooks, one file per module
    mutations/            TanStack Query mutation hooks, one file per module
    actions/              server actions (forms call these)
    auth/                 session parsing and cookie names/options
    routes.ts             every in-app path
    messages.ts           user-facing strings
  proxy.ts                Next.js middleware: route guards and silent token refresh
```

### Middleware (`src/proxy.ts`)

Guards every protected and guest-only path:

- Redirects `/` to the user's role home or `/login`.
- Blocks guests from `/admin`, `/teacher`, `/student`, `/dashboard`; sends
  them to `/login?next=<original path>` so they return after signing in.
- Blocks signed-in users from `/login` and `/register`.
- Restricts role-only prefixes to the matching role (`Admin` for `/admin`,
  etc.) and sends the wrong role back to their own workspace.
- Silently refreshes the access token before it expires, reusing the rotating
  refresh token; clears stale auth cookies when a refresh fails.

## Checks

```bash
bun run lint       # eslint
bunx tsc --noEmit  # typecheck
bun run build      # production build
```

## Manual testing

End-to-end walkthroughs for every role live in
[`../docs/testing/`](../docs/testing): admin grades/subjects/assignments, admin
sections, the teacher workspace, and the student workspace.

## Note

`frontend/AGENTS.md` carries a `BEGIN:nextjs-agent-rules` block that `next dev`
re-writes on every run. Leave it alone: removing it only makes the tree dirty,
and this version of Next.js differs from most training data, so read the docs
under `node_modules/next/dist/docs/` before writing frontend code.
