"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import type { SubjectSummary } from "@/lib/api/schemas/admin-subjects.schema";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function buildSubjectColumns(options: {
  onEdit: (subject: SubjectSummary) => void;
  onDelete: (subject: SubjectSummary) => void;
  onAssignTeacher: (subject: SubjectSummary) => void;
  onUnassignTeacher: (subject: SubjectSummary) => void;
}): ColumnDef<SubjectSummary>[] {
  return [
    {
      accessorKey: "name",
      header: "Name",
    },
    {
      accessorKey: "code",
      header: "Code",
      cell: ({ row }) => row.original.code ?? "—",
    },
    {
      accessorKey: "gradeName",
      header: "Grade",
      cell: ({ row }) => row.original.gradeName ?? "—",
    },
    {
      accessorKey: "teacherName",
      header: "Teacher",
      cell: ({ row }) =>
        row.original.teacherName ? (
          row.original.teacherName
        ) : (
          <Badge variant="secondary">Unassigned</Badge>
        ),
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => {
        const subject = row.original;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8">
                <MoreHorizontalIcon className="size-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => options.onEdit(subject)}>Edit subject</DropdownMenuItem>
              <DropdownMenuItem onClick={() => options.onAssignTeacher(subject)}>
                {subject.teacherId ? "Reassign teacher" : "Assign teacher"}
              </DropdownMenuItem>
              {subject.teacherId && (
                <DropdownMenuItem onClick={() => options.onUnassignTeacher(subject)}>
                  Unassign teacher
                </DropdownMenuItem>
              )}
              <DropdownMenuItem onClick={() => options.onDelete(subject)} variant="destructive">
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}