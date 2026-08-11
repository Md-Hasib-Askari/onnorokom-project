import { apiClient, authHeaders } from "./client";
import {
  studentAssignmentDetailSchema,
  studentAssignmentListSchema,
  type StudentAssignmentDetail,
  type StudentAssignmentListItem,
  type SubmissionRequest,
} from "./schemas/student.schema";

const ASSIGNMENTS_PATH = "/api/student/assignments";

/** The submission is addressed through its assignment, so submission ids stay out of this API. */
const submissionPath = (assignmentId: string) => `${ASSIGNMENTS_PATH}/${assignmentId}/submission`;

/** `/api/student/*`, requires a Student access token. Every route is scoped to the caller's section. */
export class StudentApi {
  static async listAssignments(accessToken: string): Promise<StudentAssignmentListItem[]> {
    const { data } = await apiClient.get(ASSIGNMENTS_PATH, {
      headers: authHeaders(accessToken),
    });
    return studentAssignmentListSchema.parse(data);
  }

  static async getAssignment(accessToken: string, id: string): Promise<StudentAssignmentDetail> {
    const { data } = await apiClient.get(`${ASSIGNMENTS_PATH}/${id}`, {
      headers: authHeaders(accessToken),
    });
    return studentAssignmentDetailSchema.parse(data);
  }

  static async submit(
    accessToken: string,
    assignmentId: string,
    payload: SubmissionRequest
  ): Promise<StudentAssignmentDetail> {
    const { data } = await apiClient.post(submissionPath(assignmentId), payload, {
      headers: authHeaders(accessToken),
    });
    return studentAssignmentDetailSchema.parse(data);
  }

  static async updateSubmission(
    accessToken: string,
    assignmentId: string,
    payload: SubmissionRequest
  ): Promise<StudentAssignmentDetail> {
    const { data } = await apiClient.put(submissionPath(assignmentId), payload, {
      headers: authHeaders(accessToken),
    });
    return studentAssignmentDetailSchema.parse(data);
  }
}