import { apiClient, authHeaders } from "./client";
import {
  adminUserListResponseSchema,
  adminUserSummarySchema,
  approveUserResponseSchema,
  userDetailSchema,
  type AdminCreateUserRequest,
  type AdminUpdateUserRequest,
  type AdminUserSummary,
  type ApproveUserRequest,
  type ApproveUserResponse,
  type UserDetail,
} from "./schemas/admin-users.schema";
import type { CursorPage } from "./schemas/common.schema";

export interface AdminUserListParams {
  limit?: number;
  cursor?: string;
  status?: string;
  role?: string;
}

/** `/api/admin/users/*`, requires an Admin access token. */
export class AdminUsersApi {
  static async list(
    accessToken: string,
    params: AdminUserListParams = {}
  ): Promise<CursorPage<AdminUserSummary>> {
    const { data } = await apiClient.get("/api/admin/users", {
      headers: authHeaders(accessToken),
      params,
    });
    return adminUserListResponseSchema.parse(data);
  }

  static async getById(accessToken: string, id: string): Promise<UserDetail> {
    const { data } = await apiClient.get(`/api/admin/users/${id}`, {
      headers: authHeaders(accessToken),
    });
    return userDetailSchema.parse(data);
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
    // `<input type="date">` submits "" when left blank, but the backend binds these fields to
    // `DateTimeOffset?`, which fails to deserialize an empty string. Omit the key instead.
    const sanitized: AdminUpdateUserRequest = {
      ...payload,
      studentProfile: payload.studentProfile && {
        ...payload.studentProfile,
        dateOfBirth: payload.studentProfile.dateOfBirth || undefined,
        admissionDate: payload.studentProfile.admissionDate || undefined,
      },
      teacherProfile: payload.teacherProfile && {
        ...payload.teacherProfile,
        dateOfJoining: payload.teacherProfile.dateOfJoining || undefined,
      },
    };
    const { data } = await apiClient.put(`/api/admin/users/${id}`, sanitized, {
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
