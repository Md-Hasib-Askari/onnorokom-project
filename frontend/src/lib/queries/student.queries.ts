"use client";

import { useQuery } from "@tanstack/react-query";
import { getMyAssignmentAction, listMyAssignmentsAction } from "@/lib/actions/student.actions";

export const studentKeys = {
  all: ["student"] as const,
  assignments: () => [...studentKeys.all, "assignments"] as const,
  assignmentList: () => [...studentKeys.assignments(), "list"] as const,
  assignment: (id: string) => [...studentKeys.assignments(), id] as const,
};

/** Grouped under one namespace so every student query is defined in a single place. */
export const StudentQueries = {
  useAssignments() {
    return useQuery({
      queryKey: studentKeys.assignmentList(),
      queryFn: () => listMyAssignmentsAction(),
    });
  },

  useAssignment(id: string) {
    return useQuery({
      queryKey: studentKeys.assignment(id),
      queryFn: () => getMyAssignmentAction(id),
    });
  },
};