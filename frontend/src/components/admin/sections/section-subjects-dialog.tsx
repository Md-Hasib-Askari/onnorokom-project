"use client";

import { toast } from "sonner";

import type { SectionSummary } from "@/lib/api/schemas/sections.schema";
import { isEligibleTeacher } from "@/lib/eligible-teacher";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSectionMutations } from "@/lib/mutations/admin-sections.mutations";
import { AdminSectionQueries } from "@/lib/queries/admin-sections.queries";
import { AdminUserQueries } from "@/lib/queries/admin-users.queries";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Dialog,
  DialogContent,
  DialogDescription,
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

/** Radix `SelectItem` rejects an empty-string value, so the "no teacher yet" option uses this sentinel. */
const NO_TEACHER_VALUE = "none";

interface SectionSubjectsDialogProps {
  section: SectionSummary | null;
  onOpenChange: (open: boolean) => void;
}

export function SectionSubjectsDialog({ section, onOpenChange }: SectionSubjectsDialogProps) {
  const subjectsQuery = AdminSectionQueries.useSectionSubjects(section?.id);
  const usersQuery = AdminUserQueries.useList();
  const assignMutation = AdminSectionMutations.useAssignSubjectTeacher();
  const unassignMutation = AdminSectionMutations.useUnassignSubjectTeacher();

  const eligibleTeachers = (usersQuery.data ?? []).filter(isEligibleTeacher);

  function handleTeacherChange(subjectId: string, subjectName: string, value: string) {
    if (!section) return;
    if (value === NO_TEACHER_VALUE) {
      unassignMutation.mutate(
        { sectionId: section.id, subjectId },
        {
          onSuccess: (result) => {
            if (result.success) {
              toast.success(`Teacher unassigned from ${subjectName}.`);
            } else {
              toast.error(result.error ?? ERROR_MESSAGES.generic);
            }
          },
          onError: () => toast.error(ERROR_MESSAGES.genericRetry),
        }
      );
      return;
    }
    assignMutation.mutate(
      { sectionId: section.id, subjectId, teacherId: value },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success(`Teacher assigned to ${subjectName}.`);
          } else {
            toast.error(result.error ?? ERROR_MESSAGES.generic);
          }
        },
        onError: () => toast.error(ERROR_MESSAGES.genericRetry),
      }
    );
  }

  return (
    <Dialog open={!!section} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Subjects{section ? ` in ${section.name}` : ""}</DialogTitle>
          <DialogDescription>
            Assign a teacher for each of this grade&apos;s subjects, scoped to this section only.
          </DialogDescription>
        </DialogHeader>
        {subjectsQuery.isLoading ? (
          <SubjectsSkeleton />
        ) : subjectsQuery.isError ? (
          <p className="text-sm text-destructive">Failed to load subjects.</p>
        ) : subjectsQuery.data && subjectsQuery.data.length === 0 ? (
          <p className="text-sm text-muted-foreground">This grade has no subjects yet.</p>
        ) : (
          <div className="space-y-3">
            {subjectsQuery.data?.map((item) => (
              <div key={item.subjectId} className="flex items-center justify-between gap-4 rounded-lg border p-3">
                <div className="space-y-0.5">
                  <p className="text-sm font-medium">{item.subjectName}</p>
                  {item.subjectCode && <p className="text-xs text-muted-foreground">{item.subjectCode}</p>}
                </div>
                <Select
                  value={item.teacherId ?? NO_TEACHER_VALUE}
                  onValueChange={(value) => handleTeacherChange(item.subjectId, item.subjectName, value)}
                >
                  <SelectTrigger className="w-56">
                    <SelectValue placeholder={usersQuery.isLoading ? "Loading..." : "Select a teacher"} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={NO_TEACHER_VALUE}>
                      <Badge variant="secondary">Unassigned</Badge>
                    </SelectItem>
                    {eligibleTeachers.map((teacher) => (
                      <SelectItem key={teacher.id} value={teacher.id}>
                        {teacher.fullName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            ))}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

function SubjectsSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 3 }).map((_, index) => (
        <Skeleton key={index} className="h-16 w-full" />
      ))}
    </div>
  );
}