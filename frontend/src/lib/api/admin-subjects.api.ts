import { apiClient, authHeaders } from "./client";
import {
  adminSubjectListResponseSchema,
  subjectSummarySchema,
  type AssignTeacherResponse,
  type SubjectCreateRequest,
  type SubjectSummary,
  type SubjectUpdateRequest,
} from "./schemas/admin-subjects.schema";

/** `/api/admin/subjects/*`, requires an Admin access token. */
export class AdminSubjectsApi {
  static async list(accessToken: string): Promise<SubjectSummary[]> {
    const { data } = await apiClient.get("/api/admin/subjects", {
      headers: authHeaders(accessToken),
    });
    return adminSubjectListResponseSchema.parse(data);
  }

  static async create(accessToken: string, payload: SubjectCreateRequest): Promise<SubjectSummary> {
    const { data } = await apiClient.post("/api/admin/subjects", payload, {
      headers: authHeaders(accessToken),
    });
    return subjectSummarySchema.parse(data);
  }

  static async update(
    accessToken: string,
    id: string,
    payload: SubjectUpdateRequest
  ): Promise<SubjectSummary> {
    const { data } = await apiClient.put(`/api/admin/subjects/${id}`, payload, {
      headers: authHeaders(accessToken),
    });
    return subjectSummarySchema.parse(data);
  }

  static async remove(accessToken: string, id: string): Promise<void> {
    await apiClient.delete(`/api/admin/subjects/${id}`, {
      headers: authHeaders(accessToken),
    });
  }

  static async assignTeacher(
    accessToken: string,
    id: string,
    teacherId: string
  ): Promise<AssignTeacherResponse> {
    const { data } = await apiClient.post(
      `/api/admin/subjects/${id}/teacher`,
      { teacherId },
      { headers: authHeaders(accessToken) }
    );
    return subjectSummarySchema.parse(data);
  }

  static async unassignTeacher(accessToken: string, id: string): Promise<void> {
    await apiClient.delete(`/api/admin/subjects/${id}/teacher`, {
      headers: authHeaders(accessToken),
    });
  }
}