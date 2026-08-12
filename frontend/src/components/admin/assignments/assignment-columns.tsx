"use client";

import type { ColumnDef } from "@tanstack/react-table";

import {
  AssignmentStatus,
  type AssignmentListItem,
} from "@/lib/api/schemas/admin-assignments.schema";
import { EMPTY_CELL } from "@/lib/messages";
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
      cell: ({ row }) => row.original.subjectName ?? EMPTY_CELL,
    },
    {
      accessorKey: "gradeName",
      header: "Grade",
      cell: ({ row }) => row.original.gradeName ?? EMPTY_CELL,
    },
    {
      accessorKey: "sectionName",
      header: "Section",
      cell: ({ row }) => row.original.sectionName ?? EMPTY_CELL,
    },
    {
      accessorKey: "teacherName",
      header: "Teacher",
      cell: ({ row }) => row.original.teacherName ?? EMPTY_CELL,
    },
    {
      accessorKey: "deadline",
      header: "Deadline",
      cell: ({ row }) => new Date(row.original.deadline).toLocaleString(),
    },
    {
      accessorKey: "maxMarks",
      header: "Total marks",
    },
    {
      accessorKey: "submissionCount",
      header: "Submissions",
      cell: ({ row }) => <span className="tabular-nums">{row.original.submissionCount}</span>,
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