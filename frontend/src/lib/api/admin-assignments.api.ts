import { apiClient, authHeaders } from "./client";
import {
  adminAssignmentListResponseSchema,
  adminSubmissionListResponseSchema,
  type AssignmentListItem,
  type SubmissionListItem,
} from "./schemas/admin-assignments.schema";
import type { CursorPage } from "./schemas/common.schema";

export interface AdminAssignmentListParams {
  limit?: number;
  cursor?: string;
}

/** `/api/admin/assignments`, `/api/admin/submissions`, read-only, requires an Admin access token. */
export class AdminAssignmentsApi {
  static async list(
    accessToken: string,
    params: AdminAssignmentListParams = {}
  ): Promise<CursorPage<AssignmentListItem>> {
    const { data } = await apiClient.get("/api/admin/assignments", {
      headers: authHeaders(accessToken),
      params,
    });
    return adminAssignmentListResponseSchema.parse(data);
  }

  static async listSubmissions(
    accessToken: string,
    params: AdminAssignmentListParams = {}
  ): Promise<CursorPage<SubmissionListItem>> {
    const { data } = await apiClient.get("/api/admin/submissions", {
      headers: authHeaders(accessToken),
      params,
    });
    return adminSubmissionListResponseSchema.parse(data);
  }
}