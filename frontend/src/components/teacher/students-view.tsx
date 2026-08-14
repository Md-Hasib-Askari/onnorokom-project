"use client";

import { TeacherQueries } from "@/lib/queries/teacher.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { LoadMoreButton } from "@/components/workspace/load-more-button";
import { studentColumns } from "./student-columns";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function StudentsView() {
  const studentsQuery = TeacherQueries.useStudents();
  const students = studentsQuery.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Students</h1>
        <p className="text-sm text-muted-foreground">
          Who is enrolled in the sections you teach. Every student appears once, however many
          subjects you share with them.
        </p>
      </div>

      {studentsQuery.isLoading ? (
        <TableSkeleton />
      ) : studentsQuery.isError ? (
        <ErrorState description="Failed to load students." retry={studentsQuery.refetch} />
      ) : (
        <>
          <DataTable
            columns={studentColumns}
            data={students}
            emptyMessage="No students in your sections yet."
          />
          {studentsQuery.hasNextPage && (
            <LoadMoreButton
              onClick={() => studentsQuery.fetchNextPage()}
              isLoading={studentsQuery.isFetchingNextPage}
              label="Load more students"
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
