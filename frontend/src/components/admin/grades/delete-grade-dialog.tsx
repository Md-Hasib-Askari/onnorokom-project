"use client";

import { toast } from "sonner";
import type { AdminGradeMutations } from "@/lib/mutations/admin-grades.mutations";
import type { GradeSummary } from "@/lib/api/schemas/grades.schema";
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

interface DeleteGradeDialogProps {
  grade: GradeSummary | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminGradeMutations.useDelete>;
}

export function DeleteGradeDialog({ grade, onOpenChange, mutation }: DeleteGradeDialogProps) {
  function handleDelete() {
    if (!grade) return;
    mutation.mutate(grade.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Grade deleted.");
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
    <AlertDialog open={!!grade} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete {grade?.name}?</AlertDialogTitle>
          <AlertDialogDescription>
            This action cannot be undone. A grade with subjects or enrolled students cannot be
            deleted.
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