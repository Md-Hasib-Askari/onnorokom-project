"use client";

import Link from "next/link";

import type { TeacherSectionSubject } from "@/lib/api/schemas/teacher.schema";
import { EMPTY_CELL } from "@/lib/messages";
import { TeacherQueries } from "@/lib/queries/teacher.queries";
import { ASSIGNMENT_TARGET_PARAMS, ROUTES } from "@/lib/routes";
import { classLabel } from "@/lib/teacher-sections";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

/** Placeholder cards shown while the list loads. */
const SKELETON_CARD_COUNT = 3;

/**
 * Deep-links into the assignments page with the create dialog already pointed at this pair,
 * so a teacher never has to re-pick a class they just clicked on.
 */
function newAssignmentHref(pair: TeacherSectionSubject): string {
  const params = new URLSearchParams({
    [ASSIGNMENT_TARGET_PARAMS.sectionId]: pair.sectionId,
    [ASSIGNMENT_TARGET_PARAMS.subjectId]: pair.subjectId,
  });
  return `${ROUTES.teacherAssignments}?${params.toString()}`;
}

/** One row per section-subject pair the admin has assigned to this teacher. */
export function SectionSubjectsView() {
  const query = TeacherQueries.useSectionSubjects();

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">My subjects</h1>
        <p className="text-sm text-muted-foreground">
          The classes and subjects you have been assigned. Only an admin can change this list.
        </p>
      </div>

      {query.isLoading ? (
        <ListSkeleton />
      ) : query.isError ? (
        <p className="text-sm text-destructive">Failed to load your subjects.</p>
      ) : query.data && query.data.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {query.data.map((pair) => (
            <Card key={`${pair.sectionId}-${pair.subjectId}`}>
              <CardHeader>
                <CardTitle>{pair.subjectName ?? pair.subjectCode ?? EMPTY_CELL}</CardTitle>
                <CardDescription>{classLabel(pair)}</CardDescription>
                <CardAction>
                  <Button size="sm" variant="outline" asChild>
                    <Link href={newAssignmentHref(pair)}>New assignment</Link>
                  </Button>
                </CardAction>
              </CardHeader>
            </Card>
          ))}
        </div>
      ) : (
        <p className="text-sm text-muted-foreground">
          You have no subjects yet. An admin needs to assign you to a section and subject first.
        </p>
      )}
    </div>
  );
}

function ListSkeleton() {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
      {Array.from({ length: SKELETON_CARD_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-28 w-full" />
      ))}
    </div>
  );
}