"use client";

import { AdminAssignmentQueries } from "@/lib/queries/admin-assignments.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { buildSubmissionColumns } from "./submission-columns";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function SubmissionsView() {
  const query = AdminAssignmentQueries.useSubmissions();
  const columns = buildSubmissionColumns();

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Submissions</h1>
        <p className="text-sm text-muted-foreground">
          Read-only view of every submission across the system. Grading stays with the teacher who
          set the assignment.
        </p>
      </div>

      {query.isLoading ? (
        <TableSkeleton />
      ) : query.isError ? (
        <p className="text-sm text-destructive">Failed to load submissions.</p>
      ) : (
        <DataTable
          columns={columns}
          data={query.data ?? []}
          emptyMessage="No submissions yet."
        />
      )}
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