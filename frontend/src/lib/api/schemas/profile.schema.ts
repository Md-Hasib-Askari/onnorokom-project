import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { fullNameSchema, userRoleSchema } from "./common.schema";
import { passwordSchema } from "./auth.schema";
import {
  adminProfileDetailSchema,
  adminProfileInputSchema,
  studentProfileDetailSchema,
  studentProfileInputSchema,
  teacherProfileDetailSchema,
  teacherProfileInputSchema,
} from "./admin-users.schema";

// ---- GET /api/profile, PUT /api/profile ----

export const profileSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  mustChangePassword: z.boolean(),
  canEditProfile: z.boolean(),
  studentProfile: studentProfileDetailSchema.nullable(),
  teacherProfile: teacherProfileDetailSchema.nullable(),
  adminProfile: adminProfileDetailSchema.nullable(),
});
export type Profile = z.infer<typeof profileSchema>;

/**
 * `fullName` is always editable. The nested profile blocks are only accepted (and only rendered
 * as editable fields) when the caller's role matches and, for Teacher/Student, an admin hasn't
 * disabled self-editing for that role (see `profileSchema.canEditProfile`).
 */
export const updateProfileRequestSchema = z.object({
  fullName: fullNameSchema,
  studentProfile: studentProfileInputSchema.optional(),
  teacherProfile: teacherProfileInputSchema.optional(),
  adminProfile: adminProfileInputSchema.optional(),
});
export type UpdateProfileRequest = z.infer<typeof updateProfileRequestSchema>;

export const updateProfileResponseSchema = profileSchema;
export type UpdateProfileResponse = z.infer<typeof updateProfileResponseSchema>;

// ---- POST /api/profile/change-password ----

export const changePasswordRequestSchema = z
  .object({
    currentPassword: z.string().min(1, VALIDATION_MESSAGES.currentPasswordRequired),
    newPassword: passwordSchema,
  })
  .refine((data) => data.newPassword !== data.currentPassword, {
    message: VALIDATION_MESSAGES.newPasswordSameAsCurrent,
    path: ["newPassword"],
  });
export type ChangePasswordRequest = z.infer<typeof changePasswordRequestSchema>;