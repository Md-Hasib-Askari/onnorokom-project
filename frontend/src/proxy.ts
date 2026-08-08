import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { AuthApi } from "@/lib/api/auth.api";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { NEXT_PATH_PARAM, ROUTES } from "@/lib/routes";
import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_THRESHOLD_MS,
  REFRESH_TOKEN_COOKIE,
  SESSION_COOKIE,
  authCookieOptions,
  roleHome,
} from "@/lib/auth/constants";
import { parseSessionCookie, type SessionUser } from "@/lib/auth/session-schema";

const PROTECTED_PREFIXES: readonly string[] = [ROUTES.admin, ROUTES.dashboard];
const GUEST_ONLY_PATHS: readonly string[] = [ROUTES.login, ROUTES.register];

function isExpiringSoon(iso: string): boolean {
  const expiresAt = new Date(iso).getTime();
  if (Number.isNaN(expiresAt)) return true;
  return expiresAt - Date.now() < REFRESH_THRESHOLD_MS;
}

function isProtectedPath(pathname: string): boolean {
  return PROTECTED_PREFIXES.some((prefix) => pathname === prefix || pathname.startsWith(`${prefix}/`));
}

function clearAuthCookies(response: NextResponse) {
  response.cookies.delete(SESSION_COOKIE);
  response.cookies.delete(ACCESS_TOKEN_COOKIE);
  response.cookies.delete(REFRESH_TOKEN_COOKIE);
}

function applyRefreshedCookies(
  response: NextResponse,
  refreshed: { session: SessionUser; accessToken: string; refreshToken: string } | null,
): NextResponse {
  if (!refreshed) return response;
  response.cookies.set(SESSION_COOKIE, JSON.stringify(refreshed.session), authCookieOptions);
  response.cookies.set(ACCESS_TOKEN_COOKIE, refreshed.accessToken, authCookieOptions);
  response.cookies.set(REFRESH_TOKEN_COOKIE, refreshed.refreshToken, authCookieOptions);
  return response;
}

/** Applies a successful refresh, or clears cookies left over from one that failed or couldn't run. */
function finalizeAuthCookies(
  response: NextResponse,
  refreshed: { session: SessionUser; accessToken: string; refreshToken: string } | null,
  sessionInvalidated: boolean,
): NextResponse {
  if (refreshed) return applyRefreshedCookies(response, refreshed);
  if (sessionInvalidated) clearAuthCookies(response);
  return response;
}

export async function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const isProtected = isProtectedPath(pathname);
  const isGuestOnly = GUEST_ONLY_PATHS.includes(pathname);

  if (!isProtected && !isGuestOnly && pathname !== ROUTES.home) {
    return NextResponse.next();
  }

  let session = parseSessionCookie(request.cookies.get(SESSION_COOKIE)?.value);
  const hadAuthCookies =
    request.cookies.has(SESSION_COOKIE) ||
    request.cookies.has(ACCESS_TOKEN_COOKIE) ||
    request.cookies.has(REFRESH_TOKEN_COOKIE);
  let refreshed: { session: SessionUser; accessToken: string; refreshToken: string } | null = null;

  if (session && isExpiringSoon(session.accessTokenExpiresAt)) {
    const refreshToken = request.cookies.get(REFRESH_TOKEN_COOKIE)?.value;
    if (!refreshToken) {
      session = null;
    } else {
      try {
        const auth = await AuthApi.refresh(refreshToken);
        const nextSession: SessionUser = {
          userId: auth.userId,
          fullName: auth.fullName,
          email: auth.email,
          role: auth.role,
          accessTokenExpiresAt: auth.accessTokenExpiresAt,
        };
        refreshed = { session: nextSession, accessToken: auth.accessToken, refreshToken: auth.refreshToken };
        session = nextSession;
      } catch {
        session = null;
      }
    }
  }

  const sessionInvalidated = hadAuthCookies && session === null;

  if (pathname === ROUTES.home) {
    const destination = session ? roleHome(session.role) : ROUTES.login;
    return finalizeAuthCookies(NextResponse.redirect(new URL(destination, request.url)), refreshed, sessionInvalidated);
  }

  if (isProtected) {
    if (!session) {
      const url = new URL(ROUTES.login, request.url);
      url.searchParams.set(NEXT_PATH_PARAM, pathname);
      return finalizeAuthCookies(NextResponse.redirect(url), refreshed, sessionInvalidated);
    }
    if (pathname.startsWith(ROUTES.admin) && session.role !== UserRole.Admin) {
      return finalizeAuthCookies(NextResponse.redirect(new URL(roleHome(session.role), request.url)), refreshed, sessionInvalidated);
    }
  }

  if (isGuestOnly && session) {
    return finalizeAuthCookies(NextResponse.redirect(new URL(roleHome(session.role), request.url)), refreshed, sessionInvalidated);
  }

  if (!refreshed && !sessionInvalidated) return NextResponse.next();

  if (refreshed) {
    request.cookies.set(SESSION_COOKIE, JSON.stringify(refreshed.session));
    request.cookies.set(ACCESS_TOKEN_COOKIE, refreshed.accessToken);
    request.cookies.set(REFRESH_TOKEN_COOKIE, refreshed.refreshToken);
  } else {
    request.cookies.delete(SESSION_COOKIE);
    request.cookies.delete(ACCESS_TOKEN_COOKIE);
    request.cookies.delete(REFRESH_TOKEN_COOKIE);
  }

  const response = NextResponse.next({ request });
  return finalizeAuthCookies(response, refreshed, sessionInvalidated);
}

/**
 * Next.js statically analyses `matcher` at build time and cannot follow imports,
 * so these paths must stay literal. Keep them in sync with `ROUTES`.
 */
export const config = {
  matcher: ["/", "/admin/:path*", "/dashboard/:path*", "/login", "/register"],
};
