"use client";

import Link from "next/link";

import { SubmissionStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { StudentAssignmentListItem } from "@/lib/api/schemas/student.schema";
import { formatDateTime } from "@/lib/datetime";
import { StudentQueries } from "@/lib/queries/student.queries";
import { ROUTES, ROUTE_BUILDERS } from "@/lib/routes";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/workspace/error-state";

/** One tile per stat below. */
const SKELETON_TILE_COUNT = 4;

/** Enough to answer "what should I do next" without turning the overview into a second list. */
const DUE_SOON_LIMIT = 3;

interface OverviewStats {
  total: number;
  toDo: number;
  awaitingGrade: number;
  graded: number;
}

/** Nothing submitted yet, or handed back by the teacher: either way the ball is with the student. */
function needsWork(assignment: StudentAssignmentListItem): boolean {
  return (
    assignment.submissionStatus === null ||
    assignment.submissionStatus === SubmissionStatus.Returned
  );
}

/** Matches the server's rule for whether a write will still be accepted. */
function isOpen(assignment: StudentAssignmentListItem): boolean {
  return assignment.submissionsOpen && (!assignment.isPastDeadline || assignment.allowLateSubmission);
}

/**
 * Derived on the client from the list the student already has, so the overview costs no extra
 * request. The list is now cursor-paginated, so these figures cover only the pages loaded so far
 * (preview semantics). A dedicated stats endpoint can replace this if the overview ever needs to
 * reflect the whole list.
 */
function summarise(assignments: StudentAssignmentListItem[]): OverviewStats {
  let toDo = 0;
  let awaitingGrade = 0;
  let graded = 0;
  for (const assignment of assignments) {
    if (needsWork(assignment)) {
      toDo += 1;
    } else if (assignment.submissionStatus === SubmissionStatus.Graded) {
      graded += 1;
    } else {
      awaitingGrade += 1;
    }
  }
  return { total: assignments.length, toDo, awaitingGrade, graded };
}

function dueSoon(assignments: StudentAssignmentListItem[]): StudentAssignmentListItem[] {
  return assignments
    .filter((assignment) => needsWork(assignment) && isOpen(assignment))
    .sort((a, b) => Date.parse(a.deadline) - Date.parse(b.deadline))
    .slice(0, DUE_SOON_LIMIT);
}

export function StudentOverview({ fullName }: { fullName: string }) {
  const query = StudentQueries.useAssignments();
  const assignments = query.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Welcome, {fullName}</h1>
          <p className="text-sm text-muted-foreground">
            What is due, what you are waiting on, and what has come back marked.
          </p>
        </div>
        <Button asChild>
          <Link href={ROUTES.studentAssignments}>Go to assignments</Link>
        </Button>
      </div>

      {query.isLoading ? (
        <StatsSkeleton />
      ) : query.isError ? (
        <ErrorState description="Failed to load your assignments." retry={query.refetch} />
      ) : (
        <Section
          title="Assignments"
          description="What is due, what you are waiting on, and what has come back marked."
        >
          <StatGrid stats={summarise(assignments)} />
          <DueSoonCard assignments={dueSoon(assignments)} />
        </Section>
      )}
    </div>
  );
}

function Section({
  title,
  description,
  children,
}: {
  title: string;
  description: string;
  children: React.ReactNode;
}) {
  return (
    <section className="space-y-4">
      <div className="space-y-1">
        <h2 className="text-lg font-semibold tracking-tight">{title}</h2>
        <p className="text-sm text-muted-foreground">{description}</p>
      </div>
      {children}
    </section>
  );
}

function StatGrid({ stats }: { stats: OverviewStats }) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <Stat label="Assignments" value={stats.total} hint="Published for your section." />
      <Stat label="To do" value={stats.toDo} hint="Not submitted, or sent back to you." />
      <Stat label="Awaiting grade" value={stats.awaitingGrade} hint="Handed in, not marked yet." />
      <Stat label="Graded" value={stats.graded} hint="Marks and feedback are ready." />
    </div>
  );
}

function Stat({ label, value, hint }: { label: string; value: number; hint: string }) {
  return (
    <Card size="sm">
      <CardHeader>
        <CardDescription>{label}</CardDescription>
        <CardTitle className="text-3xl font-semibold tabular-nums">{value}</CardTitle>
        <CardDescription>{hint}</CardDescription>
      </CardHeader>
    </Card>
  );
}

function DueSoonCard({ assignments }: { assignments: StudentAssignmentListItem[] }) {
  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle>Due next</CardTitle>
        <CardDescription>The soonest deadlines still open to you.</CardDescription>
      </CardHeader>
      <CardContent>
        {assignments.length === 0 ? (
          <p className="text-sm text-muted-foreground">Nothing is waiting on you right now.</p>
        ) : (
          <ul className="divide-y">
            {assignments.map((assignment) => (
              <li
                key={assignment.id}
                className="flex flex-wrap items-center justify-between gap-2 py-3 first:pt-0 last:pb-0"
              >
                <div className="space-y-1">
                  <Link
                    href={ROUTE_BUILDERS.studentAssignment(assignment.id)}
                    className="font-medium underline-offset-4 hover:underline"
                  >
                    {assignment.title}
                  </Link>
                  <p className="text-sm text-muted-foreground">
                    Due {formatDateTime(assignment.deadline)}
                  </p>
                </div>
                {assignment.isPastDeadline ? <Badge variant="destructive">Overdue</Badge> : null}
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}

function StatsSkeleton() {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      {Array.from({ length: SKELETON_TILE_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-28 w-full" />
      ))}
    </div>
  );
}