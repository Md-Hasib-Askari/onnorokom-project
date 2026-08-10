"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { AdminSectionsApi } from "@/lib/api/admin-sections.api";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import {
  assignSectionSubjectTeacherRequestSchema,
  sectionCreateRequestSchema,
  sectionUpdateRequestSchema,
  type SectionCreateRequest,
  type SectionUpdateRequest,
} from "@/lib/api/schemas/sections.schema";
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

export async function listSectionsAction() {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    return await AdminSectionsApi.list(token);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function createSectionAction(input: SectionCreateRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = sectionCreateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSectionsApi.create(token, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSections);
  return { success: true };
}

export async function updateSectionAction(id: string, input: SectionUpdateRequest): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = sectionUpdateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSectionsApi.update(token, id, parsed.data);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSections);
  return { success: true };
}

export async function deleteSectionAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminSectionsApi.remove(token, id);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSections);
  return { success: true };
}

export async function getSectionSubjectsAction(sectionId: string) {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    return await AdminSectionsApi.getSectionSubjects(token, sectionId);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function assignSectionSubjectTeacherAction(
  sectionId: string,
  subjectId: string,
  teacherId: string
): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  const parsed = assignSectionSubjectTeacherRequestSchema.safeParse({ teacherId });
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await AdminSectionsApi.assignSubjectTeacher(token, sectionId, subjectId, parsed.data.teacherId);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message, fieldErrors: error.fieldErrors };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSections);
  return { success: true };
}

export async function unassignSectionSubjectTeacherAction(
  sectionId: string,
  subjectId: string
): Promise<ActionResult> {
  await requireRole(UserRole.Admin);
  try {
    const token = await accessTokenOrThrow();
    await AdminSectionsApi.unassignSubjectTeacher(token, sectionId, subjectId);
  } catch (error) {
    if (error instanceof ApiError) return { success: false, error: error.message };
    return { success: false, error: ERROR_MESSAGES.genericRetry };
  }
  revalidatePath(ROUTES.adminSections);
  return { success: true };
}