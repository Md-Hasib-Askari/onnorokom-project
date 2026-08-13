"use client";

import { useState } from "react";

import type { TeacherAssignment } from "@/lib/api/schemas/teacher.schema";
import { TeacherMutations } from "@/lib/mutations/teacher.mutations";
import { TeacherQueries } from "@/lib/queries/teacher.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { LoadMoreButton } from "@/components/workspace/load-more-button";
import { buildAssignmentColumns } from "./assignment-columns";
import { CreateAssignmentDialog, type AssignmentTarget } from "./create-assignment-dialog";
import { DeleteAssignmentDialog } from "./delete-assignment-dialog";
import { EditAssignmentDialog } from "./edit-assignment-dialog";
import { PublishAssignmentDialog } from "./publish-assignment-dialog";
import { UnpublishAssignmentDialog } from "./unpublish-assignment-dialog";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

interface AssignmentsViewProps {
  /** Present when the teacher arrived from "My subjects", which opens the create dialog prefilled. */
  initialTarget?: AssignmentTarget;
}

export function AssignmentsView({ initialTarget }: AssignmentsViewProps) {
  const assignmentsQuery = TeacherQueries.useAssignments();
  const publishMutation = TeacherMutations.usePublishAssignment();
  const unpublishMutation = TeacherMutations.useUnpublishAssignment();
  const closeSubmissionsMutation = TeacherMutations.useCloseSubmissions();
  const reopenSubmissionsMutation = TeacherMutations.useReopenSubmissions();
  const deleteMutation = TeacherMutations.useDeleteAssignment();
  const assignments = assignmentsQuery.data?.pages.flatMap((page) => page.items) ?? [];

  const [createOpen, setCreateOpen] = useState(Boolean(initialTarget));
  const [editingAssignment, setEditingAssignment] = useState<TeacherAssignment | null>(null);
  const [publishingAssignment, setPublishingAssignment] = useState<TeacherAssignment | null>(null);
  const [unpublishingAssignment, setUnpublishingAssignment] = useState<TeacherAssignment | null>(
    null
  );
  const [deletingAssignment, setDeletingAssignment] = useState<TeacherAssignment | null>(null);

  const columns = buildAssignmentColumns({
    onEdit: setEditingAssignment,
    onPublish: setPublishingAssignment,
    onUnpublish: setUnpublishingAssignment,
    onCloseSubmissions: (assignment) => closeSubmissionsMutation.mutate(assignment.id),
    onReopenSubmissions: (assignment) => reopenSubmissionsMutation.mutate(assignment.id),
    onDelete: setDeletingAssignment,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
          <p className="text-sm text-muted-foreground">
            Everything you have set for your classes. Drafts stay hidden until you publish them.
          </p>
        </div>
        <CreateAssignmentDialog
          open={createOpen}
          onOpenChange={setCreateOpen}
          defaultTarget={initialTarget}
        />
      </div>

      {assignmentsQuery.isLoading ? (
        <TableSkeleton />
      ) : assignmentsQuery.isError ? (
        <ErrorState
          description="Failed to load assignments."
          retry={assignmentsQuery.refetch}
        />
      ) : (
        <>
          <DataTable
            columns={columns}
            data={assignments}
            emptyMessage="No assignments yet."
          />
          {assignmentsQuery.hasNextPage && (
            <LoadMoreButton
              onClick={() => assignmentsQuery.fetchNextPage()}
              isLoading={assignmentsQuery.isFetchingNextPage}
              label="Load more assignments"
            />
          )}
        </>
      )}

      <EditAssignmentDialog
        assignment={editingAssignment}
        onOpenChange={(open) => !open && setEditingAssignment(null)}
      />
      <PublishAssignmentDialog
        assignment={publishingAssignment}
        onOpenChange={(open) => !open && setPublishingAssignment(null)}
        mutation={publishMutation}
      />
      <UnpublishAssignmentDialog
        assignment={unpublishingAssignment}
        onOpenChange={(open) => !open && setUnpublishingAssignment(null)}
        mutation={unpublishMutation}
      />
      <DeleteAssignmentDialog
        assignment={deletingAssignment}
        onOpenChange={(open) => !open && setDeletingAssignment(null)}
        mutation={deleteMutation}
      />
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