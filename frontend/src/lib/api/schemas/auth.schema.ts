import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import {
  accountStatusSchema,
  emailSchema,
  fullNameSchema,
  UserRole,
  userRoleSchema,
} from "./common.schema";

// ---- POST /api/auth/login ----

export const loginRequestSchema = z.object({
  email: emailSchema,
  password: z.string().min(1, VALIDATION_MESSAGES.passwordRequired),
});
export type LoginRequest = z.infer<typeof loginRequestSchema>;

export const authResponseSchema = z.object({
  accessToken: z.string(),
  refreshToken: z.string(),
  accessTokenExpiresAt: z.string(),
  userId: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  status: accountStatusSchema,
});
export type AuthResponse = z.infer<typeof authResponseSchema>;

// ---- POST /api/auth/refresh ----

export const refreshRequestSchema = z.object({
  refreshToken: z.string().min(1),
});
export type RefreshRequest = z.infer<typeof refreshRequestSchema>;

// ---- POST /api/auth/register ----

/** Mirrors the backend's password policy (AuthController/PasswordValidator). */
export const PASSWORD_MIN_LENGTH = 8;

export const passwordSchema = z
  .string()
  .min(PASSWORD_MIN_LENGTH, VALIDATION_MESSAGES.passwordTooShort(PASSWORD_MIN_LENGTH))
  .regex(/[A-Z]/, VALIDATION_MESSAGES.passwordNeedsUppercase)
  .regex(/[a-z]/, VALIDATION_MESSAGES.passwordNeedsLowercase)
  .regex(/[0-9]/, VALIDATION_MESSAGES.passwordNeedsDigit)
  .regex(/[^A-Za-z0-9]/, VALIDATION_MESSAGES.passwordNeedsSpecial);

/** Roles a visitor may pick on the public register form (admins are never self-served). */
export const selfRegisterRoleSchema = z.enum([UserRole.Teacher, UserRole.Student]);
export type SelfRegisterRole = z.infer<typeof selfRegisterRoleSchema>;

/**
 * Sign-up carries no section: a student picks none, and the approving admin assigns one. Keeping
 * the roster out of the public form means nothing about it leaks to anonymous visitors.
 */
export const registerRequestSchema = z.object({
  fullName: fullNameSchema,
  email: emailSchema,
  password: passwordSchema,
  role: selfRegisterRoleSchema,
});
export type RegisterRequest = z.infer<typeof registerRequestSchema>;

export const registerResponseSchema = z.object({
  id: z.string(),
  email: z.string(),
  fullName: z.string(),
  role: userRoleSchema,
  status: accountStatusSchema,
});
export type RegisterResponse = z.infer<typeof registerResponseSchema>;
