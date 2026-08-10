"use client";

import type { ColumnDef } from "@tanstack/react-table";

import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { EMPTY_CELL } from "@/lib/messages";

/**
 * Grade and section of a student's enrolment, spread into every users table so the three
 * tabs stay in step. The backend resolves both on the list and the pending endpoints, so
 * pending registrations show their claimed section before an admin approves them.
 */
export const enrollmentColumns: ColumnDef<AdminUserSummary>[] = [
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
];