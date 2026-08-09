"use client";

import { useQuery } from "@tanstack/react-query";
import { listGradesAction } from "@/lib/actions/admin-users.actions";
import type { GradeSummary } from "@/lib/api/schemas/grades.schema";

export const adminGradesKeys = {
  all: ["admin", "grades"] as const,
  list: () => [...adminGradesKeys.all, "list"] as const,
};

const gradeNameCollator = new Intl.Collator(undefined, { numeric: true });

function sortByName(grades: GradeSummary[]): GradeSummary[] {
  return [...grades].sort((a, b) => gradeNameCollator.compare(a.name, b.name));
}

function selectCurrentYear(grades: GradeSummary[]): GradeSummary[] {
  const latestYear = grades.reduce<string | undefined>(
    (latest, grade) => (!latest || grade.academicYear > latest ? grade.academicYear : latest),
    undefined
  );
  return sortByName(grades.filter((grade) => grade.academicYear === latestYear));
}

/** Grouped under one namespace so every admin-grade query is defined in a single place. */
export const AdminGradeQueries = {
  useList() {
    return useQuery({
      queryKey: adminGradesKeys.list(),
      queryFn: () => listGradesAction(),
      select: sortByName,
    });
  },
  /** Grades for the most recent academic year only, used by the student create/edit
   *  dialogs so admins aren't picking a grade out of every past year at once. */
  useCurrentYearList() {
    return useQuery({
      queryKey: adminGradesKeys.list(),
      queryFn: () => listGradesAction(),
      select: selectCurrentYear,
    });
  },
};
