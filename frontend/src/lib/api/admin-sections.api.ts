import { apiClient, authHeaders } from "./client";
import {
  adminSectionListResponseSchema,
  assignSectionSubjectTeacherResponseSchema,
  sectionSubjectsResponseSchema,
  sectionSummarySchema,
  type SectionCreateRequest,
  type SectionSubjectItem,
  type SectionSummary,
  type SectionUpdateRequest,
} from "./schemas/sections.schema";

/** `/api/admin/sections/*`, requires an Admin access token. */
export class AdminSectionsApi {
  static async list(accessToken: string): Promise<SectionSummary[]> {
    const { data } = await apiClient.get("/api/admin/sections", {
      headers: authHeaders(accessToken),
    });
    return adminSectionListResponseSchema.parse(data);
  }

  static async create(accessToken: string, payload: SectionCreateRequest): Promise<SectionSummary> {
    const { data } = await apiClient.post("/api/admin/sections", payload, {
      headers: authHeaders(accessToken),
    });
    return sectionSummarySchema.parse(data);
  }

  static async update(
    accessToken: string,
    id: string,
    payload: SectionUpdateRequest
  ): Promise<SectionSummary> {
    const { data } = await apiClient.put(`/api/admin/sections/${id}`, payload, {
      headers: authHeaders(accessToken),
    });
    return sectionSummarySchema.parse(data);
  }

  static async remove(accessToken: string, id: string): Promise<void> {
    await apiClient.delete(`/api/admin/sections/${id}`, {
      headers: authHeaders(accessToken),
    });
  }

  static async getSectionSubjects(accessToken: string, sectionId: string): Promise<SectionSubjectItem[]> {
    const { data } = await apiClient.get(`/api/admin/sections/${sectionId}/subjects`, {
      headers: authHeaders(accessToken),
    });
    return sectionSubjectsResponseSchema.parse(data);
  }

  static async assignSubjectTeacher(
    accessToken: string,
    sectionId: string,
    subjectId: string,
    teacherId: string
  ): Promise<SectionSubjectItem> {
    const { data } = await apiClient.post(
      `/api/admin/sections/${sectionId}/subjects/${subjectId}/teacher`,
      { teacherId },
      { headers: authHeaders(accessToken) }
    );
    return assignSectionSubjectTeacherResponseSchema.parse(data);
  }

  static async unassignSubjectTeacher(
    accessToken: string,
    sectionId: string,
    subjectId: string
  ): Promise<SectionSubjectItem> {
    const { data } = await apiClient.delete(
      `/api/admin/sections/${sectionId}/subjects/${subjectId}/teacher`,
      { headers: authHeaders(accessToken) }
    );
    return assignSectionSubjectTeacherResponseSchema.parse(data);
  }
}