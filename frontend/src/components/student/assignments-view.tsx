"use client";

import { StudentQueries } from "@/lib/queries/student.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { LoadMoreButton } from "@/components/workspace/load-more-button";
import { buildStudentAssignmentColumns } from "./assignment-columns";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function StudentAssignmentsView() {
  const query = StudentQueries.useAssignments();
  const columns = buildStudentAssignmentColumns();
  const assignments = query.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
        <p className="text-sm text-muted-foreground">
          Everything your teachers have published for your section.
        </p>
      </div>

      {query.isLoading ? (
        <TableSkeleton />
      ) : query.isError ? (
        <ErrorState description="Failed to load your assignments." retry={query.refetch} />
      ) : (
        <>
          <DataTable
            columns={columns}
            data={assignments}
            emptyMessage="No assignments have been published for your section yet."
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