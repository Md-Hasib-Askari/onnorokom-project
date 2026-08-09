"use server";

import { redirect } from "next/navigation";
import { AdminAssignmentsApi } from "@/lib/api/admin-assignments.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES } from "@/lib/routes";
import { getAccessToken, requireRole } from "@/lib/auth/session";

async function accessTokenOrThrow(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new ApiError(HttpStatus.Unauthorized, ERROR_MESSAGES.sessionExpired);
  return token;
}

/** Redirects to login on an expired/missing session instead of surfacing a generic load error. */
function redirectOnSessionExpired(error: unknown): never {
  if (error instanceof ApiError && error.status === HttpStatus.Unauthorized) {
    redirect(ROUTES.login);
  }
  throw error;
}

export async function listAssignmentsAction() {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    return await AdminAssignmentsApi.list(token);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function listSubmissionsAction() {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    return await AdminAssignmentsApi.listSubmissions(token);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}