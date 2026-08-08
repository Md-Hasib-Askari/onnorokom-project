"use client";

import { useQuery } from "@tanstack/react-query";
import { listPendingUsersAction, listUsersAction } from "@/lib/actions/admin-users.actions";

export const adminUsersKeys = {
  all: ["admin", "users"] as const,
  list: () => [...adminUsersKeys.all, "list"] as const,
  pending: () => [...adminUsersKeys.all, "pending"] as const,
};

/** Grouped under one namespace so every admin-user query is defined in a single place. */
export const AdminUserQueries = {
  useList() {
    return useQuery({
      queryKey: adminUsersKeys.list(),
      queryFn: () => listUsersAction(),
    });
  },

  usePending() {
    return useQuery({
      queryKey: adminUsersKeys.pending(),
      queryFn: () => listPendingUsersAction(),
    });
  },
};
