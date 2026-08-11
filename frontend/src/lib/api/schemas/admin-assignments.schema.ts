import { z } from "zod";

// ---- GET /api/admin/assignments ----

export const assignmentStatusSchema = z.enum(["Draft", "Published"]);
export type AssignmentStatus = z.infer<typeof assignmentStatusSchema>;
export const AssignmentStatus = assignmentStatusSchema.enum;

export const assignmentListItemSchema = z.object({
  id: z.string(),
  title: z.string(),
  description: z.string().nullable(),
  sectionId: z.string(),
  sectionName: z.string().nullable(),
  subjectId: z.string(),
  subjectName: z.string().nullable(),
  gradeName: z.string().nullable(),
  teacherId: z.string(),
  teacherName: z.string().nullable(),
  deadline: z.string(),
  maxMarks: z.number(),
  status: assignmentStatusSchema,
  allowLateSubmission: z.boolean(),
  submissionCount: z.number(),
});
export type AssignmentListItem = z.infer<typeof assignmentListItemSchema>;

export const adminAssignmentListResponseSchema = z.array(assignmentListItemSchema);

// ---- GET /api/admin/submissions ----

export const submissionStatusSchema = z.enum([
  "Submitted",
  "Resubmitted",
  "Returned",
  "Graded",
]);
export type SubmissionStatus = z.infer<typeof submissionStatusSchema>;
export const SubmissionStatus = submissionStatusSchema.enum;

export const submissionListItemSchema = z.object({
  id: z.string(),
  assignmentId: z.string(),
  assignmentTitle: z.string().nullable(),
  studentId: z.string(),
  studentName: z.string().nullable(),
  content: z.string().nullable(),
  attachmentUrl: z.string().nullable(),
  status: submissionStatusSchema,
  marks: z.number().nullable(),
  feedback: z.string().nullable(),
  submittedAt: z.string(),
});
export type SubmissionListItem = z.infer<typeof submissionListItemSchema>;

export const adminSubmissionListResponseSchema = z.array(submissionListItemSchema);