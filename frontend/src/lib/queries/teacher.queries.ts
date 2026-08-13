"use client";

import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import {
  getAssignmentAction,
  listAssignmentsAction,
  listSectionSubjectsAction,
  listStudentsAction,
  listSubmissionsAction,
} from "@/lib/actions/teacher.actions";

export const teacherKeys = {
  all: ["teacher"] as const,
  sectionSubjects: () => [...teacherKeys.all, "section-subjects"] as const,
  students: () => [...teacherKeys.all, "students"] as const,
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

  useStudents() {
    return useInfiniteQuery({
      queryKey: teacherKeys.students(),
      queryFn: ({ pageParam }) =>
        listStudentsAction(pageParam ? { cursor: pageParam } : {}),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    });
  },

  useAssignments() {
    return useInfiniteQuery({
      queryKey: teacherKeys.assignmentList(),
      queryFn: ({ pageParam }) =>
        listAssignmentsAction(pageParam ? { cursor: pageParam } : {}),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    });
  },

  useAssignment(id: string) {
    return useQuery({
      queryKey: teacherKeys.assignment(id),
      queryFn: () => getAssignmentAction(id),
    });
  },

  useSubmissions(assignmentId: string) {
    return useInfiniteQuery({
      queryKey: teacherKeys.submissions(assignmentId),
      queryFn: ({ pageParam }) =>
        listSubmissionsAction(assignmentId, pageParam ? { cursor: pageParam } : {}),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    });
  },
};