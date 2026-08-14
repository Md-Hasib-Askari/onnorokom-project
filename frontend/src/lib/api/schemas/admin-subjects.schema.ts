import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { cursorPageSchema } from "./common.schema";

// ---- GET /api/admin/subjects ----

export const subjectSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  code: z.string().nullable(),
  gradeId: z.string(),
  gradeName: z.string().nullable(),
  teacherCount: z.number(),
});
export type SubjectSummary = z.infer<typeof subjectSummarySchema>;

export const adminSubjectListResponseSchema = cursorPageSchema(subjectSummarySchema);

// ---- POST /api/admin/subjects ----

export const subjectCreateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.subjectNameRequired).max(100),
  gradeId: z.string().min(1, VALIDATION_MESSAGES.subjectGradeRequired).pipe(z.uuid()),
  code: z.string().trim().max(20).optional(),
});
export type SubjectCreateRequest = z.infer<typeof subjectCreateRequestSchema>;

export const subjectCreateResponseSchema = subjectSummarySchema;
export type SubjectCreateResponse = z.infer<typeof subjectCreateResponseSchema>;

// ---- PUT /api/admin/subjects/:id ----
// No teacherId field: a subject carries no teacher of its own. Teachers are assigned per
// section, through the section subjects endpoints in sections.schema.ts.

export const subjectUpdateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.subjectNameRequired).max(100),
  gradeId: z.string().min(1, VALIDATION_MESSAGES.subjectGradeRequired).pipe(z.uuid()),
  code: z.string().trim().max(20).optional(),
});
export type SubjectUpdateRequest = z.infer<typeof subjectUpdateRequestSchema>;

export const subjectUpdateResponseSchema = subjectSummarySchema;
export type SubjectUpdateResponse = z.infer<typeof subjectUpdateResponseSchema>;