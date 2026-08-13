"use client";

import type { ColumnDef } from "@tanstack/react-table";
import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

export function buildRejectedColumns(options: {
  onViewDetails: (user: AdminUserSummary) => void;
  onApprove: (user: AdminUserSummary, approve: boolean) => void;
  onDelete: (user: AdminUserSummary) => void;
  pendingId?: string;
  deletingId?: string;
}): ColumnDef<AdminUserSummary>[] {
  return [
    {
      accessorKey: "fullName", header: "Name",
      cell: ({ row }) => (
        <button
          type="button"
          className="cursor-pointer hover:underline"
          onClick={() => options.onViewDetails(row.original)}
        >
          {row.original.fullName}
        </button>
      ),
    },
    { accessorKey: "email", header: "Email" },
    {
      accessorKey: "role",
      header: "Role",
      cell: ({ row }) => <Badge variant="outline">{row.original.role}</Badge>,
    },
    {
      accessorKey: "createdAt",
      header: "Requested",
      cell: ({ row }) => new Date(row.original.createdAt).toLocaleDateString(),
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => {
        const user = row.original;
        const busy = options.pendingId === user.id;
        const deleting = options.deletingId === user.id;
        return (
          <div className="flex justify-end gap-2">
            <Button size="sm" variant="ghost" onClick={() => options.onViewDetails(user)}>
              View details
            </Button>
            <Button
              size="sm"
              variant="outline"
              disabled={busy || deleting}
              onClick={() => options.onDelete(user)}
            >
              Delete
            </Button>
            <Button size="sm" disabled={busy || deleting} onClick={() => options.onApprove(user, true)}>
              Approve
            </Button>
          </div>
        );
      },
    },
  ];
}
