"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  assignSectionSubjectTeacherAction,
  createSectionAction,
  deleteSectionAction,
  unassignSectionSubjectTeacherAction,
  updateSectionAction,
} from "@/lib/actions/admin-sections.actions";
import type { SectionCreateRequest, SectionUpdateRequest } from "@/lib/api/schemas/sections.schema";
import { adminSectionsKeys } from "@/lib/queries/admin-sections.queries";

/** Grouped under one namespace so every admin-section mutation is defined in a single place. */
export const AdminSectionMutations = {
  useCreate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: SectionCreateRequest) => createSectionAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSectionsKeys.all });
        }
      },
    });
  },

  useUpdate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ id, values }: { id: string; values: SectionUpdateRequest }) =>
        updateSectionAction(id, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSectionsKeys.all });
        }
      },
    });
  },

  useDelete() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (id: string) => deleteSectionAction(id),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSectionsKeys.all });
        }
      },
    });
  },

  useAssignSubjectTeacher() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({
        sectionId,
        subjectId,
        teacherId,
      }: {
        sectionId: string;
        subjectId: string;
        teacherId: string;
      }) => assignSectionSubjectTeacherAction(sectionId, subjectId, teacherId),
      onSuccess: (result, variables) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSectionsKeys.subjects(variables.sectionId) });
        }
      },
    });
  },

  useUnassignSubjectTeacher() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ sectionId, subjectId }: { sectionId: string; subjectId: string }) =>
        unassignSectionSubjectTeacherAction(sectionId, subjectId),
      onSuccess: (result, variables) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSectionsKeys.subjects(variables.sectionId) });
        }
      },
    });
  },
};