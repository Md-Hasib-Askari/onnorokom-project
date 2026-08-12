import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { fullNameSchema, userRoleSchema } from "./common.schema";
import { passwordSchema } from "./auth.schema";

// ---- GET /api/profile, PUT /api/profile ----

export const profileSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  mustChangePassword: z.boolean(),
});
export type Profile = z.infer<typeof profileSchema>;

/** Only the full name is editable; email and role are set by an admin. */
export const updateProfileRequestSchema = z.object({
  fullName: fullNameSchema,
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