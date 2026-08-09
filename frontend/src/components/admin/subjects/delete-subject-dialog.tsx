"use client";

import { toast } from "sonner";
import type { AdminSubjectMutations } from "@/lib/mutations/admin-subjects.mutations";
import type { SubjectSummary } from "@/lib/api/schemas/admin-subjects.schema";
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

interface DeleteSubjectDialogProps {
  subject: SubjectSummary | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminSubjectMutations.useDelete>;
}

export function DeleteSubjectDialog({ subject, onOpenChange, mutation }: DeleteSubjectDialogProps) {
  function handleDelete() {
    if (!subject) return;
    mutation.mutate(subject.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Subject deleted.");
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
    <AlertDialog open={!!subject} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete {subject?.name}?</AlertDialogTitle>
          <AlertDialogDescription>
            This action cannot be undone. A subject with assignments cannot be deleted.
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