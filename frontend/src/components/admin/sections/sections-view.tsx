"use client";

import { useState } from "react";

import type { SectionSummary } from "@/lib/api/schemas/sections.schema";
import { AdminSectionQueries } from "@/lib/queries/admin-sections.queries";
import { AdminSectionMutations } from "@/lib/mutations/admin-sections.mutations";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { buildSectionColumns } from "./section-columns";
import { CreateSectionDialog } from "./create-section-dialog";
import { EditSectionDialog } from "./edit-section-dialog";
import { DeleteSectionDialog } from "./delete-section-dialog";
import { SectionSubjectsDialog } from "./section-subjects-dialog";

/** Placeholder rows shown while the table loads. */
const SKELETON_ROW_COUNT = 5;

export function SectionsView() {
  const sectionsQuery = AdminSectionQueries.useList();
  const deleteMutation = AdminSectionMutations.useDelete();

  const [editingSection, setEditingSection] = useState<SectionSummary | null>(null);
  const [deletingSection, setDeletingSection] = useState<SectionSummary | null>(null);
  const [managingSubjectsSection, setManagingSubjectsSection] = useState<SectionSummary | null>(null);

  const columns = buildSectionColumns({
    onEdit: setEditingSection,
    onDelete: setDeletingSection,
    onManageSubjects: setManagingSubjectsSection,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Sections</h1>
          <p className="text-sm text-muted-foreground">
            Manage the classes within each grade and assign a teacher per subject, per section.
          </p>
        </div>
        <CreateSectionDialog />
      </div>

      {sectionsQuery.isLoading ? (
        <TableSkeleton />
      ) : sectionsQuery.isError ? (
        <p className="text-sm text-destructive">Failed to load sections.</p>
      ) : (
        <DataTable columns={columns} data={sectionsQuery.data ?? []} emptyMessage="No sections yet." />
      )}

      <EditSectionDialog section={editingSection} onOpenChange={(open) => !open && setEditingSection(null)} />
      <DeleteSectionDialog
        section={deletingSection}
        onOpenChange={(open) => !open && setDeletingSection(null)}
        mutation={deleteMutation}
      />
      <SectionSubjectsDialog
        section={managingSubjectsSection}
        onOpenChange={(open) => !open && setManagingSubjectsSection(null)}
      />
    </div>
  );
}

function TableSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: SKELETON_ROW_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-10 w-full" />
      ))}
    </div>
  );
}