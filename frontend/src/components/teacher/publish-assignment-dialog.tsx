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

interface PublishAssignmentDialogProps {
  assignment: TeacherAssignment | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof TeacherMutations.usePublishAssignment>;
}

/** Publishing is one-way, so it gets a confirmation the way destructive actions do. */
export function PublishAssignmentDialog({
  assignment,
  onOpenChange,
  mutation,
}: PublishAssignmentDialogProps) {
  function handlePublish() {
    if (!assignment) return;
    mutation.mutate(assignment.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Assignment published.");
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
          <AlertDialogTitle>Publish {assignment?.title}?</AlertDialogTitle>
          <AlertDialogDescription>
            Students in the class see the assignment as soon as it is published, and it cannot go
            back to draft.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e) => {
              e.preventDefault();
              handlePublish();
            }}
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Publishing..." : "Publish"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}