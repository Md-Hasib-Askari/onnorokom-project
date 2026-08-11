import { apiClient, authHeaders } from "./client";
import {
  teacherAssignmentListSchema,
  teacherAssignmentSchema,
  teacherSectionSubjectListSchema,
  teacherSubmissionListSchema,
  teacherSubmissionSchema,
  type AssignmentCreateRequest,
  type AssignmentUpdateRequest,
  type GradeSubmissionRequest,
  type TeacherAssignment,
  type TeacherSectionSubject,
  type TeacherSubmission,
} from "./schemas/teacher.schema";

const ASSIGNMENTS_PATH = "/api/teacher/assignments";
const SUBMISSIONS_PATH = "/api/teacher/submissions";
const SECTION_SUBJECTS_PATH = "/api/teacher/section-subjects";

/** `/api/teacher/*`, requires a Teacher access token. Every route is scoped to the caller. */
export class TeacherApi {
  static async listSectionSubjects(accessToken: string): Promise<TeacherSectionSubject[]> {
    const { data } = await apiClient.get(SECTION_SUBJECTS_PATH, {
      headers: authHeaders(accessToken),
    });
    return teacherSectionSubjectListSchema.parse(data);
  }

  static async listAssignments(accessToken: string): Promise<TeacherAssignment[]> {
    const { data } = await apiClient.get(ASSIGNMENTS_PATH, {
      headers: authHeaders(accessToken),
    });
    return teacherAssignmentListSchema.parse(data);
  }

  static async getAssignment(accessToken: string, id: string): Promise<TeacherAssignment> {
    const { data } = await apiClient.get(`${ASSIGNMENTS_PATH}/${id}`, {
      headers: authHeaders(accessToken),
    });
    return teacherAssignmentSchema.parse(data);
  }

  static async createAssignment(
    accessToken: string,
    payload: AssignmentCreateRequest
  ): Promise<TeacherAssignment> {
    const { data } = await apiClient.post(ASSIGNMENTS_PATH, payload, {
      headers: authHeaders(accessToken),
    });
    return teacherAssignmentSchema.parse(data);
  }

  static async updateAssignment(
    accessToken: string,
    id: string,
    payload: AssignmentUpdateRequest
  ): Promise<TeacherAssignment> {
    const { data } = await apiClient.put(`${ASSIGNMENTS_PATH}/${id}`, payload, {
      headers: authHeaders(accessToken),
    });
    return teacherAssignmentSchema.parse(data);
  }

  static async publishAssignment(accessToken: string, id: string): Promise<TeacherAssignment> {
    const { data } = await apiClient.post(`${ASSIGNMENTS_PATH}/${id}/publish`, null, {
      headers: authHeaders(accessToken),
    });
    return teacherAssignmentSchema.parse(data);
  }

  static async deleteAssignment(accessToken: string, id: string): Promise<void> {
    await apiClient.delete(`${ASSIGNMENTS_PATH}/${id}`, {
      headers: authHeaders(accessToken),
    });
  }

  static async listSubmissions(accessToken: string, assignmentId: string): Promise<TeacherSubmission[]> {
    const { data } = await apiClient.get(`${ASSIGNMENTS_PATH}/${assignmentId}/submissions`, {
      headers: authHeaders(accessToken),
    });
    return teacherSubmissionListSchema.parse(data);
  }

  static async gradeSubmission(
    accessToken: string,
    submissionId: string,
    payload: GradeSubmissionRequest
  ): Promise<TeacherSubmission> {
    const { data } = await apiClient.put(`${SUBMISSIONS_PATH}/${submissionId}/grade`, payload, {
      headers: authHeaders(accessToken),
    });
    return teacherSubmissionSchema.parse(data);
  }

  static async returnSubmission(accessToken: string, submissionId: string): Promise<TeacherSubmission> {
    const { data } = await apiClient.post(`${SUBMISSIONS_PATH}/${submissionId}/return`, null, {
      headers: authHeaders(accessToken),
    });
    return teacherSubmissionSchema.parse(data);
  }
}