import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";

export const userRoleSchema = z.enum(["Admin", "Teacher", "Student"]);
export type UserRole = z.infer<typeof userRoleSchema>;
/** Enum-style accessor (`UserRole.Admin`) so role literals are written exactly once. */
export const UserRole = userRoleSchema.enum;
/** Every role, in display order. Use this to build role dropdowns. */
export const USER_ROLES = userRoleSchema.options;

export const accountStatusSchema = z.enum(["Pending", "Approved", "Rejected"]);
export type AccountStatus = z.infer<typeof accountStatusSchema>;
/** Enum-style accessor (`AccountStatus.Pending`) so status literals are written exactly once. */
export const AccountStatus = accountStatusSchema.enum;

export const FULL_NAME_MAX_LENGTH = 100;

export const fullNameSchema = z
  .string()
  .trim()
  .min(1, VALIDATION_MESSAGES.fullNameRequired)
  .max(FULL_NAME_MAX_LENGTH);

/** Trims/lowercases first, then validates format (login/register are case-insensitive). */
export const emailSchema = z
  .string()
  .trim()
  .toLowerCase()
  .min(1, VALIDATION_MESSAGES.emailRequired)
  .pipe(z.email(VALIDATION_MESSAGES.emailInvalid));
