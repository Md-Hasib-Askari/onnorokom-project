"use server";

import { revalidatePath } from "next/cache";
import { AdminSettingsApi } from "@/lib/api/admin-settings.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  systemSettingsUpdateRequestSchema,
  type SystemSettings,
  type SystemSettingsUpdateRequest,
} from "@/lib/api/schemas/settings.schema";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES } from "@/lib/routes";
import { getAccessToken, requireRole } from "@/lib/auth/session";
import type { ActionResult } from "./auth.actions";

async function accessTokenOrThrow(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new ApiError(HttpStatus.Unauthorized, ERROR_MESSAGES.sessionExpired);
  return token;
}

export async function getSystemSettingsAction(): Promise<SystemSettings> {
  await requireRole(UserRole.Admin);
  const token = await accessTokenOrThrow();
  return AdminSettingsApi.getSystemSettings(token);
}

export async function updateSystemSettingsAction(
  input: SystemSettingsUpdateRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = systemSettingsUpdateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSettingsApi.updateSystemSettings(token, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSettings);
  return { success: true };
}
