"use server";

import { revalidatePath } from "next/cache";
import { AdminUsersApi } from "@/lib/api/admin-users.api";
import { AdminGradesApi } from "@/lib/api/admin-grades.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  adminCreateUserRequestSchema,
  adminUpdateUserSchemaFor,
  type AdminCreateUserRequest,
  type AdminUpdateUserRequest,
} from "@/lib/api/schemas/admin-users.schema";
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

export async function listUsersAction() {
  await requireRole(UserRole.Admin);
  const token = await accessTokenOrThrow();
  return AdminUsersApi.list(token);
}

export async function listPendingUsersAction() {
  await requireRole(UserRole.Admin);
  const token = await accessTokenOrThrow();
  return AdminUsersApi.listPending(token);
}

export async function listGradesAction() {
  await requireRole(UserRole.Admin);
  const token = await accessTokenOrThrow();
  return AdminGradesApi.list(token);
}

export async function approveUserAction(userId: string, approve: boolean): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminUsersApi.approve(token, userId, approve);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminUsers);
  return { success: true };
}

export async function createUserAction(input: AdminCreateUserRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = adminCreateUserRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminUsersApi.create(token, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminUsers);
  return { success: true };
}

export async function updateUserAction(
  id: string,
  role: UserRole,
  input: AdminUpdateUserRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = adminUpdateUserSchemaFor(role).safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminUsersApi.update(token, id, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminUsers);
  return { success: true };
}

export async function deleteUserAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminUsersApi.remove(token, id);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminUsers);
  return { success: true };
}
