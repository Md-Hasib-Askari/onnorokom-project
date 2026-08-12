"use client";

import type { ColumnDef } from "@tanstack/react-table";

import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { EMPTY_CELL } from "@/lib/messages";

/**
 * Admin-assigned visible identifier for a teacher, spread into every users table so the
 * three tabs stay in step, mirroring how enrollmentColumns surfaces a student's grade/section.
 */
export const teacherColumns: ColumnDef<AdminUserSummary>[] = [
  {
    accessorKey: "teacherCode",
    header: "Teacher code",
    cell: ({ row }) => row.original.teacherCode ?? EMPTY_CELL,
  },
];
