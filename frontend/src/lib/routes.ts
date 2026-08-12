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
  forgotPassword: "/forgot-password",
  resetPassword: "/reset-password",
  changePassword: "/change-password",
  pendingApproval: "/pending-approval",
  dashboard: "/dashboard",
  profile: "/profile",
  admin: "/admin",
  adminUsers: "/admin/users",
  adminGrades: "/admin/grades",
  adminSections: "/admin/sections",
  adminSubjects: "/admin/subjects",
  adminAssignments: "/admin/assignments",
  adminSubmissions: "/admin/submissions",
  adminSettings: "/admin/settings",
  teacher: "/teacher",
  teacherSubjects: "/teacher/subjects",
  teacherAssignments: "/teacher/assignments",
  student: "/student",
  studentAssignments: "/student/assignments",
} as const;

export type AppRoute = (typeof ROUTES)[keyof typeof ROUTES];

/**
 * Paths that need a runtime value. Kept out of `ROUTES` because adding functions there
 * would poison the `AppRoute` union.
 */
export const ROUTE_BUILDERS = {
  teacherAssignment: (id: string) => `${ROUTES.teacherAssignments}/${id}`,
  studentAssignment: (id: string) => `${ROUTES.studentAssignments}/${id}`,
} as const;

/**
 * Query-string keys the teacher assignments page reads to preselect the target of a new
 * assignment, so "My subjects" can deep-link straight into a prefilled create dialog.
 */
export const ASSIGNMENT_TARGET_PARAMS = {
  sectionId: "sectionId",
  subjectId: "subjectId",
} as const;

/** Query-string key carrying the path to return to after a forced sign-in. */
export const NEXT_PATH_PARAM = "next";

/**
 * Returns `path` only when it is a safe in-app path, otherwise `undefined`.
 *
 * Resolves `path` against a dummy origin and rejects it unless the resolved
 * origin is unchanged. A prefix check alone (blocking `//`, `\`) is not
 * enough: WHATWG URL parsing strips control characters like tabs before
 * resolving, so `/\t/evil.com` slips past a prefix check but still resolves
 * cross-origin in the browser. Never trust a `next` query param without this
 * check.
 */
export function sanitizeNextPath(path: string | null | undefined): string | undefined {
  if (!path) return undefined;
  const DUMMY_ORIGIN = "http://sanitize.invalid";
  let url: URL;
  try {
    url = new URL(path, DUMMY_ORIGIN);
  } catch {
    return undefined;
  }
  if (url.origin !== DUMMY_ORIGIN) return undefined;
  const resolved = `${url.pathname}${url.search}${url.hash}`;
  if (!resolved.startsWith(ROUTES.home)) return undefined;
  return resolved;
}
