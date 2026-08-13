"use client";

import { useState } from "react";
import Link from "next/link";
import { notFound, useRouter } from "next/navigation";
import { ArrowLeftIcon } from "lucide-react";

import { AssignmentStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { TeacherAssignment, TeacherSubmission } from "@/lib/api/schemas/teacher.schema";
import { isNotFoundError } from "@/lib/api/client";
import { formatDateTime, isPast } from "@/lib/datetime";
import { TeacherMutations } from "@/lib/mutations/teacher.mutations";
import { TeacherQueries } from "@/lib/queries/teacher.queries";
import { ROUTES } from "@/lib/routes";
import { classLabel } from "@/lib/teacher-sections";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { LoadMoreButton } from "@/components/workspace/load-more-button";
import { DeleteAssignmentDialog } from "./delete-assignment-dialog";
import { EditAssignmentDialog } from "./edit-assignment-dialog";
import { GradeSubmissionDialog } from "./grade-submission-dialog";
import { PublishAssignmentDialog } from "./publish-assignment-dialog";
import { ReturnSubmissionDialog } from "./return-submission-dialog";
import { UnpublishAssignmentDialog } from "./unpublish-assignment-dialog";
import { buildSubmissionColumns } from "./submission-columns";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function AssignmentDetailView({ assignmentId }: { assignmentId: string }) {
  const router = useRouter();
  const assignmentQuery = TeacherQueries.useAssignment(assignmentId);
  const submissionsQuery = TeacherQueries.useSubmissions(assignmentId);
  const publishMutation = TeacherMutations.usePublishAssignment();
  const unpublishMutation = TeacherMutations.useUnpublishAssignment();
  const closeSubmissionsMutation = TeacherMutations.useCloseSubmissions();
  const reopenSubmissionsMutation = TeacherMutations.useReopenSubmissions();
  const deleteMutation = TeacherMutations.useDeleteAssignment();
  const returnMutation = TeacherMutations.useReturnSubmission();

  const [isEditing, setIsEditing] = useState(false);
  const [isPublishing, setIsPublishing] = useState(false);
  const [isUnpublishing, setIsUnpublishing] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [gradingSubmission, setGradingSubmission] = useState<TeacherSubmission | null>(null);
  const [returningSubmission, setReturningSubmission] = useState<TeacherSubmission | null>(null);

  const assignment = assignmentQuery.data;
  const submissions = submissionsQuery.data?.pages.flatMap((page) => page.items) ?? [];

  if (assignmentQuery.isLoading) return <DetailSkeleton />;
  if (assignmentQuery.isError) {
    if (isNotFoundError(assignmentQuery.error)) notFound();
    return (
      <ErrorState
        description="Failed to load this assignment."
        retry={assignmentQuery.refetch}
      />
    );
  }
  if (!assignment) return <DetailSkeleton />;

  const isDraft = assignment.status === AssignmentStatus.Draft;
  const columns = buildSubmissionColumns({
    maxMarks: assignment.maxMarks,
    onGrade: setGradingSubmission,
    onReturn: setReturningSubmission,
  });

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link href={ROUTES.teacherAssignments}>
          <ArrowLeftIcon className="size-4" />
          All assignments
        </Link>
      </Button>

      <AssignmentHeader
        assignment={assignment}
        isDraft={isDraft}
        onEdit={() => setIsEditing(true)}
        onPublish={() => setIsPublishing(true)}
        onUnpublish={() => setIsUnpublishing(true)}
        onDelete={() => setIsDeleting(true)}
        onToggleSubmissions={(open) =>
          open
            ? reopenSubmissionsMutation.mutate(assignment.id)
            : closeSubmissionsMutation.mutate(assignment.id)
        }
        isTogglingSubmissions={closeSubmissionsMutation.isPending || reopenSubmissionsMutation.isPending}
      />

      <div className="space-y-3">
        <h2 className="text-lg font-semibold tracking-tight">Submissions</h2>
        {submissionsQuery.isLoading ? (
          <TableSkeleton />
        ) : submissionsQuery.isError ? (
          <ErrorState
            compact
            description="Failed to load submissions."
            retry={submissionsQuery.refetch}
          />
        ) : (
          <>
            <DataTable
              columns={columns}
              data={submissions}
              emptyMessage={
                isDraft ? "Publish this assignment to start collecting work." : "No submissions yet."
              }
            />
            {submissionsQuery.hasNextPage && (
              <LoadMoreButton
                onClick={() => submissionsQuery.fetchNextPage()}
                isLoading={submissionsQuery.isFetchingNextPage}
                label="Load more submissions"
              />
            )}
          </>
        )}
      </div>

      <EditAssignmentDialog
        assignment={isEditing ? assignment : null}
        onOpenChange={(open) => !open && setIsEditing(false)}
      />
      <PublishAssignmentDialog
        assignment={isPublishing ? assignment : null}
        onOpenChange={(open) => !open && setIsPublishing(false)}
        mutation={publishMutation}
      />
      <UnpublishAssignmentDialog
        assignment={isUnpublishing ? assignment : null}
        onOpenChange={(open) => !open && setIsUnpublishing(false)}
        mutation={unpublishMutation}
      />
      <DeleteAssignmentDialog
        assignment={isDeleting ? assignment : null}
        onOpenChange={(open) => !open && setIsDeleting(false)}
        mutation={deleteMutation}
        onDeleted={() => router.push(ROUTES.teacherAssignments)}
      />
      <GradeSubmissionDialog
        submission={gradingSubmission}
        assignmentId={assignmentId}
        maxMarks={assignment.maxMarks}
        onOpenChange={(open) => !open && setGradingSubmission(null)}
      />
      <ReturnSubmissionDialog
        submission={returningSubmission}
        assignmentId={assignmentId}
        onOpenChange={(open) => !open && setReturningSubmission(null)}
        mutation={returnMutation}
      />
    </div>
  );
}

