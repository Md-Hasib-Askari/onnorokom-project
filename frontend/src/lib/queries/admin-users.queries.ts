"use client";

import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import {
  getUserDetailAction,
  listUsersAction,
} from "@/lib/actions/admin-users.actions";
import type { AdminUserListParams } from "@/lib/api/admin-users.api";

export const adminUsersKeys = {
  all: ["admin", "users"] as const,
  list: (filters?: AdminUserListParams) =>
    [...adminUsersKeys.all, "list", serializeFilters(filters)] as const,
  detail: (id: string) => [...adminUsersKeys.all, "detail", id] as const,
};

/**
 * Filters are part of the query key so each tab's server-side filtered list caches separately;
 * the cursor is deliberately absent so `invalidateQueries` on the `adminUsersKeys.all` prefix
 * resets every loaded page after a mutation.
 */
function serializeFilters(filters?: AdminUserListParams): string {
  if (!filters) return "all";
  return [filters.status ?? "all", filters.role ?? "all", filters.limit ?? "default"].join(":");
}

/** Grouped under one namespace so every admin-user query is defined in a single place. */
export const AdminUserQueries = {
  useList(filters?: AdminUserListParams) {
    return useInfiniteQuery({
      queryKey: adminUsersKeys.list(filters),
      queryFn: ({ pageParam }) =>
        listUsersAction({ ...filters, cursor: pageParam ?? undefined }),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    });
  },

  useDetail(id: string | undefined) {
    return useQuery({
      queryKey: adminUsersKeys.detail(id ?? ""),
      queryFn: () => getUserDetailAction(id!),
      enabled: !!id,
    });
  },
};
