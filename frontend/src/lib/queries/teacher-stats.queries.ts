"use client";

import { useQuery } from "@tanstack/react-query";
import { getTeacherOverviewAction } from "@/lib/actions/teacher-stats.actions";

export const teacherStatsKeys = {
  all: ["teacher", "stats"] as const,
  overview: () => [...teacherStatsKeys.all, "overview"] as const,
};

/** Counts backing the teacher overview page, refreshed after every assignment mutation. */
export function useTeacherOverview() {
  return useQuery({
    queryKey: teacherStatsKeys.overview(),
    queryFn: () => getTeacherOverviewAction(),
  });
}
