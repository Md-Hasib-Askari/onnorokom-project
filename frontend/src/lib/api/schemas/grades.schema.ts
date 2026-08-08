import { z } from "zod";

// ---- GET /api/admin/grades ----

export const gradeSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  academicYear: z.string(),
  description: z.string().nullable(),
});
export type GradeSummary = z.infer<typeof gradeSummarySchema>;

export const gradeListResponseSchema = z.array(gradeSummarySchema);
