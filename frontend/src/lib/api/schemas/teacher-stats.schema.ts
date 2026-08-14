import { z } from "zod";
import { assignmentStatusSchema } from "./admin-assignments.schema";

// ---- GET /api/teacher/stats/overview ----

export const teacherRecentAssignmentSchema = z.object({
  id: z.string(),
  title: z.string(),
  sectionName: z.string().nullable(),
  gradeName: z.string().nullable(),
  subjectName: z.string().nullable(),
  deadline: z.string(),
  status: assignmentStatusSchema,
  submissionCount: z.number(),
  gradedCount: z.number(),
});
export type TeacherRecentAssignment = z.infer<typeof teacherRecentAssignmentSchema>;

export const teacherOverviewSchema = z.object({
  assignments: z.number(),
  drafts: z.number(),
  published: z.number(),
  awaitingGrading: z.number(),
  students: z.number(),
  recentAssignments: z.array(teacherRecentAssignmentSchema),
});
export type TeacherOverview = z.infer<typeof teacherOverviewSchema>;
