"use client";

import Link from "next/link";
import type { ColumnDef } from "@tanstack/react-table";

import type { StudentAssignmentListItem } from "@/lib/api/schemas/student.schema";
import { formatDateTime } from "@/lib/datetime";
import { EMPTY_CELL } from "@/lib/messages";
import { ROUTE_BUILDERS } from "@/lib/routes";
import { SubmissionStatusBadge } from "./submission-status-badge";

export function buildStudentAssignmentColumns(): ColumnDef<StudentAssignmentListItem>[] {
  return [
    {
      accessorKey: "title",
      header: "Title",
      cell: ({ row }) => (
        <Link
          href={ROUTE_BUILDERS.studentAssignment(row.original.id)}
          className="font-medium underline-offset-4 hover:underline"
        >
          {row.original.title}
        </Link>
      ),
    },
    {
      accessorKey: "subjectName",
      header: "Subject",
      cell: ({ row }) => row.original.subjectName ?? EMPTY_CELL,
    },
    {
      accessorKey: "teacherName",
      header: "Teacher",
      cell: ({ row }) => row.original.teacherName ?? EMPTY_CELL,
    },
    {
      accessorKey: "deadline",
      header: "Deadline",
      cell: ({ row }) => formatDateTime(row.original.deadline),
    },
    {
      accessorKey: "submissionStatus",
      header: "Status",
      cell: ({ row }) => <SubmissionStatusBadge assignment={row.original} />,
    },
    {
      accessorKey: "marks",
      header: "Marks",
      cell: ({ row }) =>
        row.original.marks === null
          ? EMPTY_CELL
          : `${row.original.marks} / ${row.original.maxMarks}`,
    },
  ];
}