"use client";

import { useState } from "react";
import { toast } from "sonner";

import type { SubjectSummary } from "@/lib/api/schemas/admin-subjects.schema";
import { isEligibleTeacher } from "@/lib/eligible-teacher";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSubjectMutations } from "@/lib/mutations/admin-subjects.mutations";
import { AdminUserQueries } from "@/lib/queries/admin-users.queries";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface AssignTeacherDialogProps {
  subject: SubjectSummary | null;
  onOpenChange: (open: boolean) => void;
}

export function AssignTeacherDialog({ subject, onOpenChange }: AssignTeacherDialogProps) {
  const usersQuery = AdminUserQueries.useList();
  const mutation = AdminSubjectMutations.useAssignTeacher();
  const [teacherId, setTeacherId] = useState<string | undefined>(undefined);

  const eligibleTeachers = (usersQuery.data ?? []).filter(isEligibleTeacher);

  function handleClose(open: boolean) {
    if (!open) setTeacherId(undefined);
    onOpenChange(open);
  }

  function onSubmit() {
    if (!subject || !teacherId) return;
    mutation.mutate(
      { subjectId: subject.id, teacherId },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Teacher assigned.");
            handleClose(false);
          } else {
            toast.error(result.error ?? ERROR_MESSAGES.generic);
          }
        },
        onError: () => {
          toast.error(ERROR_MESSAGES.genericRetry);
        },
      }
    );
  }

  return (
    <Dialog open={!!subject} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Assign teacher{subject ? ` to ${subject.name}` : ""}</DialogTitle>
          <DialogDescription>Only approved, active teachers can be assigned.</DialogDescription>
        </DialogHeader>
        <Select value={teacherId} onValueChange={setTeacherId}>
          <SelectTrigger className="w-full">
            <SelectValue placeholder={usersQuery.isLoading ? "Loading..." : "Select a teacher"} />
          </SelectTrigger>
          <SelectContent>
            {eligibleTeachers.map((teacher) => (
              <SelectItem key={teacher.id} value={teacher.id}>
                {teacher.fullName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <DialogFooter>
          <Button type="button" onClick={onSubmit} disabled={!teacherId || mutation.isPending}>
            {mutation.isPending ? "Assigning..." : "Assign teacher"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}