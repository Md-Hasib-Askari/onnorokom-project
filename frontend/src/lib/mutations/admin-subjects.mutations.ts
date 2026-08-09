"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  assignTeacherAction,
  createSubjectAction,
  deleteSubjectAction,
  unassignTeacherAction,
  updateSubjectAction,
} from "@/lib/actions/admin-subjects.actions";
import type {
  SubjectCreateRequest,
  SubjectUpdateRequest,
} from "@/lib/api/schemas/admin-subjects.schema";
import { adminSubjectsKeys } from "@/lib/queries/admin-subjects.queries";

/** Grouped under one namespace so every admin-subject mutation is defined in a single place. */
export const AdminSubjectMutations = {
  useCreate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: SubjectCreateRequest) => createSubjectAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSubjectsKeys.all });
        }
      },
    });
  },

  useUpdate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ id, values }: { id: string; values: SubjectUpdateRequest }) =>
        updateSubjectAction(id, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSubjectsKeys.all });
        }
      },
    });
  },

  useDelete() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (id: string) => deleteSubjectAction(id),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSubjectsKeys.all });
        }
      },
    });
  },

  useAssignTeacher() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ subjectId, teacherId }: { subjectId: string; teacherId: string }) =>
        assignTeacherAction(subjectId, teacherId),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSubjectsKeys.all });
        }
      },
    });
  },

  useUnassignTeacher() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (subjectId: string) => unassignTeacherAction(subjectId),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSubjectsKeys.all });
        }
      },
    });
  },
};