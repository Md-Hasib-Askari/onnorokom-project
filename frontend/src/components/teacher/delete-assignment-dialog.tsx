"use client";

import { toast } from "sonner";

import type { TeacherAssignment } from "@/lib/api/schemas/teacher.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import type { TeacherMutations } from "@/lib/mutations/teacher.mutations";
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

interface DeleteAssignmentDialogProps {
  assignment: TeacherAssignment | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof TeacherMutations.useDeleteAssignment>;
  onDeleted?: () => void;
}

export function DeleteAssignmentDialog({
  assignment,
  onOpenChange,
  mutation,
  onDeleted,
}: DeleteAssignmentDialogProps) {
  function handleDelete() {
    if (!assignment) return;
    mutation.mutate(assignment.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Assignment deleted.");
          onOpenChange(false);
          onDeleted?.();
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
    <AlertDialog open={!!assignment} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete {assignment?.title}?</AlertDialogTitle>
          <AlertDialogDescription>
            This cannot be undone. An assignment that already has submissions cannot be deleted.
          </AlertDialogDescription>
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