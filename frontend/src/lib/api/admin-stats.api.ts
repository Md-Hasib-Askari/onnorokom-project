import { apiClient, authHeaders } from "./client";
import { adminOverviewSchema, type AdminOverview } from "./schemas/admin-stats.schema";

/** `/api/admin/stats/*`, requires an Admin access token. */
export class AdminStatsApi {
  static async getOverview(accessToken: string): Promise<AdminOverview> {
    const { data } = await apiClient.get("/api/admin/stats/overview", {
      headers: authHeaders(accessToken),
    });
    return adminOverviewSchema.parse(data);
  }
}
