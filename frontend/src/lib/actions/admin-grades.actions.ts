"use server";

import { revalidatePath } from "next/cache";
import { AdminGradesApi } from "@/lib/api/admin-grades.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  gradeCreateRequestSchema,
  gradeUpdateRequestSchema,
  type GradeCreateRequest,
  type GradeUpdateRequest,
} from "@/lib/api/schemas/grades.schema";
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

export async function createGradeAction(input: GradeCreateRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = gradeCreateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminGradesApi.create(token, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminGrades);
  return { success: true };
}

export async function updateGradeAction(id: string, input: GradeUpdateRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = gradeUpdateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminGradesApi.update(token, id, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminGrades);
  return { success: true };
}

export async function deleteGradeAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminGradesApi.remove(token, id);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminGrades);
  return { success: true };
}