"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  assignmentUpdateRequestSchema,
  type AssignmentUpdateRequest,
  type TeacherAssignment,
} from "@/lib/api/schemas/teacher.schema";
import { toDateTimeLocalInput, toIsoInstant } from "@/lib/datetime";
import { ERROR_MESSAGES } from "@/lib/messages";
import { TeacherMutations } from "@/lib/mutations/teacher.mutations";
import { classLabel } from "@/lib/teacher-sections";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
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
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

interface EditAssignmentDialogProps {
  assignment: TeacherAssignment | null;
  onOpenChange: (open: boolean) => void;
}

export function EditAssignmentDialog({ assignment, onOpenChange }: EditAssignmentDialogProps) {
  const mutation = TeacherMutations.useUpdateAssignment();

  const form = useForm<AssignmentUpdateRequest>({
    resolver: zodResolver(assignmentUpdateRequestSchema),
    defaultValues: { title: "", description: "", deadline: "", maxMarks: 0, allowLateSubmission: false },
  });

  useEffect(() => {
    if (!assignment) return;
    form.reset({
      title: assignment.title,
      description: assignment.description ?? "",
      deadline: toDateTimeLocalInput(assignment.deadline),
      maxMarks: assignment.maxMarks,
      allowLateSubmission: assignment.allowLateSubmission,
    });
  }, [assignment, form]);

  if (!assignment) return null;

  function onSubmit(values: AssignmentUpdateRequest) {
    if (!assignment) return;
    mutation.mutate(
      { id: assignment.id, values: { ...values, deadline: toIsoInstant(values.deadline) } },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Assignment updated.");
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof AssignmentUpdateRequest, { message });
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
    <Dialog open={!!assignment} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit assignment</DialogTitle>
          <DialogDescription>
            {classLabel(assignment)}
            {assignment.subjectName ? ` - ${assignment.subjectName}` : ""}. The class and subject are
            fixed once an assignment exists.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="title"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Title</FormLabel>
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
                    <Textarea {...field} value={field.value ?? ""} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="deadline"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Deadline</FormLabel>
                    <FormControl>
                      <Input type="datetime-local" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="maxMarks"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Total marks</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min={0}
                        step="any"
                        value={field.value ?? ""}
                        onChange={(event) =>
                          field.onChange(
                            event.target.value === "" ? undefined : event.target.valueAsNumber
                          )
                        }
                        onBlur={field.onBlur}
                        name={field.name}
                        ref={field.ref}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
            <FormField
              control={form.control}
              name="allowLateSubmission"
              render={({ field }) => (
                <FormItem className="flex items-start justify-between gap-4 rounded-lg border p-4">
                  <div className="space-y-1">
                    <FormLabel>Allow late submission</FormLabel>
                    <FormDescription>
                      Students can still submit after the deadline, flagged as late.
                    </FormDescription>
                  </div>
                  <FormControl>
                    <Switch
                      checked={field.value}
                      onCheckedChange={field.onChange}
                      aria-label="Allow late submission"
                    />
                  </FormControl>
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