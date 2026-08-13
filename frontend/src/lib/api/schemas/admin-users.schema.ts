import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import {
  AccountStatus,
  accountStatusSchema,
  cursorPageSchema,
  emailSchema,
  fullNameSchema,
  UserRole,
  userRoleSchema,
} from "./common.schema";
import { passwordSchema } from "./auth.schema";

export const genderSchema = z.enum(["Male", "Female", "Other"]);
export type Gender = z.infer<typeof genderSchema>;
/** Enum-style accessor (`Gender.Male`) so gender literals are written exactly once. */
export const Gender = genderSchema.enum;
export const GENDERS = genderSchema.options;

// ---- GET /api/admin/users, GET /api/admin/users/pending ----

export const adminUserSummarySchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  status: accountStatusSchema,
  createdAt: z.string(),
  isActive: z.boolean(),
  studentSectionId: z.string().nullable(),
  sectionName: z.string().nullable(),
  gradeName: z.string().nullable(),
  teacherCode: z.string().nullable(),
  rollNumber: z.string().nullable(),
  dateOfBirth: z.string().nullable(),
  gender: genderSchema.nullable(),
  guardianName: z.string().nullable(),
  guardianPhone: z.string().nullable(),
  address: z.string().nullable(),
  admissionDate: z.string().nullable(),
});
export type AdminUserSummary = z.infer<typeof adminUserSummarySchema>;

export const adminUserListResponseSchema = cursorPageSchema(adminUserSummarySchema);

// ---- POST /api/admin/users/approve ----

/**
 * `studentSectionId` is required only when approving a self-registered student, who has no section
 * yet. Ignored for every other role and for students an admin created with a section already.
 */
export const approveUserRequestSchema = z.object({
  userId: z.uuid(),
  approve: z.boolean(),
  studentSectionId: z.uuid().optional(),
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
    studentSectionId: z.uuid().optional(),
  })
  .refine((data) => data.role !== UserRole.Student || !!data.studentSectionId, {
    message: VALIDATION_MESSAGES.sectionRequired,
    path: ["studentSectionId"],
  });
export type AdminCreateUserRequest = z.infer<typeof adminCreateUserRequestSchema>;

export const adminCreateUserResponseSchema = adminUserSummarySchema;
export type AdminCreateUserResponse = z.infer<typeof adminCreateUserResponseSchema>;

// ---- PUT /api/admin/users/:id ----

export const studentProfileInputSchema = z.object({
  rollNumber: z.string().trim().optional(),
  dateOfBirth: z.string().optional(),
  gender: genderSchema.optional(),
  guardianName: z.string().trim().optional(),
  guardianPhone: z.string().trim().optional(),
  address: z.string().trim().optional(),
  admissionDate: z.string().optional(),
});
export type StudentProfileInput = z.infer<typeof studentProfileInputSchema>;

export const teacherProfileInputSchema = z.object({
  teacherCode: z.string().trim().optional(),
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
  studentSectionId: z.uuid().optional(),
  studentProfile: studentProfileInputSchema.optional(),
  teacherProfile: teacherProfileInputSchema.optional(),
  adminProfile: adminProfileInputSchema.optional(),
});
export type AdminUpdateUserRequest = z.infer<typeof adminUpdateUserRequestSchema>;

/** studentSectionId is only required when editing a user whose role is Student. */
export function adminUpdateUserSchemaFor(role: UserRole) {
  if (role !== UserRole.Student) return adminUpdateUserRequestSchema;
  return adminUpdateUserRequestSchema.refine((data) => !!data.studentSectionId, {
    message: VALIDATION_MESSAGES.sectionRequired,
    path: ["studentSectionId"],
  });
}

export const adminUpdateUserResponseSchema = adminUserSummarySchema;
export type AdminUpdateUserResponse = z.infer<typeof adminUpdateUserResponseSchema>;

// ---- GET /api/admin/users/:id ----

export const studentProfileDetailSchema = z.object({
  sectionId: z.string(),
  sectionName: z.string().nullable(),
  gradeName: z.string().nullable(),
  rollNumber: z.string().nullable(),
  dateOfBirth: z.string().nullable(),
  gender: genderSchema.nullable(),
  guardianName: z.string().nullable(),
  guardianPhone: z.string().nullable(),
  address: z.string().nullable(),
  admissionDate: z.string().nullable(),
});
export type StudentProfileDetail = z.infer<typeof studentProfileDetailSchema>;

export const teacherProfileDetailSchema = z.object({
  teacherCode: z.string().nullable(),
  department: z.string().nullable(),
  designation: z.string().nullable(),
  qualification: z.string().nullable(),
  phoneNumber: z.string().nullable(),
  address: z.string().nullable(),
  dateOfJoining: z.string().nullable(),
});
export type TeacherProfileDetail = z.infer<typeof teacherProfileDetailSchema>;

export const adminProfileDetailSchema = z.object({
  position: z.string().nullable(),
  phoneNumber: z.string().nullable(),
});
export type AdminProfileDetail = z.infer<typeof adminProfileDetailSchema>;

export const userDetailSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  status: accountStatusSchema,
  createdAt: z.string(),
  isActive: z.boolean(),
  studentProfile: studentProfileDetailSchema.nullable(),
  teacherProfile: teacherProfileDetailSchema.nullable(),
  adminProfile: adminProfileDetailSchema.nullable(),
});
export type UserDetail = z.infer<typeof userDetailSchema>;
