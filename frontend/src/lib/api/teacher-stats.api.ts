import { apiClient, authHeaders } from "./client";
import { teacherOverviewSchema, type TeacherOverview } from "./schemas/teacher-stats.schema";

/** `/api/teacher/stats/*`, requires a Teacher access token. */
export class TeacherStatsApi {
  static async getOverview(accessToken: string): Promise<TeacherOverview> {
    const { data } = await apiClient.get("/api/teacher/stats/overview", {
      headers: authHeaders(accessToken),
    });
    return teacherOverviewSchema.parse(data);
  }
}
