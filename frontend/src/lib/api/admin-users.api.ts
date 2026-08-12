import { apiClient, authHeaders } from "./client";
import {
  adminUserListResponseSchema,
  adminUserSummarySchema,
  approveUserResponseSchema,
  type AdminCreateUserRequest,
  type AdminUpdateUserRequest,
  type AdminUserSummary,
  type ApproveUserRequest,
  type ApproveUserResponse,
} from "./schemas/admin-users.schema";

/** `/api/admin/users/*`, requires an Admin access token. */
export class AdminUsersApi {
  static async list(accessToken: string): Promise<AdminUserSummary[]> {
    const { data } = await apiClient.get("/api/admin/users", {
      headers: authHeaders(accessToken),
    });
    return adminUserListResponseSchema.parse(data);
  }

  static async listPending(accessToken: string): Promise<AdminUserSummary[]> {
    const { data } = await apiClient.get("/api/admin/users/pending", {
      headers: authHeaders(accessToken),
    });
    return adminUserListResponseSchema.parse(data);
  }

  static async approve(
    accessToken: string,
    payload: ApproveUserRequest
  ): Promise<ApproveUserResponse> {
    const { data } = await apiClient.post("/api/admin/users/approve", payload, {
      headers: authHeaders(accessToken),
    });
    return approveUserResponseSchema.parse(data);
  }

  static async create(
    accessToken: string,
    payload: AdminCreateUserRequest
  ): Promise<AdminUserSummary> {
    const { data } = await apiClient.post("/api/admin/users", payload, {
      headers: authHeaders(accessToken),
    });
    return adminUserSummarySchema.parse(data);
  }

  static async update(
    accessToken: string,
    id: string,
    payload: AdminUpdateUserRequest
  ): Promise<AdminUserSummary> {
    const { data } = await apiClient.put(`/api/admin/users/${id}`, payload, {
      headers: authHeaders(accessToken),
    });
    return adminUserSummarySchema.parse(data);
  }

  static async remove(accessToken: string, id: string): Promise<void> {
    await apiClient.delete(`/api/admin/users/${id}`, {
      headers: authHeaders(accessToken),
    });
  }

  /** Generates a random password and emails it to the user; nothing is returned to the admin. */
  static async resetPassword(accessToken: string, id: string): Promise<void> {
    await apiClient.post(`/api/admin/users/${id}/reset-password`, undefined, {
      headers: authHeaders(accessToken),
    });
  }
}
