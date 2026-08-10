"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  sectionUpdateRequestSchema,
  type SectionSummary,
  type SectionUpdateRequest,
} from "@/lib/api/schemas/sections.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSectionMutations } from "@/lib/mutations/admin-sections.mutations";
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

interface EditSectionDialogProps {
  section: SectionSummary | null;
  onOpenChange: (open: boolean) => void;
}

export function EditSectionDialog({ section, onOpenChange }: EditSectionDialogProps) {
  const grades = AdminGradeQueries.useCurrentYearList();
  const mutation = AdminSectionMutations.useUpdate();

  const form = useForm<SectionUpdateRequest>({
    resolver: zodResolver(sectionUpdateRequestSchema),
    defaultValues: { name: "", gradeId: "" },
  });

  useEffect(() => {
    if (!section) return;
    form.reset({
      name: section.name,
      gradeId: section.gradeId,
    });
  }, [section, form]);

  if (!section) return null;

  function onSubmit(values: SectionUpdateRequest) {
    if (!section) return;
    mutation.mutate(
      { id: section.id, values },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Section updated.");
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof SectionUpdateRequest, { message });
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
    <Dialog open={!!section} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit section</DialogTitle>
          <DialogDescription>Update the section&apos;s name and grade.</DialogDescription>
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