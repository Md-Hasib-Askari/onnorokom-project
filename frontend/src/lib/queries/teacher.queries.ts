"use client";

import { useQuery } from "@tanstack/react-query";
import {
  getAssignmentAction,
  listAssignmentsAction,
  listSectionSubjectsAction,
  listSubmissionsAction,
} from "@/lib/actions/teacher.actions";

export const teacherKeys = {
  all: ["teacher"] as const,
  sectionSubjects: () => [...teacherKeys.all, "section-subjects"] as const,
  assignments: () => [...teacherKeys.all, "assignments"] as const,
  assignmentList: () => [...teacherKeys.assignments(), "list"] as const,
  assignment: (id: string) => [...teacherKeys.assignments(), id] as const,
  submissions: (assignmentId: string) => [...teacherKeys.assignment(assignmentId), "submissions"] as const,
};

/** Grouped under one namespace so every teacher query is defined in a single place. */
export const TeacherQueries = {
  useSectionSubjects() {
    return useQuery({
      queryKey: teacherKeys.sectionSubjects(),
      queryFn: () => listSectionSubjectsAction(),
    });
  },

  useAssignments() {
    return useQuery({
      queryKey: teacherKeys.assignmentList(),
      queryFn: () => listAssignmentsAction(),
    });
  },

  useAssignment(id: string) {
    return useQuery({
      queryKey: teacherKeys.assignment(id),
      queryFn: () => getAssignmentAction(id),
    });
  },

  useSubmissions(assignmentId: string) {
    return useQuery({
      queryKey: teacherKeys.submissions(assignmentId),
      queryFn: () => listSubmissionsAction(assignmentId),
    });
  },
};