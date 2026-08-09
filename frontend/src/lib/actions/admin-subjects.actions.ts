"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { AdminSubjectsApi } from "@/lib/api/admin-subjects.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  subjectCreateRequestSchema,
  subjectUpdateRequestSchema,
  type SubjectCreateRequest,
  type SubjectUpdateRequest,
} from "@/lib/api/schemas/admin-subjects.schema";
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

/** Redirects to login on an expired/missing session instead of surfacing a generic load error. */
function redirectOnSessionExpired(error: unknown): never {
  if (error instanceof ApiError && error.status === HttpStatus.Unauthorized) {
    redirect(ROUTES.login);
  }
  throw error;
}

export async function listSubjectsAction() {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    return await AdminSubjectsApi.list(token);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function createSubjectAction(input: SubjectCreateRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = subjectCreateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSubjectsApi.create(token, {
      ...parsed.data,
      teacherId: parsed.data.teacherId || undefined,
    });
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSubjects);
  return { success: true };
}

export async function updateSubjectAction(id: string, input: SubjectUpdateRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = subjectUpdateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSubjectsApi.update(token, id, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSubjects);
  return { success: true };
}

export async function deleteSubjectAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminSubjectsApi.remove(token, id);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSubjects);
  return { success: true };
}

export async function assignTeacherAction(subjectId: string, teacherId: string): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminSubjectsApi.assignTeacher(token, subjectId, teacherId);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSubjects);
  return { success: true };
}

export async function unassignTeacherAction(subjectId: string): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminSubjectsApi.unassignTeacher(token, subjectId);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSubjects);
  return { success: true };
}