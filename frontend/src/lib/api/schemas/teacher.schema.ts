import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { assignmentStatusSchema, submissionStatusSchema } from "./admin-assignments.schema";

export const ASSIGNMENT_TITLE_MAX_LENGTH = 200;
export const ASSIGNMENT_DESCRIPTION_MAX_LENGTH = 2000;
export const FEEDBACK_MAX_LENGTH = 2000;

// ---- GET /api/teacher/section-subjects ----

export const teacherSectionSubjectSchema = z.object({
  sectionId: z.string(),
  sectionName: z.string().nullable(),
  gradeId: z.string(),
  gradeName: z.string().nullable(),
  subjectId: z.string(),
  subjectName: z.string().nullable(),
  subjectCode: z.string().nullable(),
});
export type TeacherSectionSubject = z.infer<typeof teacherSectionSubjectSchema>;

export const teacherSectionSubjectListSchema = z.array(teacherSectionSubjectSchema);

// ---- GET /api/teacher/assignments ----

export const teacherAssignmentSchema = z.object({
  id: z.string(),
  title: z.string(),
  description: z.string().nullable(),
  sectionId: z.string(),
  sectionName: z.string().nullable(),
  gradeName: z.string().nullable(),
  subjectId: z.string(),
  subjectName: z.string().nullable(),
  deadline: z.string(),
  maxMarks: z.number(),
  status: assignmentStatusSchema,
  allowLateSubmission: z.boolean(),
  submissionCount: z.number(),
  gradedCount: z.number(),
});
export type TeacherAssignment = z.infer<typeof teacherAssignmentSchema>;

export const teacherAssignmentListSchema = z.array(teacherAssignmentSchema);

// ---- GET /api/teacher/assignments/:id/submissions ----

export const teacherSubmissionSchema = z.object({
  id: z.string(),
  studentId: z.string(),
  studentName: z.string().nullable(),
  rollNumber: z.string().nullable(),
  content: z.string().nullable(),
  attachmentUrl: z.string().nullable(),
  status: submissionStatusSchema,
  isLate: z.boolean(),
  marks: z.number().nullable(),
  feedback: z.string().nullable(),
  submittedAt: z.string(),
  gradedAt: z.string().nullable(),
});
export type TeacherSubmission = z.infer<typeof teacherSubmissionSchema>;

export const teacherSubmissionListSchema = z.array(teacherSubmissionSchema);

// ---- POST/PUT /api/teacher/assignments ----
// `deadline` is an ISO instant here. The form works in the browser's local time and converts
// on submit, so the wire format never depends on how the input widget spells a date.

const deadlineSchema = z
  .string()
  .min(1, VALIDATION_MESSAGES.deadlineRequired)
  .refine((value) => !Number.isNaN(Date.parse(value)), VALIDATION_MESSAGES.deadlineInvalid);

/**
 * Only creation demands a future deadline, matching the backend: editing has to let a teacher
 * correct a deadline that has already passed, or shorten one to close an assignment early.
 */
const futureDeadlineSchema = deadlineSchema.refine(
  (value) => Date.parse(value) > Date.now(),
  VALIDATION_MESSAGES.deadlineMustBeFuture
);

const assignmentFieldsSchema = z.object({
  title: z
    .string()
    .trim()
    .min(1, VALIDATION_MESSAGES.assignmentTitleRequired)
    .max(ASSIGNMENT_TITLE_MAX_LENGTH),
  description: z.string().trim().max(ASSIGNMENT_DESCRIPTION_MAX_LENGTH).optional(),
  deadline: deadlineSchema,
  // A cleared number input reads back as undefined, so the schema-level message covers that too.
  maxMarks: z
    .number({ error: VALIDATION_MESSAGES.maxMarksPositive })
    .positive(VALIDATION_MESSAGES.maxMarksPositive),
  allowLateSubmission: z.boolean(),
});

export const assignmentCreateRequestSchema = assignmentFieldsSchema.extend({
  sectionId: z.string().min(1, VALIDATION_MESSAGES.sectionRequired).pipe(z.uuid()),
  subjectId: z.string().min(1, VALIDATION_MESSAGES.subjectRequired).pipe(z.uuid()),
  deadline: futureDeadlineSchema,
});
export type AssignmentCreateRequest = z.infer<typeof assignmentCreateRequestSchema>;

/**
 * Section and subject are absent by design: they fix the assignment's audience at creation,
 * and the backend rejects any attempt to move it afterwards.
 */
export const assignmentUpdateRequestSchema = assignmentFieldsSchema;
export type AssignmentUpdateRequest = z.infer<typeof assignmentUpdateRequestSchema>;

// ---- PUT /api/teacher/submissions/:id/grade ----

export const gradeSubmissionRequestSchema = z.object({
  marks: z
    .number({ error: VALIDATION_MESSAGES.marksNotNegative })
    .min(0, VALIDATION_MESSAGES.marksNotNegative),
  feedback: z.string().trim().max(FEEDBACK_MAX_LENGTH).optional(),
});
export type GradeSubmissionRequest = z.infer<typeof gradeSubmissionRequestSchema>;

/**
 * The upper bound lives on the parent assignment, which the request never carries, so the grade
 * form builds its own schema. The backend enforces the same ceiling in the service layer.
 */
export function buildGradeSubmissionSchema(maxMarks: number) {
  return gradeSubmissionRequestSchema.refine((values) => values.marks <= maxMarks, {
    path: ["marks"],
    error: VALIDATION_MESSAGES.marksAboveMax(maxMarks),
  });
}