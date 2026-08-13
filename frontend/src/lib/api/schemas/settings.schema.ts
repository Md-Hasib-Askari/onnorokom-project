import { z } from "zod";

// ---- GET /api/auth/registration-policy ----

/**
 * Which roles the public sign-up form currently accepts. The anonymous auth endpoint returns this
 * shape; the admin view reads the same flags as part of the full system-settings payload.
 */
export const registrationPolicySchema = z.object({
  teacherSelfRegistrationEnabled: z.boolean(),
  studentSelfRegistrationEnabled: z.boolean(),
});
export type RegistrationPolicy = z.infer<typeof registrationPolicySchema>;

/**
 * Used when the policy cannot be read. Closing both roles is the safe direction: the sign-up form
 * hides options it is unsure about, and the backend refuses anything it did not open anyway.
 */
export const CLOSED_REGISTRATION_POLICY: RegistrationPolicy = {
  teacherSelfRegistrationEnabled: false,
  studentSelfRegistrationEnabled: false,
};

// ---- GET/PUT /api/admin/settings ----

/** Every admin-tunable system setting in one payload, fetched and saved atomically. */
export const systemSettingsSchema = z.object({
  teacherSelfRegistrationEnabled: z.boolean(),
  studentSelfRegistrationEnabled: z.boolean(),
  teacherProfileSelfEditEnabled: z.boolean(),
  studentProfileSelfEditEnabled: z.boolean(),
});
export type SystemSettings = z.infer<typeof systemSettingsSchema>;

/** All four flags are always sent, so a save writes the admin's full intent rather than a delta. */
export const systemSettingsUpdateRequestSchema = systemSettingsSchema;
export type SystemSettingsUpdateRequest = z.infer<typeof systemSettingsUpdateRequestSchema>;

export const systemSettingsUpdateResponseSchema = systemSettingsSchema;
export type SystemSettingsUpdateResponse = z.infer<typeof systemSettingsUpdateResponseSchema>;

/**
 * Used when the settings cannot be read. Closing everything is the safe direction: the admin
 * would rather see all toggles off than have the UI claim the server is open by default.
 */
export const CLOSED_SYSTEM_SETTINGS: SystemSettings = {
  teacherSelfRegistrationEnabled: false,
  studentSelfRegistrationEnabled: false,
  teacherProfileSelfEditEnabled: false,
  studentProfileSelfEditEnabled: false,
};
