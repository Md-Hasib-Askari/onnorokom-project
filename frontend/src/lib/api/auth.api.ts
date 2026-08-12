import { apiClient } from "./client";
import {
  authResponseSchema,
  registerResponseSchema,
  type AuthResponse,
  type ForgotPasswordRequest,
  type RegisterRequest,
  type RegisterResponse,
  type ResetPasswordRequest,
} from "./schemas/auth.schema";
import { registrationPolicySchema, type RegistrationPolicy } from "./schemas/settings.schema";

/** `/api/auth/*`: anonymous endpoints, called from the Next.js server only. */
export class AuthApi {
  static async login(email: string, password: string): Promise<AuthResponse> {
    const { data } = await apiClient.post("/api/auth/login", { email, password });
    return authResponseSchema.parse(data);
  }

  static async refresh(refreshToken: string): Promise<AuthResponse> {
    const { data } = await apiClient.post("/api/auth/refresh", { refreshToken });
    return authResponseSchema.parse(data);
  }

  static async logout(refreshToken: string): Promise<void> {
    await apiClient.post("/api/auth/logout", { refreshToken });
  }

  static async register(payload: RegisterRequest): Promise<RegisterResponse> {
    const { data } = await apiClient.post("/api/auth/register", payload);
    return registerResponseSchema.parse(data);
  }

  /** Anonymous read of the two policy flags, so the sign-up form can offer only the open roles. */
  static async getRegistrationPolicy(): Promise<RegistrationPolicy> {
    const { data } = await apiClient.get("/api/auth/registration-policy");
    return registrationPolicySchema.parse(data);
  }

  /** Silently no-ops server-side when the email doesn't exist, so this never throws for that case. */
  static async forgotPassword(payload: ForgotPasswordRequest): Promise<void> {
    await apiClient.post("/api/auth/forgot-password", payload);
  }

  /** Does not log the user in: the backend issues no tokens for an OTP-based reset. */
  static async resetPassword(payload: ResetPasswordRequest): Promise<void> {
    await apiClient.post("/api/auth/reset-password", payload);
  }
}
