"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import type { SectionSummary } from "@/lib/api/schemas/sections.schema";
import { EMPTY_CELL } from "@/lib/messages";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function buildSectionColumns(options: {
  onEdit: (section: SectionSummary) => void;
  onDelete: (section: SectionSummary) => void;
  onManageSubjects: (section: SectionSummary) => void;
}): ColumnDef<SectionSummary>[] {
  return [
    {
      accessorKey: "name",
      header: "Name",
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
        const section = row.original;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8">
                <MoreHorizontalIcon className="size-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => options.onManageSubjects(section)}>
                Manage subjects
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => options.onEdit(section)}>Edit section</DropdownMenuItem>
              <DropdownMenuItem onClick={() => options.onDelete(section)} variant="destructive">
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}