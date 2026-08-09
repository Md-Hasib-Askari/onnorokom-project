import { apiClient, authHeaders } from "./client";
import {
  adminAssignmentListResponseSchema,
  adminSubmissionListResponseSchema,
  type AssignmentListItem,
  type SubmissionListItem,
} from "./schemas/admin-assignments.schema";

/** `/api/admin/assignments`, `/api/admin/submissions` — read-only, requires an Admin access token. */
export class AdminAssignmentsApi {
  static async list(accessToken: string): Promise<AssignmentListItem[]> {
    const { data } = await apiClient.get("/api/admin/assignments", {
      headers: authHeaders(accessToken),
    });
    return adminAssignmentListResponseSchema.parse(data);
  }

  static async listSubmissions(accessToken: string): Promise<SubmissionListItem[]> {
    const { data } = await apiClient.get("/api/admin/submissions", {
      headers: authHeaders(accessToken),
    });
    return adminSubmissionListResponseSchema.parse(data);
  }
}