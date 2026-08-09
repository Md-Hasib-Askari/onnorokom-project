"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  gradeUpdateRequestSchema,
  type GradeSummary,
  type GradeUpdateRequest,
} from "@/lib/api/schemas/grades.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminGradeMutations } from "@/lib/mutations/admin-grades.mutations";
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
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

interface EditGradeDialogProps {
  grade: GradeSummary | null;
  onOpenChange: (open: boolean) => void;
}

export function EditGradeDialog({ grade, onOpenChange }: EditGradeDialogProps) {
  const mutation = AdminGradeMutations.useUpdate();

  const form = useForm<GradeUpdateRequest>({
    resolver: zodResolver(gradeUpdateRequestSchema),
    defaultValues: { name: "", academicYear: "", description: "" },
  });

  useEffect(() => {
    if (!grade) return;
    form.reset({
      name: grade.name,
      academicYear: grade.academicYear,
      description: grade.description ?? "",
    });
  }, [grade, form]);

  if (!grade) return null;

  function onSubmit(values: GradeUpdateRequest) {
    if (!grade) return;
    mutation.mutate(
      { id: grade.id, values },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Grade updated.");
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof GradeUpdateRequest, { message });
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
    <Dialog open={!!grade} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit grade</DialogTitle>
          <DialogDescription>Update the grade&apos;s name, year, and description.</DialogDescription>
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
              name="academicYear"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Academic year</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
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