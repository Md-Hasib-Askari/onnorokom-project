"use client";

import { useQuery } from "@tanstack/react-query";
import { listAssignmentsAction, listSubmissionsAction } from "@/lib/actions/admin-assignments.actions";

export const adminAssignmentsKeys = {
  all: ["admin", "assignments"] as const,
  list: () => [...adminAssignmentsKeys.all, "list"] as const,
  submissions: () => [...adminAssignmentsKeys.all, "submissions"] as const,
};

/** Grouped under one namespace so every admin-assignment query is defined in a single place. */
export const AdminAssignmentQueries = {
  useList() {
    return useQuery({
      queryKey: adminAssignmentsKeys.list(),
      queryFn: () => listAssignmentsAction(),
    });
  },

  useSubmissions() {
    return useQuery({
      queryKey: adminAssignmentsKeys.submissions(),
      queryFn: () => listSubmissionsAction(),
    });
  },
};