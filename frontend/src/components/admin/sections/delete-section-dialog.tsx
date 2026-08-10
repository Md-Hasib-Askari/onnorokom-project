"use client";

import { toast } from "sonner";
import type { AdminSectionMutations } from "@/lib/mutations/admin-sections.mutations";
import type { SectionSummary } from "@/lib/api/schemas/sections.schema";
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

interface DeleteSectionDialogProps {
  section: SectionSummary | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminSectionMutations.useDelete>;
}

export function DeleteSectionDialog({ section, onOpenChange, mutation }: DeleteSectionDialogProps) {
  function handleDelete() {
    if (!section) return;
    mutation.mutate(section.id, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Section deleted.");
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
    <AlertDialog open={!!section} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete {section?.name}?</AlertDialogTitle>
          <AlertDialogDescription>
            This action cannot be undone. A section with enrolled students cannot be deleted.
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