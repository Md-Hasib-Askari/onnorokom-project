import { apiClient, authHeaders } from "./client";
import {
  systemSettingsSchema,
  systemSettingsUpdateResponseSchema,
  type SystemSettings,
  type SystemSettingsUpdateRequest,
} from "./schemas/settings.schema";

/** `/api/admin/settings`, requires an Admin access token. */
export class AdminSettingsApi {
  static async getSystemSettings(accessToken: string): Promise<SystemSettings> {
    const { data } = await apiClient.get("/api/admin/settings", {
      headers: authHeaders(accessToken),
    });
    return systemSettingsSchema.parse(data);
  }

  static async updateSystemSettings(
    accessToken: string,
    payload: SystemSettingsUpdateRequest
  ): Promise<SystemSettings> {
    const { data } = await apiClient.put("/api/admin/settings", payload, {
      headers: authHeaders(accessToken),
    });
    return systemSettingsUpdateResponseSchema.parse(data);
  }
}
