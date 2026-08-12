"use server";

import { redirect } from "next/navigation";
import { AuthApi } from "@/lib/api/auth.api";
import { ApiError } from "@/lib/api/client";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  forgotPasswordRequestSchema,
  loginRequestSchema,
  registerRequestSchema,
  resetPasswordRequestSchema,
  type ForgotPasswordRequest,
  type LoginRequest,
  type RegisterRequest,
  type ResetPasswordRequest,
} from "@/lib/api/schemas/auth.schema";
import {
  CLOSED_REGISTRATION_POLICY,
  type RegistrationPolicy,
} from "@/lib/api/schemas/settings.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES, sanitizeNextPath } from "@/lib/routes";
import { roleHome } from "@/lib/auth/constants";
import { applyAuthResponse, clearSession, getRefreshToken } from "@/lib/auth/session";

export interface ActionResult {
  success: boolean;
  error?: string;
  fieldErrors?: Record<string, string>;
}

export async function loginAction(input: LoginRequest, nextPath?: string): Promise<ActionResult> {
  const parsed = loginRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }

  let redirectPath: string;
  try {
    const auth = await AuthApi.login(parsed.data.email, parsed.data.password);
    const session = await applyAuthResponse(auth);
    redirectPath = sanitizeNextPath(nextPath) ?? roleHome(session.role);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }

  redirect(redirectPath);
}

export async function registerAction(input: RegisterRequest): Promise<ActionResult> {
  const parsed = registerRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }

  try {
    await AuthApi.register(parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }

  redirect(ROUTES.pendingApproval);
}

/**
 * Reads the roles an admin has opened to public sign-up. Falls back to closed rather than
 * surfacing an error: the form then offers nothing, which matches what the backend would accept
 * if it is genuinely unreachable, and never invites a visitor into a role that may be shut.
 */
export async function getRegistrationPolicyAction(): Promise<RegistrationPolicy> {
  try {
    return await AuthApi.getRegistrationPolicy();
  } catch {
    return CLOSED_REGISTRATION_POLICY;
  }
}

/**
 * Always reports success, even when the email is unknown or the backend silently no-ops: only a
 * genuine failure (e.g. the resend cooldown) surfaces an error, so the form never leaks whether an
 * address is registered.
 */
export async function forgotPasswordAction(input: ForgotPasswordRequest): Promise<ActionResult> {
  const parsed = forgotPasswordRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }

  try {
    await AuthApi.forgotPassword(parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }

  return { success: true };
}

/** Does not sign the user in: the backend issues no tokens for an OTP-based reset. */
export async function resetPasswordAction(input: ResetPasswordRequest): Promise<ActionResult> {
  const parsed = resetPasswordRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }

  try {
    await AuthApi.resetPassword(parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }

  redirect(ROUTES.login);
}

export async function logoutAction(): Promise<void> {
  const refreshToken = await getRefreshToken();
  if (refreshToken) {
    try {
      await AuthApi.logout(refreshToken);
    } catch {
      // Best-effort revoke: cookies are cleared regardless so the user is signed out locally.
    }
  }

  await clearSession();
  redirect(ROUTES.login);
}
