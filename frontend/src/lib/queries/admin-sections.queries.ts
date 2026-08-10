"use client";

import { useQuery } from "@tanstack/react-query";
import {
  getSectionSubjectsAction,
  listSectionsAction,
} from "@/lib/actions/admin-sections.actions";
import type { SectionSummary } from "@/lib/api/schemas/sections.schema";

export const adminSectionsKeys = {
  all: ["admin", "sections"] as const,
  list: () => [...adminSectionsKeys.all, "list"] as const,
  subjects: (sectionId: string) => [...adminSectionsKeys.all, "subjects", sectionId] as const,
};

const sectionNameCollator = new Intl.Collator(undefined, { numeric: true });

function sortByName(sections: SectionSummary[]): SectionSummary[] {
  return [...sections].sort((a, b) => sectionNameCollator.compare(a.name, b.name));
}

/** Grouped under one namespace so every admin-section query is defined in a single place. */
export const AdminSectionQueries = {
  useList() {
    return useQuery({
      queryKey: adminSectionsKeys.list(),
      queryFn: () => listSectionsAction(),
      select: sortByName,
    });
  },
  /** Sections for a single grade, used by the student create/edit dialogs' cascading picker. */
  useByGrade(gradeId: string | undefined) {
    return useQuery({
      queryKey: adminSectionsKeys.list(),
      queryFn: () => listSectionsAction(),
      select: (sections) => sortByName(sections.filter((section) => section.gradeId === gradeId)),
      enabled: !!gradeId,
    });
  },
  useSectionSubjects(sectionId: string | undefined) {
    return useQuery({
      queryKey: adminSectionsKeys.subjects(sectionId ?? ""),
      queryFn: () => getSectionSubjectsAction(sectionId!),
      enabled: !!sectionId,
    });
  },
};