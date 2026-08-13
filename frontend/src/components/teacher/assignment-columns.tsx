"use client";

import Link from "next/link";
import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import { AssignmentStatus } from "@/lib/api/schemas/admin-assignments.schema";
import type { TeacherAssignment } from "@/lib/api/schemas/teacher.schema";
import { formatDateTime } from "@/lib/datetime";
import { EMPTY_CELL } from "@/lib/messages";
import { ROUTE_BUILDERS } from "@/lib/routes";
import { classLabel } from "@/lib/teacher-sections";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const statusVariant: Record<TeacherAssignment["status"], "default" | "secondary"> = {
  [AssignmentStatus.Published]: "default",
  [AssignmentStatus.Draft]: "secondary",
};

export function buildAssignmentColumns(options: {
  onEdit: (assignment: TeacherAssignment) => void;
  onPublish: (assignment: TeacherAssignment) => void;
  onUnpublish: (assignment: TeacherAssignment) => void;
  onCloseSubmissions: (assignment: TeacherAssignment) => void;
  onReopenSubmissions: (assignment: TeacherAssignment) => void;
  onDelete: (assignment: TeacherAssignment) => void;
}): ColumnDef<TeacherAssignment>[] {
  return [
    {
      accessorKey: "title",
      header: "Title",
      cell: ({ row }) => (
        <Link
          href={ROUTE_BUILDERS.teacherAssignment(row.original.id)}
          className="font-medium underline-offset-4 hover:underline"
        >
          {row.original.title}
        </Link>
      ),
    },
    {
      id: "class",
      header: "Class",
      cell: ({ row }) => classLabel(row.original),
    },
    {
      accessorKey: "subjectName",
      header: "Subject",
      cell: ({ row }) => row.original.subjectName ?? EMPTY_CELL,
    },
    {
      accessorKey: "deadline",
      header: "Deadline",
      cell: ({ row }) => formatDateTime(row.original.deadline),
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
      id: "submissions",
      header: "Graded",
      cell: ({ row }) => `${row.original.gradedCount} of ${row.original.submissionCount}`,
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => {
        const assignment = row.original;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8">
                <MoreHorizontalIcon className="size-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem asChild>
                <Link href={ROUTE_BUILDERS.teacherAssignment(assignment.id)}>View submissions</Link>
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => options.onEdit(assignment)}>Edit</DropdownMenuItem>
              {assignment.status === AssignmentStatus.Draft ? (
                <DropdownMenuItem onClick={() => options.onPublish(assignment)}>
                  Publish
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onClick={() => options.onUnpublish(assignment)}>
                  Unpublish
                </DropdownMenuItem>
              )}
              {assignment.submissionsOpen ? (
                <DropdownMenuItem onClick={() => options.onCloseSubmissions(assignment)}>
                  Close submissions
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onClick={() => options.onReopenSubmissions(assignment)}>
                  Reopen submissions
                </DropdownMenuItem>
              )}
              <DropdownMenuItem onClick={() => options.onDelete(assignment)} variant="destructive">
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}