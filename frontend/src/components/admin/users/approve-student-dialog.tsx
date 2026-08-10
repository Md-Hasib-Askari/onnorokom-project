"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";

import type { AdminUserSummary } from "@/lib/api/schemas/admin-users.schema";
import { ERROR_MESSAGES, SELECT_PLACEHOLDERS, VALIDATION_MESSAGES } from "@/lib/messages";
import type { AdminUserMutations } from "@/lib/mutations/admin-users.mutations";
import { AdminGradeQueries } from "@/lib/queries/admin-grades.queries";
import { SectionSelect } from "@/components/admin/users/section-select";
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
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

const approveStudentFormSchema = z.object({
  studentSectionId: z.uuid(VALIDATION_MESSAGES.sectionRequired),
});

type ApproveStudentFormValues = z.infer<typeof approveStudentFormSchema>;

interface ApproveStudentDialogProps {
  user: AdminUserSummary | null;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminUserMutations.useApprove>;
}

/**
 * Self-registered students arrive with no section, because the public form never asks for one.
 * Approving is therefore also an enrolment decision, so the admin picks the section here.
 */
export function ApproveStudentDialog({ user, onOpenChange, mutation }: ApproveStudentDialogProps) {
  return (
    <Dialog open={!!user} onOpenChange={onOpenChange}>
      <DialogContent>
        {/* Remounting per user clears the previous pick, so one approval cannot seed the next. */}
        {user && <ApproveStudentForm key={user.id} user={user} onOpenChange={onOpenChange} mutation={mutation} />}
      </DialogContent>
    </Dialog>
  );
}

function ApproveStudentForm({
  user,
  onOpenChange,
  mutation,
}: {
  user: AdminUserSummary;
  onOpenChange: (open: boolean) => void;
  mutation: ReturnType<typeof AdminUserMutations.useApprove>;
}) {
  const [gradeId, setGradeId] = useState<string | undefined>(undefined);
  const grades = AdminGradeQueries.useCurrentYearList();

  const form = useForm<ApproveStudentFormValues>({
    resolver: zodResolver(approveStudentFormSchema),
    defaultValues: { studentSectionId: "" },
  });

  function onSubmit(values: ApproveStudentFormValues) {
    mutation.mutate(
      { userId: user.id, approve: true, studentSectionId: values.studentSectionId },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success(`${user.fullName} approved.`);
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors?.studentSectionId) {
            form.setError("studentSectionId", { message: result.fieldErrors.studentSectionId });
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
    <>
      <DialogHeader>
        <DialogTitle>Approve {user.fullName}</DialogTitle>
        <DialogDescription>
          Choose the section this student joins. They can sign in as soon as you approve them.
        </DialogDescription>
      </DialogHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <FormItem>
            <FormLabel>Grade</FormLabel>
            <Select
              value={gradeId ?? ""}
              onValueChange={(value) => {
                setGradeId(value);
                form.resetField("studentSectionId");
              }}
            >
              <FormControl>
                <SelectTrigger className="w-full">
                  <SelectValue
                    placeholder={
                      grades.isLoading ? SELECT_PLACEHOLDERS.loading : SELECT_PLACEHOLDERS.grade
                    }
                  />
                </SelectTrigger>
              </FormControl>
              <SelectContent>
                {grades.data?.map((grade) => (
                  <SelectItem key={grade.id} value={grade.id}>
                    {grade.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormItem>
          <FormField
            control={form.control}
            name="studentSectionId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Section</FormLabel>
                <SectionSelect
                  gradeId={gradeId}
                  value={field.value}
                  onValueChange={field.onChange}
                />
                <FormMessage />
              </FormItem>
            )}
          />
          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Approving..." : "Approve"}
            </Button>
          </DialogFooter>
        </form>
      </Form>
    </>
  );
}
