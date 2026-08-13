"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { ApiError } from "@/lib/api/client";
import { HttpStatus } from "@/lib/api/http-status";
import { StudentApi } from "@/lib/api/student.api";
import { fieldErrorsFrom } from "@/lib/api/zod-error";
import { UserRole } from "@/lib/api/schemas/common.schema";
import {
  submissionRequestSchema,
  type SubmissionRequest,
} from "@/lib/api/schemas/student.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { ROUTES, ROUTE_BUILDERS } from "@/lib/routes";
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

function failureFrom(error: unknown): ActionResult {
  if (error instanceof ApiError) {
    return { success: false, error: error.message, fieldErrors: error.fieldErrors };
  }
  return { success: false, error: ERROR_MESSAGES.genericRetry };
}

/** The list shows a status badge per row, so a write refreshes it alongside the detail page. */
function revalidateAssignment(assignmentId: string) {
  revalidatePath(ROUTES.studentAssignments);
  revalidatePath(ROUTE_BUILDERS.studentAssignment(assignmentId));
}

// ---- Reads ----

export async function listMyAssignmentsAction(params: { limit?: number; cursor?: string } = {}) {
  await requireRole(UserRole.Student);
  try {
    const token = await accessTokenOrThrow();
    return await StudentApi.listAssignments(token, params);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

export async function getMyAssignmentAction(id: string) {
  await requireRole(UserRole.Student);
  try {
    const token = await accessTokenOrThrow();
    return await StudentApi.getAssignment(token, id);
  } catch (error) {
    redirectOnSessionExpired(error);
  }
}

// ---- Submission mutations ----

export async function submitAssignmentAction(
  assignmentId: string,
  input: SubmissionRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Student);
  const parsed = submissionRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await StudentApi.submit(token, assignmentId, parsed.data);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(assignmentId);
  return { success: true };
}

export async function updateSubmissionAction(
  assignmentId: string,
  input: SubmissionRequest
): Promise<ActionResult> {
  await requireRole(UserRole.Student);
  const parsed = submissionRequestSchema.safeParse(input);
  if (!parsed.success) {
    return { success: false, error: ERROR_MESSAGES.validation, fieldErrors: fieldErrorsFrom(parsed.error) };
  }
  try {
    const token = await accessTokenOrThrow();
    await StudentApi.updateSubmission(token, assignmentId, parsed.data);
  } catch (error) {
    return failureFrom(error);
  }
  revalidateAssignment(assignmentId);
  return { success: true };
}