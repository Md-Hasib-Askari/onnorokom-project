"use client";

import type { ColumnDef } from "@tanstack/react-table";

import {
  AssignmentStatus,
  type AssignmentListItem,
} from "@/lib/api/schemas/admin-assignments.schema";
import { Badge } from "@/components/ui/badge";

const statusVariant: Record<AssignmentListItem["status"], "default" | "secondary"> = {
  [AssignmentStatus.Published]: "default",
  [AssignmentStatus.Draft]: "secondary",
};

export function buildAssignmentColumns(): ColumnDef<AssignmentListItem>[] {
  return [
    {
      accessorKey: "title",
      header: "Title",
    },
    {
      accessorKey: "subjectName",
      header: "Subject",
      cell: ({ row }) => row.original.subjectName ?? "—",
    },
    {
      accessorKey: "gradeName",
      header: "Grade",
      cell: ({ row }) => row.original.gradeName ?? "—",
    },
    {
      accessorKey: "teacherName",
      header: "Teacher",
      cell: ({ row }) => row.original.teacherName ?? "—",
    },
    {
      accessorKey: "deadline",
      header: "Deadline",
      cell: ({ row }) => new Date(row.original.deadline).toLocaleString(),
    },
    {
      accessorKey: "maxMarks",
      header: "Max marks",
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => (
        <Badge variant={statusVariant[row.original.status]}>{row.original.status}</Badge>
      ),
    },
    {
      accessorKey: "allowLateSubmission",
      header: "Late submission",
      cell: ({ row }) => (
        <Badge variant="outline">{row.original.allowLateSubmission ? "Allowed" : "Not allowed"}</Badge>
      ),
    },
  ];
}