"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { TeacherApi } from "@/lib/api/teacher.api";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import { UserRole } from "@/lib/api/schemas/common.schema";
import {
  assignmentCreateRequestSchema,
  assignmentUpdateRequestSchema,
  gradeSubmissionRequestSchema,
  type AssignmentCreateRequest,
  type AssignmentUpdateRequest,
  type GradeSubmissionRequest,
} from "@/lib/api/schemas/teacher.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES, ROUTE_BUILDERS } from "@/lib/routes";
import { getAccessToken, requireRole } from "@/lib/auth/session";
import type { ActionResult } from "./auth.actions";

/** Carries the new id so the create dialog can send the teacher straight to the detail page. */
export interface CreateAssignmentResult extends ActionResult {
  assignmentId?: string;
}

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

function failureFrom(error: unknown): ActionResult {
  if (error instanceof ApiError) {
    return { success: false, error: error.message, fieldErrors: error.fieldErrors };
  }
  return { success: false, error: ERROR_MESSAGES.genericRetry };
}

/** The list and the detail page both show submission counts, so a mutation refreshes both. */
function revalidateAssignment(assignmentId: string) {
  revalidatePath(ROUTES.teacherAssignments);
  revalidatePath(ROUTE_BUILDERS.teacherAssignment(assignmentId));
}

// ---- Reads ----

export async function listSectionSubjectsAction() {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    return await TeacherApi.listSectionSubjects(token);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function listStudentsAction(params: { limit?: number; cursor?: string } = {}) {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    return await TeacherApi.listStudents(token, params);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function listAssignmentsAction(params: { limit?: number; cursor?: string } = {}) {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    return await TeacherApi.listAssignments(token, params);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function getAssignmentAction(id: string) {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    return await TeacherApi.getAssignment(token, id);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function listSubmissionsAction(
  assignmentId: string,
  params: { limit?: number; cursor?: string } = {}
) {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    return await TeacherApi.listSubmissions(token, assignmentId, params);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

// ---- Assignment mutations ----

export async function createAssignmentAction(
  input: AssignmentCreateRequest
): Promise<CreateAssignmentResult> {
  await requireRole(UserRole.Teacher);
  const parsed = assignmentCreateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  let assignmentId: string;
  try {
    const token = await accessTokenOrThrow();
    const created = await TeacherApi.createAssignment(token, parsed.data);
    assignmentId = created.id;
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(assignmentId);
  return { success: true, assignmentId };
}

export async function updateAssignmentAction(
  id: string,
  input: AssignmentUpdateRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  const parsed = assignmentUpdateRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.updateAssignment(token, id, parsed.data);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(id);
  return { success: true };
}

export async function publishAssignmentAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.publishAssignment(token, id);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(id);
  return { success: true };
}

export async function unpublishAssignmentAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.unpublishAssignment(token, id);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(id);
  return { success: true };
}

export async function closeSubmissionsAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.closeSubmissions(token, id);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(id);
  return { success: true };
}

export async function reopenSubmissionsAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.reopenSubmissions(token, id);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(id);
  return { success: true };
}

export async function deleteAssignmentAction(id: string): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.deleteAssignment(token, id);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(id);
  return { success: true };
}

// ---- Submission mutations ----

export async function gradeSubmissionAction(
  assignmentId: string,
  submissionId: string,
  input: GradeSubmissionRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  const parsed = gradeSubmissionRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.gradeSubmission(token, submissionId, parsed.data);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(assignmentId);
  return { success: true };
}

export async function returnSubmissionAction(
  assignmentId: string,
  submissionId: string
): Promise<ActionResult> {
  await requireRole(UserRole.Teacher);
  try {
    const token = await accessTokenOrThrow();
    await TeacherApi.returnSubmission(token, submissionId);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(assignmentId);
  return { success: true };
}