import { UserRole } from "@/lib/api/schemas/common.schema";
import { ROUTES } from "@/lib/routes";

export const SESSION_COOKIE = "session";
export const ACCESS_TOKEN_COOKIE = "access_token";
export const REFRESH_TOKEN_COOKIE = "refresh_token";

/** Matches backend RefreshTokenExpirationDays (appsettings.json). */
const SECONDS_PER_MINUTE = 60;
const MINUTES_PER_HOUR = 60;
const HOURS_PER_DAY = 24;
const REFRESH_TOKEN_EXPIRATION_DAYS = 7;
export const REFRESH_TOKEN_MAX_AGE_SECONDS =
  SECONDS_PER_MINUTE * MINUTES_PER_HOUR * HOURS_PER_DAY * REFRESH_TOKEN_EXPIRATION_DAYS;

/** Proactively refresh the access token once this little time is left on it. */
export const REFRESH_THRESHOLD_MS = 60_000;

const PRODUCTION_NODE_ENV = "production";

/** Shared by `proxy.ts` (NextResponse cookies) and `session.ts` (the cookie store). */
export const authCookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === PRODUCTION_NODE_ENV,
  sameSite: "lax",
  path: ROUTES.home,
  maxAge: REFRESH_TOKEN_MAX_AGE_SECONDS,
} as const;

const ROLE_HOMES: Record<UserRole, string> = {
  [UserRole.Admin]: ROUTES.adminUsers,
  [UserRole.Teacher]: ROUTES.teacher,
  [UserRole.Student]: ROUTES.student,
};

/** Where a signed-in user lands when they have no specific destination. */
export function roleHome(role: UserRole) {
  return ROLE_HOMES[role];
}
