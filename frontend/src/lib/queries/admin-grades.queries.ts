"use client";

import { useQuery } from "@tanstack/react-query";
import { listGradesAction } from "@/lib/actions/admin-users.actions";

export const adminGradesKeys = {
  all: ["admin", "grades"] as const,
  list: () => [...adminGradesKeys.all, "list"] as const,
};

/** Grouped under one namespace so every admin-grade query is defined in a single place. */
export const AdminGradeQueries = {
  useList() {
    return useQuery({
      queryKey: adminGradesKeys.list(),
      queryFn: () => listGradesAction(),
    });
  },
};
