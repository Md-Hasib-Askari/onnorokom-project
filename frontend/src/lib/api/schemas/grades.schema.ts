import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { cursorPageSchema } from "./common.schema";

// ---- GET /api/admin/grades ----

export const gradeSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  academicYear: z.string(),
  description: z.string().nullable(),
  teacherCount: z.number(),
  studentCount: z.number(),
});
export type GradeSummary = z.infer<typeof gradeSummarySchema>;

export const gradeListResponseSchema = cursorPageSchema(gradeSummarySchema);

// ---- POST /api/admin/grades ----

export const gradeCreateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.gradeNameRequired).max(100),
  academicYear: z.string().trim().min(1, VALIDATION_MESSAGES.academicYearRequired).max(20),
  description: z.string().trim().max(500).optional(),
});
export type GradeCreateRequest = z.infer<typeof gradeCreateRequestSchema>;

export const gradeCreateResponseSchema = gradeSummarySchema;
export type GradeCreateResponse = z.infer<typeof gradeCreateResponseSchema>;

// ---- PUT /api/admin/grades/:id ----

export const gradeUpdateRequestSchema = z.object({
  name: z.string().trim().min(1, VALIDATION_MESSAGES.gradeNameRequired).max(100),
  academicYear: z.string().trim().min(1, VALIDATION_MESSAGES.academicYearRequired).max(20),
  description: z.string().trim().max(500).optional(),
});
export type GradeUpdateRequest = z.infer<typeof gradeUpdateRequestSchema>;

export const gradeUpdateResponseSchema = gradeSummarySchema;
export type GradeUpdateResponse = z.infer<typeof gradeUpdateResponseSchema>;
