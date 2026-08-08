/**
 * Every in-app path in one place, so renaming a route is a single edit.
 *
 * Note: `proxy.ts` still spells its `config.matcher` out as literals, because
 * Next.js statically analyses that array at build time and cannot follow imports.
 */
export const ROUTES = {
  home: "/",
  login: "/login",
  register: "/register",
  pendingApproval: "/pending-approval",
  dashboard: "/dashboard",
  admin: "/admin",
  adminUsers: "/admin/users",
  adminGrades: "/admin/grades",
  adminSubjects: "/admin/subjects",
  adminAssignments: "/admin/assignments",
} as const;

export type AppRoute = (typeof ROUTES)[keyof typeof ROUTES];

/** Query-string key carrying the path to return to after a forced sign-in. */
export const NEXT_PATH_PARAM = "next";

/**
 * Returns `path` only when it is a safe in-app path, otherwise `undefined`.
 *
 * Rejects protocol-relative (`//host`), backslash-prefixed (`/\host`), and any
 * other value containing a backslash, all of which browsers can resolve as a
 * cross-origin location. Never trust a `next` query param without this check.
 */
export function sanitizeNextPath(path: string | null | undefined): string | undefined {
  if (!path || !path.startsWith(ROUTES.home)) return undefined;
  if (path.startsWith("//") || path.includes("\\")) return undefined;
  return path;
}
