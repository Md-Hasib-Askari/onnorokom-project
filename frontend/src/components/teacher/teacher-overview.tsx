"use client";

import Link from "next/link";

import { AssignmentStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { TeacherRecentAssignment } from "@/lib/api/schemas/teacher-stats.schema";
import { formatDateTime } from "@/lib/datetime";
import { useTeacherOverview } from "@/lib/queries/teacher-stats.queries";
import { ROUTES } from "@/lib/routes";
import { classLabel } from "@/lib/teacher-sections";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/workspace/error-state";

/** One tile per stat below. */
const SKELETON_TILE_COUNT = 4;

interface OverviewStats {
  total: number;
  drafts: number;
  published: number;
  ungraded: number;
}

/**
 * Backed by the dedicated stats endpoint, so the counts are real totals across everything the
 * teacher has set, not a sum over the first page of the paginated list.
 */
export function TeacherOverview({ fullName }: { fullName: string }) {
  const stats = useTeacherOverview();

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Welcome, {fullName}</h1>
          <p className="text-sm text-muted-foreground">
            A quick look at the work you have set and what is still waiting on you.
          </p>
        </div>
        <Button asChild>
          <Link href={ROUTES.teacherAssignments}>Go to assignments</Link>
        </Button>
      </div>

      {stats.isLoading ? (
        <StatsSkeleton />
      ) : stats.isError ? (
        <ErrorState description="Failed to load your overview." retry={stats.refetch} />
      ) : (
        <>
          <Section title="Assignments" description="Everything you have set, by status.">
            <StatGrid stats={toStats(stats.data!)} />
          </Section>

          <Section title="Students" description="Who is enrolled in the classes you teach.">
            <StudentsCard count={stats.data!.students} />
          </Section>

          <Section
            title="Recently set"
            description="Your newest assignments, and how their submissions are going."
          >
            <RecentAssignmentsCard assignments={stats.data!.recentAssignments} />
          </Section>
        </>
      )}
    </div>
  );
}

function toStats(data: {
  assignments: number;
  drafts: number;
  published: number;
  awaitingGrading: number;
}): OverviewStats {
  return {
    total: data.assignments,
    drafts: data.drafts,
    published: data.published,
    ungraded: data.awaitingGrading,
  };
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
      <Stat label="Assignments" value={stats.total} hint="Everything you have created." />
      <Stat label="Drafts" value={stats.drafts} hint="Not visible to students yet." />
      <Stat label="Published" value={stats.published} hint="Live for your classes." />
      <Stat label="Awaiting grading" value={stats.ungraded} hint="Submissions with no mark yet." />
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

function StudentsCard({ count }: { count: number }) {
  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle>Your students</CardTitle>
        <CardDescription>
          {count} student{count === 1 ? "" : "s"} across your classes.
        </CardDescription>
      </CardHeader>
      <CardFooter>
        <Button asChild variant="outline" size="sm">
          <Link href={ROUTES.teacherStudents}>Go to students</Link>
        </Button>
      </CardFooter>
    </Card>
  );
}

function RecentAssignmentsCard({ assignments }: { assignments: TeacherRecentAssignment[] }) {
  return (
    <Card size="sm">
      <CardContent>
        {assignments.length === 0 ? (
          <p className="text-sm text-muted-foreground">You have not set anything yet.</p>
        ) : (
          <ul className="divide-y">
            {assignments.map((assignment) => (
              <li
                key={assignment.id}
                className="flex flex-wrap items-center justify-between gap-2 py-3 first:pt-0 last:pb-0"
              >
                <div className="space-y-1">
                  <p className="font-medium">{assignment.title}</p>
                  <p className="text-sm text-muted-foreground">
                    {classLabel(assignment)}. Due {formatDateTime(assignment.deadline)}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm tabular-nums text-muted-foreground">
                    {assignment.gradedCount}/{assignment.submissionCount} graded
                  </span>
                  <Badge variant={assignment.status === AssignmentStatus.Draft ? "secondary" : "default"}>
                    {assignment.status}
                  </Badge>
                </div>
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
