"use client";

import type { ColumnDef } from "@tanstack/react-table";

import {
  SubmissionStatus,
  type SubmissionListItem,
} from "@/lib/api/schemas/admin-assignments.schema";
import { EMPTY_CELL } from "@/lib/messages";
import { Badge } from "@/components/ui/badge";

const statusVariant: Record<SubmissionListItem["status"], "default" | "secondary" | "destructive"> = {
  [SubmissionStatus.Graded]: "default",
  [SubmissionStatus.Submitted]: "secondary",
  [SubmissionStatus.Resubmitted]: "secondary",
  [SubmissionStatus.Late]: "destructive",
};

export function buildSubmissionColumns(): ColumnDef<SubmissionListItem>[] {
  return [
    {
      accessorKey: "assignmentTitle",
      header: "Assignment",
      cell: ({ row }) => row.original.assignmentTitle ?? EMPTY_CELL,
    },
    {
      accessorKey: "studentName",
      header: "Student",
      cell: ({ row }) => row.original.studentName ?? EMPTY_CELL,
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => (
        <Badge variant={statusVariant[row.original.status]}>{row.original.status}</Badge>
      ),
    },
    {
      accessorKey: "marks",
      header: "Marks",
      cell: ({ row }) => row.original.marks ?? EMPTY_CELL,
    },
    {
      accessorKey: "submittedAt",
      header: "Submitted",
      cell: ({ row }) => new Date(row.original.submittedAt).toLocaleString(),
    },
  ];
}