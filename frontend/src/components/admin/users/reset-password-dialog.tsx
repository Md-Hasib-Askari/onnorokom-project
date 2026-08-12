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

interface ResetPasswordDialogProps {
  user: AdminUserSummary | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminUserMutations.useResetPassword>;
}

export function ResetPasswordDialog({ user, onOpenChange, mutation }: ResetPasswordDialogProps) {
  function handleReset() {
    if (!user) return;
    mutation.mutate(user.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success(`A new password was emailed to ${user.email}.`);
        } else {
          toast.error(result.error ?? ERROR_MESSAGES.generic);
        }
        onOpenChange(false);
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
          <AlertDialogTitle>Reset password for {user?.fullName}?</AlertDialogTitle>
          <AlertDialogDescription>
            A new randomly generated password will be emailed to {user?.email}. They will be
            required to change it the next time they sign in.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e) => {
              e.preventDefault();
              handleReset();
            }}
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Resetting..." : "Reset password"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}