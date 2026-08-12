"use client";

import { AdminAssignmentQueries } from "@/lib/queries/admin-assignments.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { LoadMoreButton } from "@/components/workspace/load-more-button";
import { buildAssignmentColumns } from "./assignment-columns";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function AssignmentsView() {
  const query = AdminAssignmentQueries.useList();
  const columns = buildAssignmentColumns();
  const assignments = query.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
        <p className="text-sm text-muted-foreground">
          Read-only view of every assignment across the system, with how many students have
          answered each one.
        </p>
      </div>

      {query.isLoading ? (
        <TableSkeleton />
      ) : query.isError ? (
        <ErrorState description="Failed to load assignments." retry={query.refetch} />
      ) : (
        <>
          <DataTable
            columns={columns}
            data={assignments}
            emptyMessage="No assignments yet."
          />
          {query.hasNextPage && (
            <LoadMoreButton
              onClick={() => query.fetchNextPage()}
              isLoading={query.isFetchingNextPage}
              label="Load more assignments"
            />
          )}
        </>
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