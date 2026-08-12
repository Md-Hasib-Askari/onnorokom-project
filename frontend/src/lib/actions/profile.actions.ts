"use server";

import { revalidatePath } from "next/cache";
import { ProfileApi } from "@/lib/api/profile.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  changePasswordRequestSchema,
  updateProfileRequestSchema,
  type ChangePasswordRequest,
  type Profile,
  type UpdateProfileRequest,
} from "@/lib/api/schemas/profile.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES } from "@/lib/routes";
import { applyAuthResponse, getAccessToken, requireSession } from "@/lib/auth/session";
import type { ActionResult } from "./auth.actions";

async function accessTokenOrThrow(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new ApiError(HttpStatus.Unauthorized, ERROR_MESSAGES.sessionExpired);
  return token;
}

export async function getProfileAction(): Promise<Profile> {
  await requireSession();
  const token = await accessTokenOrThrow();
  return ProfileApi.get(token);
}

export async function updateProfileAction(input: UpdateProfileRequest): Promise<ActionResult> {
  await requireSession();
  const parsed = updateProfileRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await ProfileApi.update(token, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.profile);
  return { success: true };
}

/**
 * Reissues the session's tokens on success, so the current session survives while the prior
 * refresh token (and therefore any other session) is revoked. The caller must persist the new
 * session with `applyAuthResponse`, which happens here.
 */
export async function changePasswordAction(input: ChangePasswordRequest): Promise<ActionResult> {
  await requireSession();
  const parsed = changePasswordRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    const auth = await ProfileApi.changePassword(token, parsed.data);
    await applyAuthResponse(auth);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  return { success: true };
}