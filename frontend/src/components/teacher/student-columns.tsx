import type { ColumnDef } from "@tanstack/react-table";

import type { TeacherStudent } from "@/lib/api/schemas/teacher.schema";
import { EMPTY_CELL } from "@/lib/messages";
import { classLabel } from "@/lib/teacher-sections";

/** The teacher's roster: who is in the sections they teach. Rows are grouped by class. */
export const studentColumns: ColumnDef<TeacherStudent>[] = [
  {
    accessorKey: "fullName",
    header: "Name",
    cell: ({ row }) => (
      <span className="font-medium">{row.original.fullName || EMPTY_CELL}</span>
    ),
  },
  {
    accessorKey: "rollNumber",
    header: "Roll number",
    cell: ({ row }) => row.original.rollNumber ?? EMPTY_CELL,
  },
  {
    accessorKey: "class",
    header: "Class",
    cell: ({ row }) => classLabel(row.original),
  },
];
