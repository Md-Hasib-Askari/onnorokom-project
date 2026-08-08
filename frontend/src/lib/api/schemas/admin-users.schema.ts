import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import {
  AccountStatus,
  accountStatusSchema,
  emailSchema,
  fullNameSchema,
  UserRole,
  userRoleSchema,
} from "./common.schema";
import { passwordSchema } from "./auth.schema";

// ---- GET /api/admin/users, GET /api/admin/users/pending ----

export const adminUserSummarySchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  status: accountStatusSchema,
  createdAt: z.string(),
  isActive: z.boolean(),
  studentGradeId: z.string().nullable(),
  gradeName: z.string().nullable(),
});
export type AdminUserSummary = z.infer<typeof adminUserSummarySchema>;

export const adminUserListResponseSchema = z.array(adminUserSummarySchema);

// ---- POST /api/admin/users/approve ----

export const approveUserRequestSchema = z.object({
  userId: z.uuid(),
  approve: z.boolean(),
});
export type ApproveUserRequest = z.infer<typeof approveUserRequestSchema>;

export const approveUserResponseSchema = z.object({
  id: z.string(),
  email: z.string(),
  fullName: z.string(),
  role: userRoleSchema,
  status: accountStatusSchema,
});
export type ApproveUserResponse = z.infer<typeof approveUserResponseSchema>;

// ---- POST /api/admin/users ----

export const adminCreateUserRequestSchema = z
  .object({
    fullName: fullNameSchema,
    email: emailSchema,
    password: passwordSchema,
    role: userRoleSchema,
    studentGradeId: z.uuid().optional(),
  })
  .refine((data) => data.role !== UserRole.Student || !!data.studentGradeId, {
    message: VALIDATION_MESSAGES.gradeRequired,
    path: ["studentGradeId"],
  });
export type AdminCreateUserRequest = z.infer<typeof adminCreateUserRequestSchema>;

export const adminCreateUserResponseSchema = adminUserSummarySchema;
export type AdminCreateUserResponse = z.infer<typeof adminCreateUserResponseSchema>;

// ---- PUT /api/admin/users/:id ----

export const teacherProfileInputSchema = z.object({
  department: z.string().trim().optional(),
  designation: z.string().trim().optional(),
  qualification: z.string().trim().optional(),
  phoneNumber: z.string().trim().optional(),
  address: z.string().trim().optional(),
  dateOfJoining: z.string().optional(),
});
export type TeacherProfileInput = z.infer<typeof teacherProfileInputSchema>;

export const adminProfileInputSchema = z.object({
  position: z.string().trim().optional(),
  phoneNumber: z.string().trim().optional(),
});
export type AdminProfileInput = z.infer<typeof adminProfileInputSchema>;

/** An admin can move an account to Approved or Rejected, but never back to Pending. */
export const editableAccountStatusSchema = z.enum([
  AccountStatus.Approved,
  AccountStatus.Rejected,
]);
export type EditableAccountStatus = z.infer<typeof editableAccountStatusSchema>;
export const EDITABLE_ACCOUNT_STATUSES = editableAccountStatusSchema.options;

export const adminUpdateUserRequestSchema = z.object({
  fullName: fullNameSchema,
  email: emailSchema,
  status: editableAccountStatusSchema,
  isActive: z.boolean(),
  studentGradeId: z.uuid().optional(),
  teacherProfile: teacherProfileInputSchema.optional(),
  adminProfile: adminProfileInputSchema.optional(),
});
export type AdminUpdateUserRequest = z.infer<typeof adminUpdateUserRequestSchema>;

/** studentGradeId is only required when editing a user whose role is Student. */
export function adminUpdateUserSchemaFor(role: UserRole) {
  if (role !== UserRole.Student) return adminUpdateUserRequestSchema;
  return adminUpdateUserRequestSchema.refine((data) => !!data.studentGradeId, {
    message: VALIDATION_MESSAGES.gradeRequired,
    path: ["studentGradeId"],
  });
}

export const adminUpdateUserResponseSchema = adminUserSummarySchema;
export type AdminUpdateUserResponse = z.infer<typeof adminUpdateUserResponseSchema>;
