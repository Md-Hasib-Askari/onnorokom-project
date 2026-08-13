"use client";

import { useState } from "react";

import type { GradeSummary } from "@/lib/api/schemas/grades.schema";
import { AdminGradeQueries } from "@/lib/queries/admin-grades.queries";
import { AdminGradeMutations } from "@/lib/mutations/admin-grades.mutations";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { buildGradeColumns } from "./grade-columns";
import { CreateGradeDialog } from "./create-grade-dialog";
import { EditGradeDialog } from "./edit-grade-dialog";
import { DeleteGradeDialog } from "./delete-grade-dialog";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function GradesView() {
  const gradesQuery = AdminGradeQueries.useList();
  const deleteMutation = AdminGradeMutations.useDelete();

  const [editingGrade, setEditingGrade] = useState<GradeSummary | null>(null);
  const [deletingGrade, setDeletingGrade] = useState<GradeSummary | null>(null);

  const columns = buildGradeColumns({
    onEdit: setEditingGrade,
    onDelete: setDeletingGrade,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Grades</h1>
          <p className="text-sm text-muted-foreground">
            Manage the academic-year grades subjects and students belong to.
          </p>
        </div>
        <CreateGradeDialog />
      </div>

      {gradesQuery.isLoading ? (
        <TableSkeleton />
      ) : gradesQuery.isError ? (
        <ErrorState description="Failed to load grades." retry={gradesQuery.refetch} />
      ) : (
        <DataTable columns={columns} data={gradesQuery.data ?? []} emptyMessage="No grades yet." />
      )}

      <EditGradeDialog grade={editingGrade} onOpenChange={(open) => !open && setEditingGrade(null)} />
      <DeleteGradeDialog
        grade={deletingGrade}
        onOpenChange={(open) => !open && setDeletingGrade(null)}
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