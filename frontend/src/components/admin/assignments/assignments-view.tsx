"use client";

import { AdminAssignmentQueries } from "@/lib/queries/admin-assignments.queries";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/admin/data-table";
import { buildAssignmentColumns } from "./assignment-columns";
import { buildSubmissionColumns } from "./submission-columns";

/** Tab identifiers for the assignments screen. */
const ASSIGNMENT_TAB = {
  assignments: "assignments",
  submissions: "submissions",
} as const;

/** Placeholder rows shown while a table loads. */
const SKELETON_ROW_COUNT = 5;

export function AssignmentsView() {
  const assignmentsQuery = AdminAssignmentQueries.useList();
  const submissionsQuery = AdminAssignmentQueries.useSubmissions();

  const assignmentColumns = buildAssignmentColumns();
  const submissionColumns = buildSubmissionColumns();

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
        <p className="text-sm text-muted-foreground">
          Read-only view of every assignment and submission across the system.
        </p>
      </div>

      <Tabs defaultValue={ASSIGNMENT_TAB.assignments}>
        <TabsList className="bg-muted/60">
          <TabsTrigger value={ASSIGNMENT_TAB.assignments}>Assignments</TabsTrigger>
          <TabsTrigger value={ASSIGNMENT_TAB.submissions}>Submissions</TabsTrigger>
        </TabsList>
        <TabsContent value={ASSIGNMENT_TAB.assignments} className="space-y-4">
          {assignmentsQuery.isLoading ? (
            <TableSkeleton />
          ) : assignmentsQuery.isError ? (
            <p className="text-sm text-destructive">Failed to load assignments.</p>
          ) : (
            <DataTable
              columns={assignmentColumns}
              data={assignmentsQuery.data ?? []}
              emptyMessage="No assignments yet."
            />
          )}
        </TabsContent>
        <TabsContent value={ASSIGNMENT_TAB.submissions} className="space-y-4">
          {submissionsQuery.isLoading ? (
            <TableSkeleton />
          ) : submissionsQuery.isError ? (
            <p className="text-sm text-destructive">Failed to load submissions.</p>
          ) : (
            <DataTable
              columns={submissionColumns}
              data={submissionsQuery.data ?? []}
              emptyMessage="No submissions yet."
            />
          )}
        </TabsContent>
      </Tabs>
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