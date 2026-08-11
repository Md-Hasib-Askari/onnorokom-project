"use client";

import Link from "next/link";
import { ArrowLeftIcon } from "lucide-react";

import { SubmissionStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { StudentAssignmentDetail } from "@/lib/api/schemas/student.schema";
import { formatDateTime } from "@/lib/datetime";
import { StudentQueries } from "@/lib/queries/student.queries";
import { ROUTES } from "@/lib/routes";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { SubmissionForm } from "./submission-form";
import { SubmissionStatusBadge } from "./submission-status-badge";

export function StudentAssignmentDetailView({ assignmentId }: { assignmentId: string }) {
  const query = StudentQueries.useAssignment(assignmentId);
  const assignment = query.data;

  if (query.isLoading) return <DetailSkeleton />;
  if (query.isError || !assignment) {
    return <p className="text-sm text-destructive">Failed to load this assignment.</p>;
  }

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link href={ROUTES.studentAssignments}>
          <ArrowLeftIcon className="size-4" />
          All assignments
        </Link>
      </Button>

      <AssignmentHeader assignment={assignment} />

      {assignment.submissionStatus === SubmissionStatus.Graded ? (
        <GradePanel assignment={assignment} />
      ) : null}

      <SubmissionForm assignment={assignment} />
    </div>
  );
}

function AssignmentHeader({ assignment }: { assignment: StudentAssignmentDetail }) {
  return (
    <div className="space-y-4 rounded-lg border p-6">
      <div className="space-y-2">
        <div className="flex flex-wrap items-center gap-2">
          <SubmissionStatusBadge assignment={assignment} />
          {assignment.subjectName ? (
            <Badge variant="outline">{assignment.subjectName}</Badge>
          ) : null}
          {assignment.isPastDeadline ? <Badge variant="outline">Deadline passed</Badge> : null}
        </div>
        <h1 className="text-2xl font-semibold tracking-tight">{assignment.title}</h1>
      </div>

      {assignment.description ? (
        <p className="text-sm whitespace-pre-wrap text-muted-foreground">{assignment.description}</p>
      ) : null}

      <dl className="grid gap-4 text-sm sm:grid-cols-3">
        <Fact label="Deadline" value={formatDateTime(assignment.deadline)} />
        <Fact label="Max marks" value={String(assignment.maxMarks)} />
        <Fact
          label="Late submission"
          value={assignment.allowLateSubmission ? "Allowed" : "Not allowed"}
        />
        <Fact label="Teacher" value={assignment.teacherName ?? "Not assigned"} />
        {assignment.submittedAt ? (
          <Fact label="Last submitted" value={formatDateTime(assignment.submittedAt)} />
        ) : null}
      </dl>
    </div>
  );
}

/**
 * Only rendered once the work is graded. Returning it for revision clears the marks and feedback
 * server-side, so there is no stale grade left to show alongside a reopened form.
 */
function GradePanel({ assignment }: { assignment: StudentAssignmentDetail }) {
  return (
    <div className="space-y-3 rounded-lg border p-6">
      <h2 className="text-lg font-semibold tracking-tight">Your result</h2>
      <p className="text-3xl font-semibold tabular-nums">
        {assignment.marks}
        <span className="text-lg font-normal text-muted-foreground"> / {assignment.maxMarks}</span>
      </p>
      <div className="space-y-1">
        <p className="text-sm font-medium">Feedback</p>
        <p className="text-sm whitespace-pre-wrap text-muted-foreground">
          {assignment.feedback?.trim() ? assignment.feedback : "Your teacher left no feedback."}
        </p>
      </div>
    </div>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div className="space-y-1">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="font-medium">{value}</dd>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-48 w-full" />
      <Skeleton className="h-64 w-full" />
    </div>
  );
}