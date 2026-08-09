"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontalIcon } from "lucide-react";

import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { AccountStatus } from "@/lib/api/schemas/common.schema";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const statusVariant: Record<AdminUserSummary["status"], "default" | "secondary" | "destructive"> = {
  [AccountStatus.Approved]: "default",
  [AccountStatus.Pending]: "secondary",
  [AccountStatus.Rejected]: "destructive",
};

export function buildUserColumns(options: {
  onEdit: (user: AdminUserSummary) => void;
  onDelete: (user: AdminUserSummary) => void;
  onApprove: (user: AdminUserSummary, approve: boolean) => void;
  currentUserId: string;
}): ColumnDef<AdminUserSummary>[] {
  return [
    {
      accessorKey: "fullName",
      header: "Name",
      cell: ({ row }) => (
        <span className="flex items-center gap-2">
          {row.original.fullName}
          {row.original.id === options.currentUserId && (
            <Badge variant="outline" className="text-[10px]">
              You
            </Badge>
          )}
        </span>
      ),
    },
    {
      accessorKey: "email",
      header: "Email",
    },
    {
      accessorKey: "role",
      header: "Role",
      cell: ({ row }) => <Badge variant="outline">{row.original.role}</Badge>,
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => (
        <Badge variant={statusVariant[row.original.status]}>{row.original.status}</Badge>
      ),
    },
    {
      accessorKey: "createdAt",
      header: "Created",
      cell: ({ row }) => new Date(row.original.createdAt).toLocaleDateString(),
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => {
        const user = row.original;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8">
                <MoreHorizontalIcon className="size-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {user.status === AccountStatus.Pending ? (
                <>
                  <DropdownMenuItem onClick={() => options.onApprove(user, true)}>
                    Approve
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => options.onApprove(user, false)} variant="destructive">
                    Reject
                  </DropdownMenuItem>
                </>
              ) : (
                <DropdownMenuItem onClick={() => options.onEdit(user)}>Edit</DropdownMenuItem>
              )}
              {user.id !== options.currentUserId && (
                <DropdownMenuItem onClick={() => options.onDelete(user)} variant="destructive">
                  Delete
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}
