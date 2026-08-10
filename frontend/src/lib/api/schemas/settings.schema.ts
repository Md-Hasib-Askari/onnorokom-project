import { z } from "zod";

// ---- GET /api/admin/settings/registration-policy, GET /api/auth/registration-policy ----

/**
 * Which roles the public sign-up form currently accepts. The anonymous auth endpoint and the
 * admin endpoint return the same shape, so both parse against this schema.
 */
export const registrationPolicySchema = z.object({
  teacherSelfRegistrationEnabled: z.boolean(),
  studentSelfRegistrationEnabled: z.boolean(),
});
export type RegistrationPolicy = z.infer<typeof registrationPolicySchema>;

// ---- PUT /api/admin/settings/registration-policy ----

/** Both flags are always sent, so a save writes the admin's full intent rather than a delta. */
export const registrationPolicyUpdateRequestSchema = registrationPolicySchema;
export type RegistrationPolicyUpdateRequest = z.infer<typeof registrationPolicyUpdateRequestSchema>;

export const registrationPolicyUpdateResponseSchema = registrationPolicySchema;
export type RegistrationPolicyUpdateResponse = z.infer<typeof registrationPolicyUpdateResponseSchema>;

/**
 * Used when the policy cannot be read. Closing both roles is the safe direction: the sign-up form
 * hides options it is unsure about, and the backend refuses anything it did not open anyway.
 */
export const CLOSED_REGISTRATION_POLICY: RegistrationPolicy = {
  teacherSelfRegistrationEnabled: false,
  studentSelfRegistrationEnabled: false,
};