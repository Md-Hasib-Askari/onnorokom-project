import { apiClient, authHeaders } from "./client";
import {
  gradeListResponseSchema,
  gradeSummarySchema,
  type GradeCreateRequest,
  type GradeSummary,
  type GradeUpdateRequest,
} from "./schemas/grades.schema";

/** `/api/admin/grades`, requires an Admin access token. */
export class AdminGradesApi {
  static async list(accessToken: string): Promise<GradeSummary[]> {
    const { data } = await apiClient.get("/api/admin/grades", {
      headers: authHeaders(accessToken),
    });
    return gradeListResponseSchema.parse(data).items;
  }

  static async create(accessToken: string, payload: GradeCreateRequest): Promise<GradeSummary> {
    const { data } = await apiClient.post("/api/admin/grades", payload, {
      headers: authHeaders(accessToken),
    });
    return gradeSummarySchema.parse(data);
  }

  static async update(
    accessToken: string,
    id: string,
    payload: GradeUpdateRequest
  ): Promise<GradeSummary> {
    const { data } = await apiClient.put(`/api/admin/grades/${id}`, payload, {
      headers: authHeaders(accessToken),
    });
    return gradeSummarySchema.parse(data);
  }

  static async remove(accessToken: string, id: string): Promise<void> {
    await apiClient.delete(`/api/admin/grades/${id}`, {
      headers: authHeaders(accessToken),
    });
  }
}
