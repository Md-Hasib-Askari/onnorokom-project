import { apiClient, authHeaders } from "./client";
import {
  registrationPolicySchema,
  registrationPolicyUpdateResponseSchema,
  type RegistrationPolicy,
  type RegistrationPolicyUpdateRequest,
} from "./schemas/settings.schema";

/** `/api/admin/settings/*`, requires an Admin access token. */
export class AdminSettingsApi {
  static async getRegistrationPolicy(accessToken: string): Promise<RegistrationPolicy> {
    const { data } = await apiClient.get("/api/admin/settings/registration-policy", {
      headers: authHeaders(accessToken),
    });
    return registrationPolicySchema.parse(data);
  }

  static async updateRegistrationPolicy(
    accessToken: string,
    payload: RegistrationPolicyUpdateRequest
  ): Promise<RegistrationPolicy> {
    const { data } = await apiClient.put("/api/admin/settings/registration-policy", payload, {
      headers: authHeaders(accessToken),
    });
    return registrationPolicyUpdateResponseSchema.parse(data);
  }
}