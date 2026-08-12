"use client";

import { useInfiniteQuery } from "@tanstack/react-query";
import {
  listAssignmentsAction,
  listSubmissionsAction,
} from "@/lib/actions/admin-assignments.actions";

export const adminAssignmentsKeys = {
  all: ["admin", "assignments"] as const,
  list: () => [...adminAssignmentsKeys.all, "list"] as const,
  submissions: () => [...adminAssignmentsKeys.all, "submissions"] as const,
};

/** Grouped under one namespace so every admin-assignment query is defined in a single place. */
export const AdminAssignmentQueries = {
  useList() {
    return useInfiniteQuery({
      queryKey: adminAssignmentsKeys.list(),
      queryFn: ({ pageParam }) => listAssignmentsAction(pageParam ? { cursor: pageParam } : {}),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    });
  },

  useSubmissions() {
    return useInfiniteQuery({
      queryKey: adminAssignmentsKeys.submissions(),
      queryFn: ({ pageParam }) => listSubmissionsAction(pageParam ? { cursor: pageParam } : {}),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    });
  },
};