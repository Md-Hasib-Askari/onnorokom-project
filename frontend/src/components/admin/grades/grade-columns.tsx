"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import type { GradeSummary } from "@/lib/api/schemas/grades.schema";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function buildGradeColumns(options: {
  onEdit: (grade: GradeSummary) => void;
  onDelete: (grade: GradeSummary) => void;
}): ColumnDef<GradeSummary>[] {
  return [
    {
      accessorKey: "name",
      header: "Name",
    },
    {
      accessorKey: "academicYear",
      header: "Academic year",
    },
    {
      accessorKey: "description",
      header: "Description",
      cell: ({ row }) => row.original.description ?? "—",
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => {
        const grade = row.original;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8">
                <MoreHorizontalIcon className="size-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => options.onEdit(grade)}>Edit</DropdownMenuItem>
              <DropdownMenuItem onClick={() => options.onDelete(grade)} variant="destructive">
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}