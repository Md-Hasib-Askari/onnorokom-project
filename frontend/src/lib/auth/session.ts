import "server-only";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import type { AuthResponse } from "@/lib/api/schemas/auth.schema";
import type { UserRole } from "@/lib/api/schemas/common.schema";
import { ROUTES } from "@/lib/routes";
import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_TOKEN_COOKIE,
  SESSION_COOKIE,
  authCookieOptions,
  roleHome,
} from "./constants";
import { parseSessionCookie, type SessionUser } from "./session-schema";

export async function getSession(): Promise<SessionUser | null> {
  const store = await cookies();
  return parseSessionCookie(store.get(SESSION_COOKIE)?.value);
}

export async function getAccessToken(): Promise<string | null> {
  const store = await cookies();
  return store.get(ACCESS_TOKEN_COOKIE)?.value ?? null;
}

export async function getRefreshToken(): Promise<string | null> {
  const store = await cookies();
  return store.get(REFRESH_TOKEN_COOKIE)?.value ?? null;
}

function toSessionUser(auth: AuthResponse): SessionUser {
  return {
    userId: auth.userId,
    fullName: auth.fullName,
    email: auth.email,
    role: auth.role,
    accessTokenExpiresAt: auth.accessTokenExpiresAt,
  };
}

/** Only callable from a Server Action or Route Handler. */
export async function applyAuthResponse(auth: AuthResponse): Promise<SessionUser> {
  const store = await cookies();
  const session = toSessionUser(auth);
  store.set(SESSION_COOKIE, JSON.stringify(session), authCookieOptions);
  store.set(ACCESS_TOKEN_COOKIE, auth.accessToken, authCookieOptions);
  store.set(REFRESH_TOKEN_COOKIE, auth.refreshToken, authCookieOptions);
  return session;
}

/** Only callable from a Server Action or Route Handler. */
export async function clearSession(): Promise<void> {
  const store = await cookies();
  store.delete(SESSION_COOKIE);
  store.delete(ACCESS_TOKEN_COOKIE);
  store.delete(REFRESH_TOKEN_COOKIE);
}

/**
 * Defense in depth for Server Components/Actions: `proxy.ts` already guards
 * `/admin/*`, but Next.js recommends re-checking inside each Server Function too,
 * since a matcher change could silently drop Proxy coverage for a route.
 */
export async function requireSession(): Promise<SessionUser> {
  const session = await getSession();
  if (!session) redirect(ROUTES.login);
  return session;
}

export async function requireRole(role: UserRole): Promise<SessionUser> {
  const session = await requireSession();
  if (session.role !== role) redirect(roleHome(session.role));
  return session;
}
