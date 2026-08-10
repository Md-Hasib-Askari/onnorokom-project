"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import type { SubjectSummary } from "@/lib/api/schemas/admin-subjects.schema";
import { EMPTY_CELL } from "@/lib/messages";
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
}): ColumnDef<SubjectSummary>[] {
  return [
    {
      accessorKey: "name",
      header: "Name",
    },
    {
      accessorKey: "code",
      header: "Code",
      cell: ({ row }) => row.original.code ?? EMPTY_CELL,
    },
    {
      accessorKey: "gradeName",
      header: "Grade",
      cell: ({ row }) => row.original.gradeName ?? EMPTY_CELL,
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