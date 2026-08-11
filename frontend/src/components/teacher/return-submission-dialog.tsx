"use client";

import { toast } from "sonner";

import type { TeacherSubmission } from "@/lib/api/schemas/teacher.schema";
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

interface ReturnSubmissionDialogProps {
  submission: TeacherSubmission | null;
  assignmentId: string;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof TeacherMutations.useReturnSubmission>;
}

export function ReturnSubmissionDialog({
  submission,
  assignmentId,
  onOpenChange,
  mutation,
}: ReturnSubmissionDialogProps) {
  function handleReturn() {
    if (!submission) return;
    mutation.mutate(
      { assignmentId, submissionId: submission.id },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Submission returned for revision.");
          } else {
            toast.error(result.error ?? ERROR_MESSAGES.generic);
          }
          onOpenChange(false);
        },
        onError: () => {
          toast.error(ERROR_MESSAGES.genericRetry);
        },
      }
    );
  }

  return (
    <AlertDialog open={!!submission} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Return {submission?.studentName}&apos;s submission?</AlertDialogTitle>
          <AlertDialogDescription>
            The current mark and feedback are cleared so the student can submit again.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e) => {
              e.preventDefault();
              handleReturn();
            }}
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Returning..." : "Return"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}