function AssignmentHeader({
  assignment,
  isDraft,
  onEdit,
  onPublish,
  onUnpublish,
  onDelete,
  onToggleSubmissions,
  isTogglingSubmissions,
}: {
  assignment: TeacherAssignment;
  isDraft: boolean;
  onEdit: () => void;
  onPublish: () => void;
  onUnpublish: () => void;
  onDelete: () => void;
  onToggleSubmissions: (open: boolean) => void;
  isTogglingSubmissions: boolean;
}) {
  return (
    <div className="space-y-4 rounded-lg border p-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant={isDraft ? "secondary" : "default"}>{assignment.status}</Badge>
            <Badge variant="outline">{classLabel(assignment)}</Badge>
            {assignment.subjectName ? (
              <Badge variant="outline">{assignment.subjectName}</Badge>
            ) : null}
            {isPast(assignment.deadline) ? <Badge variant="outline">Deadline passed</Badge> : null}
          </div>
          <h1 className="text-2xl font-semibold tracking-tight">{assignment.title}</h1>
        </div>
        <div className="flex flex-wrap items-center gap-4">
          <label className="flex items-center gap-2 text-sm">
            <span className="text-muted-foreground">Accepting submissions</span>
            <Switch
              checked={assignment.submissionsOpen}
              disabled={isTogglingSubmissions}
              onCheckedChange={onToggleSubmissions}
            />
          </label>
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" size="sm" onClick={onEdit}>
              Edit
            </Button>
            {isDraft ? (
              <Button size="sm" onClick={onPublish}>
                Publish
              </Button>
            ) : (
              <Button variant="outline" size="sm" onClick={onUnpublish}>
                Unpublish
              </Button>
            )}
            <Button variant="outline" size="sm" onClick={onDelete}>
              Delete
            </Button>
          </div>
        </div>
      </div>

      {assignment.description ? (
        <p className="text-sm whitespace-pre-wrap text-muted-foreground">
          {assignment.description}
        </p>
      ) : null}

      <dl className="grid gap-4 text-sm sm:grid-cols-3">
        <Fact label="Deadline" value={formatDateTime(assignment.deadline)} />
        <Fact label="Total marks" value={String(assignment.maxMarks)} />
        <Fact
          label="Late submission"
          value={assignment.allowLateSubmission ? "Allowed" : "Not allowed"}
        />
        <Fact
          label="Submissions"
          value={assignment.submissionsOpen ? "Open" : "Closed"}
        />
        <Fact
          label="Graded"
          value={`${assignment.gradedCount} of ${assignment.submissionCount}`}
        />
      </dl>
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
      <Skeleton className="h-40 w-full" />
      <TableSkeleton />
    </div>
  );
}

function TableSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: SKELETON_ROW_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-10 w-full" />
      ))}
    </div>
  );
}