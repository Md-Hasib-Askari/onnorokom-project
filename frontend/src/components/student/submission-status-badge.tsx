"use client";

import { SubmissionStatus } from "@/lib/api/schemas/admin-assignments.schema";
import {
  NOT_SUBMITTED_LABEL,
  type StudentAssignmentListItem,
} from "@/lib/api/schemas/student.schema";
import { Badge } from "@/components/ui/badge";

type BadgeVariant = "default" | "secondary" | "outline" | "destructive";

const statusVariant: Record<SubmissionStatus, BadgeVariant> = {
  [SubmissionStatus.Graded]: "default",
  [SubmissionStatus.Submitted]: "secondary",
  [SubmissionStatus.Resubmitted]: "secondary",
  [SubmissionStatus.Returned]: "destructive",
};

/**
 * `Returned` is styled as destructive rather than muted because it is the one state that needs
 * the student to act again, and it is easy to mistake for "finished" otherwise.
 */
export function SubmissionStatusBadge({
  assignment,
}: {
  assignment: Pick<StudentAssignmentListItem, "submissionStatus" | "isLate">;
}) {
  const { submissionStatus, isLate } = assignment;

  return (
    <div className="flex flex-wrap items-center gap-2">
      {submissionStatus === null ? (
        <Badge variant="outline">{NOT_SUBMITTED_LABEL}</Badge>
      ) : (
        <Badge variant={statusVariant[submissionStatus]}>{submissionStatus}</Badge>
      )}
      {isLate ? <Badge variant="destructive">Late</Badge> : null}
    </div>
  );
}