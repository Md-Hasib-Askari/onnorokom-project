import { apiClient, authHeaders } from "./client";
import { authResponseSchema, type AuthResponse } from "./schemas/auth.schema";
import {
  profileSchema,
  updateProfileResponseSchema,
  type ChangePasswordRequest,
  type Profile,
  type UpdateProfileRequest,
} from "./schemas/profile.schema";

/** `/api/profile/*`: the signed-in user's own account, requires an access token. */
export class ProfileApi {
  static async get(accessToken: string): Promise<Profile> {
    const { data } = await apiClient.get("/api/profile", {
      headers: authHeaders(accessToken),
    });
    return profileSchema.parse(data);
  }

  static async update(accessToken: string, payload: UpdateProfileRequest): Promise<Profile> {
    // `<input type="date">` submits "" when left blank, but the backend binds these fields to
    // `DateTimeOffset?`, which fails to deserialize an empty string. Omit the key instead.
    const sanitized: UpdateProfileRequest = {
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
    const { data } = await apiClient.put("/api/profile", sanitized, {
      headers: authHeaders(accessToken),
    });
    return updateProfileResponseSchema.parse(data);
  }

  /** Reissues tokens on success, so the caller must persist the returned session. */
  static async changePassword(
    accessToken: string,
    payload: ChangePasswordRequest
  ): Promise<AuthResponse> {
    const { data } = await apiClient.post("/api/profile/change-password", payload, {
      headers: authHeaders(accessToken),
    });
    return authResponseSchema.parse(data);
  }
}