"use client";

import Link from "next/link";

import type { AdminRecentPending } from "@/lib/api/schemas/admin-stats.schema";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { formatDateTime } from "@/lib/datetime";
import { useAdminOverview } from "@/lib/queries/admin-stats.queries";
import { ROUTES } from "@/lib/routes";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/workspace/error-state";

/** One tile per stat below. */
const SKELETON_TILE_COUNT = 4;

/**
 * Backed by the dedicated stats endpoint, so the counts are real totals across the whole system,
 * not a sum over the first page of each paginated list.
 */
export function AdminOverview({ fullName }: { fullName: string }) {
  const stats = useAdminOverview();

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Welcome, {fullName}</h1>
          <p className="text-sm text-muted-foreground">
            A quick look at users, academic structure, and assignment activity across the system.
          </p>
        </div>
        <Button asChild>
          <Link href={ROUTES.adminUsers}>Go to users</Link>
        </Button>
      </div>

      {stats.isLoading ? (
        <StatsSkeleton />
      ) : stats.isError ? (
        <ErrorState description="Failed to load your overview." retry={stats.refetch} />
      ) : (
        <>
          <Section title="Users" description="Accounts by role, and who is waiting on a decision.">
            <UserStatGrid
              students={stats.data!.students}
              teachers={stats.data!.teachers}
              admins={stats.data!.admins}
              pending={stats.data!.pending}
            />
          </Section>

          <Section
            title="Pending approvals"
            description="The most recently registered accounts awaiting review."
          >
            <PendingApprovalsCard recent={stats.data!.recentPending} />
          </Section>

          <Section
            title="Academic structure"
            description="Grades, sections, and subjects configured for the school."
          >
            <AcademicStatGrid
              grades={stats.data!.grades}
              sections={stats.data!.sections}
              subjects={stats.data!.subjects}
            />
          </Section>

          <Section
            title="Assignments"
            description="Work set by teachers, and how submissions are progressing."
          >
            <AssignmentStatGrid data={stats.data!} />
          </Section>
        </>
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

function UserStatGrid({
  students,
  teachers,
  admins,
  pending,
}: {
  students: number;
  teachers: number;
  admins: number;
  pending: number;
}) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <Stat label="Students" value={students} hint="Approved and active accounts." />
      <Stat label="Teachers" value={teachers} hint="Approved and active accounts." />
      <Stat label="Admins" value={admins} hint="Accounts with admin access." />
      <Stat label="Pending approvals" value={pending} hint="Waiting on a decision." />
    </div>
  );
}

function AcademicStatGrid({
  grades,
  sections,
  subjects,
}: {
  grades: number;
  sections: number;
  subjects: number;
}) {
  return (
    <div className="grid gap-4 sm:grid-cols-3">
      <Stat label="Grades" value={grades} hint="Defined for the school." />
      <Stat label="Sections" value={sections} hint="Across all grades." />
      <Stat label="Subjects" value={subjects} hint="Available to assign." />
    </div>
  );
}

function AssignmentStatGrid({
  data,
}: {
  data: {
    assignments: number;
    drafts: number;
    published: number;
    submissions: number;
    graded: number;
    ungraded: number;
  };
}) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-6">
      <Stat label="Assignments" value={data.assignments} hint="Created across all teachers." />
      <Stat label="Drafts" value={data.drafts} hint="Not visible to students yet." />
      <Stat label="Published" value={data.published} hint="Live for their classes." />
      <Stat label="Submissions" value={data.submissions} hint="Handed in by students." />
      <Stat label="Graded" value={data.graded} hint="Marks and feedback are ready." />
      <Stat label="Ungraded" value={data.ungraded} hint="Still waiting on a teacher." />
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

function PendingApprovalsCard({ recent }: { recent: AdminRecentPending[] }) {
  return (
    <Card size="sm">
      <CardContent>
        {recent.length === 0 ? (
          <p className="text-sm text-muted-foreground">Nothing is waiting on you right now.</p>
        ) : (
          <ul className="divide-y">
            {recent.map((user) => (
              <li
                key={user.id}
                className="flex flex-wrap items-center justify-between gap-2 py-3 first:pt-0 last:pb-0"
              >
                <div className="space-y-1">
                  <p className="font-medium">{user.fullName}</p>
                  <p className="text-sm text-muted-foreground">
                    Registered {formatDateTime(user.createdAt)}
                  </p>
                </div>
                <Badge variant={user.role === UserRole.Student ? "secondary" : "outline"}>
                  {user.role}
                </Badge>
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
