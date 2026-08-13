"use client";

import Link from "next/link";

import { AssignmentStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { TeacherAssignment } from "@/lib/api/schemas/teacher.schema";
import { TeacherQueries } from "@/lib/queries/teacher.queries";
import { ROUTES } from "@/lib/routes";
import { Button } from "@/components/ui/button";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

/** One tile per stat below. */
const SKELETON_TILE_COUNT = 4;

interface OverviewStats {
  total: number;
  drafts: number;
  published: number;
  ungraded: number;
}

/**
 * Derived on the client from the assignment list the teacher already has, so the overview
 * costs no extra request. A dedicated stats endpoint can replace this if the list ever pages.
 */
function summarise(assignments: TeacherAssignment[]): OverviewStats {
  let drafts = 0;
  let published = 0;
  let ungraded = 0;
  for (const assignment of assignments) {
    if (assignment.status === AssignmentStatus.Draft) {
      drafts += 1;
      continue;
    }
    published += 1;
    ungraded += assignment.submissionCount - assignment.gradedCount;
  }
  return { total: assignments.length, drafts, published, ungraded };
}

export function TeacherOverview({ fullName }: { fullName: string }) {
  const query = TeacherQueries.useAssignments();

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

      {query.isLoading ? (
        <StatsSkeleton />
      ) : query.isError ? (
        <p className="text-sm text-destructive">Failed to load your assignments.</p>
      ) : (
        <StatGrid stats={summarise(query.data?.pages.flatMap((page) => page.items) ?? [])} />
      )}
    </div>
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

function StatsSkeleton() {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      {Array.from({ length: SKELETON_TILE_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-28 w-full" />
      ))}
    </div>
  );
}