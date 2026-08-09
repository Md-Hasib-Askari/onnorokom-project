"use client";

import { useState } from "react";
import { toast } from "sonner";

import type { SubjectSummary } from "@/lib/api/schemas/admin-subjects.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSubjectQueries } from "@/lib/queries/admin-subjects.queries";
import { AdminSubjectMutations } from "@/lib/mutations/admin-subjects.mutations";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/admin/data-table";
import { buildSubjectColumns } from "./subject-columns";
import { CreateSubjectDialog } from "./create-subject-dialog";
import { EditSubjectDialog } from "./edit-subject-dialog";
import { DeleteSubjectDialog } from "./delete-subject-dialog";
import { AssignTeacherDialog } from "./assign-teacher-dialog";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function SubjectsView() {
  const subjectsQuery = AdminSubjectQueries.useList();
  const deleteMutation = AdminSubjectMutations.useDelete();
  const unassignTeacherMutation = AdminSubjectMutations.useUnassignTeacher();

  const [editingSubject, setEditingSubject] = useState<SubjectSummary | null>(null);
  const [deletingSubject, setDeletingSubject] = useState<SubjectSummary | null>(null);
  const [assigningTeacherSubject, setAssigningTeacherSubject] = useState<SubjectSummary | null>(null);

  function handleUnassignTeacher(subject: SubjectSummary) {
    unassignTeacherMutation.mutate(subject.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success(`Teacher unassigned from ${subject.name}.`);
        } else {
          toast.error(result.error ?? ERROR_MESSAGES.generic);
        }
      },
    });
  }

  const columns = buildSubjectColumns({
    onEdit: setEditingSubject,
    onDelete: setDeletingSubject,
    onAssignTeacher: setAssigningTeacherSubject,
    onUnassignTeacher: handleUnassignTeacher,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Subjects</h1>
          <p className="text-sm text-muted-foreground">
            Manage subjects, their grade, and their assigned teacher.
          </p>
        </div>
        <CreateSubjectDialog />
      </div>

      {subjectsQuery.isLoading ? (
        <TableSkeleton />
      ) : subjectsQuery.isError ? (
        <p className="text-sm text-destructive">Failed to load subjects.</p>
      ) : (
        <DataTable columns={columns} data={subjectsQuery.data ?? []} emptyMessage="No subjects yet." />
      )}

      <EditSubjectDialog
        subject={editingSubject}
        onOpenChange={(open) => !open && setEditingSubject(null)}
      />
      <DeleteSubjectDialog
        subject={deletingSubject}
        onOpenChange={(open) => !open && setDeletingSubject(null)}
        mutation={deleteMutation}
      />
      <AssignTeacherDialog
        subject={assigningTeacherSubject}
        onOpenChange={(open) => !open && setAssigningTeacherSubject(null)}
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