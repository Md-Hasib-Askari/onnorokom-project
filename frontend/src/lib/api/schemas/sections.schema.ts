import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { cursorPageSchema } from "./common.schema";

// ---- GET /api/admin/sections ----

export const sectionSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  gradeId: z.string(),
  gradeName: z.string().nullable(),
  teacherCount: z.number(),
  studentCount: z.number(),
});
export type SectionSummary = z.infer<typeof sectionSummarySchema>;

export const adminSectionListResponseSchema = cursorPageSchema(sectionSummarySchema);

// ---- POST /api/admin/sections ----

export const sectionCreateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.sectionNameRequired).max(100),
  gradeId: z.string().min(1, VALIDATION_MESSAGES.sectionGradeRequired).pipe(z.uuid()),
});
export type SectionCreateRequest = z.infer<typeof sectionCreateRequestSchema>;

export const sectionCreateResponseSchema = sectionSummarySchema;
export type SectionCreateResponse = z.infer<typeof sectionCreateResponseSchema>;

// ---- PUT /api/admin/sections/:id ----

export const sectionUpdateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.sectionNameRequired).max(100),
  gradeId: z.string().min(1, VALIDATION_MESSAGES.sectionGradeRequired).pipe(z.uuid()),
});
export type SectionUpdateRequest = z.infer<typeof sectionUpdateRequestSchema>;

export const sectionUpdateResponseSchema = sectionSummarySchema;
export type SectionUpdateResponse = z.infer<typeof sectionUpdateResponseSchema>;

// ---- GET /api/admin/sections/:sectionId/subjects ----

export const sectionSubjectItemSchema = z.object({
  subjectId: z.string(),
  subjectName: z.string(),
  subjectCode: z.string().nullable(),
  teacherId: z.string().nullable(),
  teacherName: z.string().nullable(),
});
export type SectionSubjectItem = z.infer<typeof sectionSubjectItemSchema>;

export const sectionSubjectsResponseSchema = cursorPageSchema(sectionSubjectItemSchema);

// ---- POST /api/admin/sections/:sectionId/subjects/:subjectId/teacher ----

export const assignSectionSubjectTeacherRequestSchema = z.object({
  teacherId: z.uuid(),
});
export type AssignSectionSubjectTeacherRequest = z.infer<typeof assignSectionSubjectTeacherRequestSchema>;

export const assignSectionSubjectTeacherResponseSchema = sectionSubjectItemSchema;
export type AssignSectionSubjectTeacherResponse = z.infer<typeof assignSectionSubjectTeacherResponseSchema>;
