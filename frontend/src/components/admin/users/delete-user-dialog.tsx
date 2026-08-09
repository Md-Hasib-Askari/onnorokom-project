"use client";

import { toast } from "sonner";
import type { AdminUserMutations } from "@/lib/mutations/admin-users.mutations";
import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface DeleteUserDialogProps {
  user: AdminUserSummary | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminUserMutations.useDelete>;
}

export function DeleteUserDialog({ user, onOpenChange, mutation }: DeleteUserDialogProps) {
  function handleDelete() {
    if (!user) return;
    mutation.mutate(user.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("User deleted.");
          onOpenChange(false);
        } else {
          toast.error(result.error ?? ERROR_MESSAGES.generic);
          onOpenChange(false);
        }
      },
      onError: () => {
        toast.error(ERROR_MESSAGES.genericRetry);
      },
    });
  }

  return (
    <AlertDialog open={!!user} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete {user?.fullName}?</AlertDialogTitle>
          <AlertDialogDescription>This action cannot be undone.</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e) => {
              e.preventDefault();
              handleDelete();
            }}
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Deleting..." : "Delete"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
