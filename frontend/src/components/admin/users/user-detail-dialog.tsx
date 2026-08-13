"use client";

import { UserRole } from "@/lib/api/schemas/common.schema";
import { AdminUserQueries } from "@/lib/queries/admin-users.queries";
import { EMPTY_CELL } from "@/lib/messages";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/workspace/error-state";

interface UserDetailDialogProps {
  user: { id: string; fullName: string; role: UserRole } | null;
  onOpenChange: (open: boolean) => void;
}

/**
 * Reads the full profile block for the selected user. The list row only carries enough for the
 * table, so the dialog refetches the detail endpoint when it opens.
 */
export function UserDetailDialog({ user, onOpenChange }: UserDetailDialogProps) {
  const detailQuery = AdminUserQueries.useDetail(user?.id);

  return (
    <Dialog open={!!user} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{user?.fullName}</DialogTitle>
          <DialogDescription>
            {user?.role} account details. Edit the account to change these fields.
          </DialogDescription>
        </DialogHeader>

        {detailQuery.isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-4 w-1/2" />
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-4 w-1/3" />
          </div>
        ) : detailQuery.isError ? (
          <ErrorState description="Failed to load user details." retry={detailQuery.refetch} />
        ) : (
          <DetailRows detail={detailQuery.data!} />
        )}
      </DialogContent>
    </Dialog>
  );
}

function DetailRows({ detail }: { detail: { email: string; status: string; isActive: boolean; createdAt: string } }) {
  return (
    <dl className="grid gap-3 text-sm">
      <DetailRow label="Email" value={detail.email} />
      <DetailRow label="Status" value={detail.status} />
      <DetailRow label="Active" value={detail.isActive ? "Yes" : "No"} />
      <DetailRow label="Created" value={new Date(detail.createdAt).toLocaleDateString()} />
    </dl>
  );
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="grid grid-cols-3 gap-2">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="col-span-2">{value || EMPTY_CELL}</dd>
    </div>
  );
}
