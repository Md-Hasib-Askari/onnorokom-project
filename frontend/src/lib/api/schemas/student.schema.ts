import { z } from "zod";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { submissionStatusSchema } from "./admin-assignments.schema";

export const SUBMISSION_CONTENT_MAX_LENGTH = 10000;
export const ATTACHMENT_URL_MAX_LENGTH = 2000;

/**
 * Shown wherever a status badge would go but the student has not submitted at all, which the
 * API spells as a null `submissionStatus` rather than a fifth status.
 */
export const NOT_SUBMITTED_LABEL = "Not submitted";

// ---- GET /api/student/assignments ----

export const studentAssignmentListItemSchema = z.object({
  id: z.string(),
  title: z.string(),
  subjectName: z.string().nullable(),
  teacherName: z.string().nullable(),
  deadline: z.string(),
  maxMarks: z.number(),
  allowLateSubmission: z.boolean(),
  isPastDeadline: z.boolean(),
  submissionStatus: submissionStatusSchema.nullable(),
  isLate: z.boolean(),
  marks: z.number().nullable(),
});
export type StudentAssignmentListItem = z.infer<typeof studentAssignmentListItemSchema>;

export const studentAssignmentListSchema = z.array(studentAssignmentListItemSchema);

// ---- GET /api/student/assignments/:id ----

/**
 * `canSubmit` and `canEdit` come from the server rather than being re-derived here, so the form
 * can never offer a write the API is about to reject. They are mutually exclusive: submit is the
 * first attempt, edit is every one after it.
 */
export const studentAssignmentDetailSchema = studentAssignmentListItemSchema.extend({
  description: z.string().nullable(),
  feedback: z.string().nullable(),
  attachmentUrl: z.string().nullable(),
  content: z.string().nullable(),
  submittedAt: z.string().nullable(),
  canSubmit: z.boolean(),
  canEdit: z.boolean(),
});
export type StudentAssignmentDetail = z.infer<typeof studentAssignmentDetailSchema>;

// ---- POST/PUT /api/student/assignments/:id/submission ----

const WEB_PROTOCOLS = ["http:", "https:"];

/**
 * Mirrors the backend rule. Relative or exotic-scheme values are rejected outright because the
 * teacher's grading view renders this straight into an anchor.
 */
function isAbsoluteWebUrl(value: string): boolean {
  try {
    return WEB_PROTOCOLS.includes(new URL(value).protocol);
  } catch {
    return false;
  }
}

/**
 * One schema covers both submitting and editing: the API keeps a single row per student per
 * assignment, so the two writes carry identical fields and only differ in the verb.
 */
export const submissionRequestSchema = z
  .object({
    content: z.string().trim().max(SUBMISSION_CONTENT_MAX_LENGTH),
    attachmentUrl: z
      .string()
      .trim()
      .max(ATTACHMENT_URL_MAX_LENGTH)
      .refine(
        (value) => value === "" || isAbsoluteWebUrl(value),
        VALIDATION_MESSAGES.attachmentUrlInvalid
      ),
  })
  // Either field alone is a complete answer, so the rule sits on the pair. The message is pinned
  // to `content` because that is the field a student is looking at when they submit an empty form.
  .refine((values) => values.content !== "" || values.attachmentUrl !== "", {
    path: ["content"],
    error: VALIDATION_MESSAGES.submissionWorkRequired,
  });
export type SubmissionRequest = z.infer<typeof submissionRequestSchema>;