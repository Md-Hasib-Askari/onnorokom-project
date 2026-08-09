"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createGradeAction,
  deleteGradeAction,
  updateGradeAction,
} from "@/lib/actions/admin-grades.actions";
import type { GradeCreateRequest, GradeUpdateRequest } from "@/lib/api/schemas/grades.schema";
import { adminGradesKeys } from "@/lib/queries/admin-grades.queries";

/** Grouped under one namespace so every admin-grade mutation is defined in a single place. */
export const AdminGradeMutations = {
  useCreate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: GradeCreateRequest) => createGradeAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminGradesKeys.all });
        }
      },
    });
  },

  useUpdate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ id, values }: { id: string; values: GradeUpdateRequest }) =>
        updateGradeAction(id, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminGradesKeys.all });
        }
      },
    });
  },

  useDelete() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (id: string) => deleteGradeAction(id),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminGradesKeys.all });
        }
      },
    });
  },
};