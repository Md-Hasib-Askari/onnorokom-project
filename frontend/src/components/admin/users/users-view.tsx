"use client";

import { useState } from "react";
import { toast } from "sonner";

import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { AccountStatus, UserRole } from "@/lib/api/schemas/common.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminUserQueries } from "@/lib/queries/admin-users.queries";
import { AdminUserMutations } from "@/lib/mutations/admin-users.mutations";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { DataTable } from "@/components/workspace/data-table";
import { ErrorState } from "@/components/workspace/error-state";
import { LoadMoreButton } from "@/components/workspace/load-more-button";
import { buildUserColumns } from "./user-columns";
import { buildPendingColumns } from "./pending-columns";
import { buildRejectedColumns } from "./rejected-columns";
import { ApproveStudentDialog } from "./approve-student-dialog";
import { CreateUserDialog } from "./create-user-dialog";
import { EditUserDialog } from "./edit-user-dialog";
import { UserDetailDialog } from "./user-detail-dialog";
import { DeleteUserDialog } from "./delete-user-dialog";
import { ResetPasswordDialog } from "./reset-password-dialog";

/** Tab identifiers for the users screen. */
const USER_TAB = {
  all: "all",
  pending: "pending",
  rejected: "rejected",
} as const;

/** Placeholder rows shown while a table loads. */
const SKELETON_ROW_COUNT = 5;

interface UsersViewProps {
  currentUserId: string;
}

export function UsersView({ currentUserId }: UsersViewProps) {
  const usersQuery = AdminUserQueries.useList();
  const pendingQuery = AdminUserQueries.useList({ status: AccountStatus.Pending });
  const rejectedQuery = AdminUserQueries.useList({ status: AccountStatus.Rejected });
  const approveMutation = AdminUserMutations.useApprove();
  const deleteMutation = AdminUserMutations.useDelete();
  const resetPasswordMutation = AdminUserMutations.useResetPassword();

  const [viewingUser, setViewingUser] = useState<AdminUserSummary | null>(null);
  const [editingUser, setEditingUser] = useState<AdminUserSummary | null>(null);
  const [deletingUser, setDeletingUser] = useState<AdminUserSummary | null>(null);
  const [approvingStudent, setApprovingStudent] = useState<AdminUserSummary | null>(null);
  const [resettingUser, setResettingUser] = useState<AdminUserSummary | null>(null);

  const users = usersQuery.data?.pages.flatMap((page) => page.items) ?? [];
  const pendingUsers = pendingQuery.data?.pages.flatMap((page) => page.items) ?? [];
  const rejectedUsers = rejectedQuery.data?.pages.flatMap((page) => page.items) ?? [];

  function handleApprove(user: AdminUserSummary, approve: boolean) {
    // A self-registered student has no section yet, so approving one is also an enrolment decision.
    // Rejecting never is, and a student an admin created already has a section to keep.
    if (approve && user.role === UserRole.Student && !user.studentSectionId) {
      setApprovingStudent(user);
      return;
    }

    approveMutation.mutate(
      { userId: user.id, approve },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success(approve ? `${user.fullName} approved.` : `${user.fullName} rejected.`);
          } else {
            toast.error(result.error ?? ERROR_MESSAGES.generic);
          }
        },
      }
    );
  }

  const userColumns = buildUserColumns({
    onViewDetails: setViewingUser,
    onEdit: setEditingUser,
    onDelete: setDeletingUser,
    onApprove: handleApprove,
    onResetPassword: setResettingUser,
    currentUserId,
  });

  const pendingColumns = buildPendingColumns({
    onViewDetails: setViewingUser,
    onApprove: handleApprove,
    pendingId: approveMutation.isPending ? approveMutation.variables?.userId : undefined,
  });

  const rejectedColumns = buildRejectedColumns({
    onViewDetails: setViewingUser,
    onApprove: handleApprove,
    onDelete: setDeletingUser,
    pendingId: approveMutation.isPending ? approveMutation.variables?.userId : undefined,
    deletingId: deleteMutation.isPending ? deleteMutation.variables : undefined,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
          <p className="text-sm text-muted-foreground">
            Manage admin, teacher, and student accounts.
          </p>
        </div>
        <CreateUserDialog />
      </div>

      <Tabs defaultValue={USER_TAB.all}>
        <TabsList className="bg-muted/60">
          <TabsTrigger value={USER_TAB.all}>All users</TabsTrigger>
          <TabsTrigger value={USER_TAB.pending}>Pending approval</TabsTrigger>
          <TabsTrigger value={USER_TAB.rejected}>Rejected</TabsTrigger>
        </TabsList>
        <TabsContent value={USER_TAB.all} className="space-y-4">
          {usersQuery.isLoading ? (
            <TableSkeleton />
          ) : usersQuery.isError ? (
            <ErrorState description="Failed to load users." retry={usersQuery.refetch} />
          ) : (
            <>
              <DataTable columns={userColumns} data={users} emptyMessage="No users yet." />
              {usersQuery.hasNextPage && (
                <LoadMoreButton
                  onClick={() => usersQuery.fetchNextPage()}
                  isLoading={usersQuery.isFetchingNextPage}
                  label="Load more users"
                />
              )}
            </>
          )}
        </TabsContent>
        <TabsContent value={USER_TAB.pending} className="space-y-4">
          {pendingQuery.isLoading ? (
            <TableSkeleton />
          ) : pendingQuery.isError ? (
            <ErrorState description="Failed to load pending users." retry={pendingQuery.refetch} />
          ) : (
            <>
              <DataTable
                columns={pendingColumns}
                data={pendingUsers}
                emptyMessage="No pending registrations."
              />
              {pendingQuery.hasNextPage && (
                <LoadMoreButton
                  onClick={() => pendingQuery.fetchNextPage()}
                  isLoading={pendingQuery.isFetchingNextPage}
                  label="Load more pending users"
                />
              )}
            </>
          )}
        </TabsContent>
        <TabsContent value={USER_TAB.rejected} className="space-y-4">
          {rejectedQuery.isLoading ? (
            <TableSkeleton />
          ) : rejectedQuery.isError ? (
            <ErrorState description="Failed to load users." retry={rejectedQuery.refetch} />
          ) : (
            <>
              <DataTable
                columns={rejectedColumns}
                data={rejectedUsers}
                emptyMessage="No rejected users."
              />
              {rejectedQuery.hasNextPage && (
                <LoadMoreButton
                  onClick={() => rejectedQuery.fetchNextPage()}
                  isLoading={rejectedQuery.isFetchingNextPage}
                  label="Load more rejected users"
                />
              )}
            </>
          )}
        </TabsContent>
      </Tabs>

      <ApproveStudentDialog
        user={approvingStudent}
        onOpenChange={(open) => !open && setApprovingStudent(null)}
        mutation={approveMutation}
      />
      <UserDetailDialog user={viewingUser} onOpenChange={(open) => !open && setViewingUser(null)} />
      <EditUserDialog user={editingUser} onOpenChange={(open) => !open && setEditingUser(null)} />
      <DeleteUserDialog
        user={deletingUser}
        onOpenChange={(open) => !open && setDeletingUser(null)}
        mutation={deleteMutation}
      />
      <ResetPasswordDialog
        user={resettingUser}
        onOpenChange={(open) => !open && setResettingUser(null)}
        mutation={resetPasswordMutation}
      />
    </div>
  );
}

function TableSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: SKELETON_ROW_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-10 w-full" />
      ))}
    </div>
  );
}
