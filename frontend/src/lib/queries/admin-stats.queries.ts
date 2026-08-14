"use client";

import { useQuery } from "@tanstack/react-query";
import { getAdminOverviewAction } from "@/lib/actions/admin-stats.actions";

export const adminStatsKeys = {
  all: ["admin", "stats"] as const,
  overview: () => [...adminStatsKeys.all, "overview"] as const,
};

/** Counts backing the admin overview page, refreshed after every user mutation. */
export function useAdminOverview() {
  return useQuery({
    queryKey: adminStatsKeys.overview(),
    queryFn: () => getAdminOverviewAction(),
  });
}
