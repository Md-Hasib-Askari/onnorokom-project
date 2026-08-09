"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  subjectUpdateRequestSchema,
  type SubjectSummary,
  type SubjectUpdateRequest,
} from "@/lib/api/schemas/admin-subjects.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSubjectMutations } from "@/lib/mutations/admin-subjects.mutations";
import { AdminGradeQueries } from "@/lib/queries/admin-grades.queries";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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

interface EditSubjectDialogProps {
  subject: SubjectSummary | null;
  onOpenChange: (open: boolean) => void;
}

export function EditSubjectDialog({ subject, onOpenChange }: EditSubjectDialogProps) {
  const grades = AdminGradeQueries.useCurrentYearList();
  const mutation = AdminSubjectMutations.useUpdate();

  const form = useForm<SubjectUpdateRequest>({
    resolver: zodResolver(subjectUpdateRequestSchema),
    defaultValues: { name: "", gradeId: "", code: "" },
  });

  useEffect(() => {
    if (!subject) return;
    form.reset({
      name: subject.name,
      gradeId: subject.gradeId,
      code: subject.code ?? "",
    });
  }, [subject, form]);

  if (!subject) return null;

  function onSubmit(values: SubjectUpdateRequest) {
    if (!subject) return;
    mutation.mutate(
      { id: subject.id, values },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Subject updated.");
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof SubjectUpdateRequest, { message });
            }
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
    <Dialog open={!!subject} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit subject</DialogTitle>
          <DialogDescription>
            Update name, code, and grade. Use the row menu to change the assigned teacher.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="code"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Code</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="gradeId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Grade</FormLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={grades.isLoading ? "Loading..." : "Select a grade"} />
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
                  <FormMessage />
                </FormItem>
              )}
            />
            <DialogFooter>
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending ? "Saving..." : "Save changes"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}