import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";

// ---- GET /api/admin/subjects ----

export const subjectSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  code: z.string().nullable(),
  gradeId: z.string(),
  gradeName: z.string().nullable(),
  teacherId: z.string().nullable(),
  teacherName: z.string().nullable(),
});
export type SubjectSummary = z.infer<typeof subjectSummarySchema>;

export const adminSubjectListResponseSchema = z.array(subjectSummarySchema);

// ---- POST /api/admin/subjects ----

export const subjectCreateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.subjectNameRequired).max(100),
  gradeId: z.string().min(1, VALIDATION_MESSAGES.subjectGradeRequired).pipe(z.uuid()),
  code: z.string().trim().max(20).optional(),
  // Optional at creation: a subject can be created before a teacher is assigned.
  // "" (the Select's unset state) is accepted here; callers strip it before hitting the API.
  teacherId: z.union([z.literal(""), z.uuid()]).optional(),
});
export type SubjectCreateRequest = z.infer<typeof subjectCreateRequestSchema>;

export const subjectCreateResponseSchema = subjectSummarySchema;
export type SubjectCreateResponse = z.infer<typeof subjectCreateResponseSchema>;

// ---- PUT /api/admin/subjects/:id ----
// No teacherId field: reassigning a subject's teacher goes through the dedicated
// assign/unassign endpoints below, never through this update request.

export const subjectUpdateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.subjectNameRequired).max(100),
  gradeId: z.string().min(1, VALIDATION_MESSAGES.subjectGradeRequired).pipe(z.uuid()),
  code: z.string().trim().max(20).optional(),
});
export type SubjectUpdateRequest = z.infer<typeof subjectUpdateRequestSchema>;

export const subjectUpdateResponseSchema = subjectSummarySchema;
export type SubjectUpdateResponse = z.infer<typeof subjectUpdateResponseSchema>;

// ---- POST /api/admin/subjects/:id/teacher ----

export const assignTeacherRequestSchema = z.object({
  teacherId: z.uuid(),
});
export type AssignTeacherRequest = z.infer<typeof assignTeacherRequestSchema>;

export const assignTeacherResponseSchema = subjectSummarySchema;
export type AssignTeacherResponse = z.infer<typeof assignTeacherResponseSchema>;