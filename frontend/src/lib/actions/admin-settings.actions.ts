"use server";

import { revalidatePath } from "next/cache";
import { AdminSettingsApi } from "@/lib/api/admin-settings.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  registrationPolicyUpdateRequestSchema,
  type RegistrationPolicy,
  type RegistrationPolicyUpdateRequest,
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

export async function getRegistrationPolicyAction(): Promise<RegistrationPolicy> {
  await requireRole(UserRole.Admin);
  const token = await accessTokenOrThrow();
  return AdminSettingsApi.getRegistrationPolicy(token);
}

export async function updateRegistrationPolicyAction(
  input: RegistrationPolicyUpdateRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = registrationPolicyUpdateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSettingsApi.updateRegistrationPolicy(token, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSettings);
  return { success: true };
}