"use client";

import { useQuery } from "@tanstack/react-query";
import { listSubjectsAction } from "@/lib/actions/admin-subjects.actions";

export const adminSubjectsKeys = {
  all: ["admin", "subjects"] as const,
  list: () => [...adminSubjectsKeys.all, "list"] as const,
};

/** Grouped under one namespace so every admin-subject query is defined in a single place. */
export const AdminSubjectQueries = {
  useList() {
    return useQuery({
      queryKey: adminSubjectsKeys.list(),
      queryFn: () => listSubjectsAction(),
    });
  },
};