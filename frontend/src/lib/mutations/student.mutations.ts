"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { submitAssignmentAction, updateSubmissionAction } from "@/lib/actions/student.actions";
import type { SubmissionRequest } from "@/lib/api/schemas/student.schema";
import { studentKeys } from "@/lib/queries/student.queries";

interface SubmissionVariables {
  assignmentId: string;
  values: SubmissionRequest;
}

/** Grouped under one namespace so every student mutation is defined in a single place. */
export const StudentMutations = {
  useSubmitAssignment() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ assignmentId, values }: SubmissionVariables) =>
        submitAssignmentAction(assignmentId, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: studentKeys.assignments() });
        }
      },
    });
  },

  useUpdateSubmission() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ assignmentId, values }: SubmissionVariables) =>
        updateSubmissionAction(assignmentId, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: studentKeys.assignments() });
        }
      },
    });
  },
};