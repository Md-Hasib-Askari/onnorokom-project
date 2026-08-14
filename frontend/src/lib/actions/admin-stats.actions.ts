"use server";

import { redirect } from "next/navigation";
import { AdminStatsApi } from "@/lib/api/admin-stats.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES } from "@/lib/routes";
import { getAccessToken, requireRole } from "@/lib/auth/session";

/** Redirects to login on an expired/missing session instead of surfacing a generic load error. */
function redirectOnSessionExpired(error: unknown): never {
  if (error instanceof ApiError && error.status === HttpStatus.Unauthorized) {
    redirect(ROUTES.login);
  }
  throw error;
}

export async function getAdminOverviewAction() {
  await requireRole(UserRole.Admin);
  try {
    const token = await getAccessToken();
    if (!token) throw new ApiError(HttpStatus.Unauthorized, ERROR_MESSAGES.sessionExpired);
    return await AdminStatsApi.getOverview(token);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}
