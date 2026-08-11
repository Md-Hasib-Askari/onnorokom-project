"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createAssignmentAction,
  deleteAssignmentAction,
  gradeSubmissionAction,
  publishAssignmentAction,
  returnSubmissionAction,
  updateAssignmentAction,
} from "@/lib/actions/teacher.actions";
import type {
  AssignmentCreateRequest,
  AssignmentUpdateRequest,
  GradeSubmissionRequest,
} from "@/lib/api/schemas/teacher.schema";
import { teacherKeys } from "@/lib/queries/teacher.queries";

/** Grouped under one namespace so every teacher mutation is defined in a single place. */
export const TeacherMutations = {
  useCreateAssignment() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: AssignmentCreateRequest) => createAssignmentAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: teacherKeys.assignments() });
        }
      },
    });
  },

  useUpdateAssignment() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ id, values }: { id: string; values: AssignmentUpdateRequest }) =>
        updateAssignmentAction(id, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: teacherKeys.assignments() });
        }
      },
    });
  },

  usePublishAssignment() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (id: string) => publishAssignmentAction(id),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: teacherKeys.assignments() });
        }
      },
    });
  },

  useDeleteAssignment() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (id: string) => deleteAssignmentAction(id),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: teacherKeys.assignments() });
        }
      },
    });
  },

  useGradeSubmission() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({
        assignmentId,
        submissionId,
        values,
      }: {
        assignmentId: string;
        submissionId: string;
        values: GradeSubmissionRequest;
      }) => gradeSubmissionAction(assignmentId, submissionId, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: teacherKeys.assignments() });
        }
      },
    });
  },

  useReturnSubmission() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ assignmentId, submissionId }: { assignmentId: string; submissionId: string }) =>
        returnSubmissionAction(assignmentId, submissionId),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: teacherKeys.assignments() });
        }
      },
    });
  },
};