import { apiClient, authHeaders } from "./client";
import { gradeListResponseSchema, type GradeSummary } from "./schemas/grades.schema";

/** `/api/admin/grades`, requires an Admin access token. Used to populate the grade
 *  dropdown when an admin creates or edits a student. */
export class AdminGradesApi {
  static async list(accessToken: string): Promise<GradeSummary[]> {
    const { data } = await apiClient.get("/api/admin/grades", {
      headers: authHeaders(accessToken),
    });
    return gradeListResponseSchema.parse(data);
  }
}
