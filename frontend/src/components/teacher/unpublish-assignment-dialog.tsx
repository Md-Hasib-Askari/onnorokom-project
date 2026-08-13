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

interface UnpublishAssignmentDialogProps {
  assignment: TeacherAssignment | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof TeacherMutations.useUnpublishAssignment>;
}

/** Sends a published assignment back to draft, hiding it from students. */
export function UnpublishAssignmentDialog({
  assignment,
  onOpenChange,
  mutation,
}: UnpublishAssignmentDialogProps) {
  function handleUnpublish() {
    if (!assignment) return;
    mutation.mutate(assignment.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Assignment unpublished.");
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
    <AlertDialog open={!!assignment} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Unpublish {assignment?.title}?</AlertDialogTitle>
          <AlertDialogDescription>
            The assignment becomes a draft again and disappears from the students&apos; view.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e) => {
              e.preventDefault();
              handleUnpublish();
            }}
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Unpublishing..." : "Unpublish"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
