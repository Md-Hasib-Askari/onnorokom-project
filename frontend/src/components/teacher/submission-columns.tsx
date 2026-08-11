"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import { SubmissionStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { TeacherSubmission } from "@/lib/api/schemas/teacher.schema";
import { formatDateTime } from "@/lib/datetime";
import { EMPTY_CELL } from "@/lib/messages";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const statusVariant: Record<TeacherSubmission["status"], "default" | "secondary" | "outline"> = {
  [SubmissionStatus.Graded]: "default",
  [SubmissionStatus.Submitted]: "secondary",
  [SubmissionStatus.Resubmitted]: "secondary",
  [SubmissionStatus.Returned]: "outline",
};

export function buildSubmissionColumns(options: {
  maxMarks: number;
  onGrade: (submission: TeacherSubmission) => void;
  onReturn: (submission: TeacherSubmission) => void;
}): ColumnDef<TeacherSubmission>[] {
  return [
    {
      accessorKey: "studentName",
      header: "Student",
      cell: ({ row }) => row.original.studentName ?? EMPTY_CELL,
    },
    {
      accessorKey: "rollNumber",
      header: "Roll",
      cell: ({ row }) => row.original.rollNumber ?? EMPTY_CELL,
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <Badge variant={statusVariant[row.original.status]}>{row.original.status}</Badge>
          {row.original.isLate ? <Badge variant="destructive">Late</Badge> : null}
        </div>
      ),
    },
    {
      accessorKey: "marks",
      header: "Marks",
      cell: ({ row }) =>
        row.original.marks === null ? EMPTY_CELL : `${row.original.marks} / ${options.maxMarks}`,
    },
    {
      accessorKey: "submittedAt",
      header: "Submitted",
      cell: ({ row }) => formatDateTime(row.original.submittedAt),
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => {
        const submission = row.original;
        const isGraded = submission.status === SubmissionStatus.Graded;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8">
                <MoreHorizontalIcon className="size-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => options.onGrade(submission)}>
                {isGraded ? "Update grade" : "Grade"}
              </DropdownMenuItem>
              {isGraded ? (
                <DropdownMenuItem onClick={() => options.onReturn(submission)}>
                  Return for revision
                </DropdownMenuItem>
              ) : null}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}