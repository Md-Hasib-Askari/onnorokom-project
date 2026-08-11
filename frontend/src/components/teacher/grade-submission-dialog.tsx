"use client";

import { useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  buildGradeSubmissionSchema,
  type GradeSubmissionRequest,
  type TeacherSubmission,
} from "@/lib/api/schemas/teacher.schema";
import { formatDateTime } from "@/lib/datetime";
import { ERROR_MESSAGES } from "@/lib/messages";
import { TeacherMutations } from "@/lib/mutations/teacher.mutations";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

interface GradeSubmissionDialogProps {
  submission: TeacherSubmission | null;
  assignmentId: string;
  maxMarks: number;
  onOpenChange: (open: boolean) => void;
}

export function GradeSubmissionDialog({
  submission,
  assignmentId,
  maxMarks,
  onOpenChange,
}: GradeSubmissionDialogProps) {
  const mutation = TeacherMutations.useGradeSubmission();
  const schema = useMemo(() => buildGradeSubmissionSchema(maxMarks), [maxMarks]);

  const form = useForm<GradeSubmissionRequest>({
    resolver: zodResolver(schema),
    defaultValues: { marks: 0, feedback: "" },
  });

  useEffect(() => {
    if (!submission) return;
    form.reset({ marks: submission.marks ?? 0, feedback: submission.feedback ?? "" });
  }, [submission, form]);

  if (!submission) return null;

  function onSubmit(values: GradeSubmissionRequest) {
    if (!submission) return;
    mutation.mutate(
      { assignmentId, submissionId: submission.id, values },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Submission graded.");
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof GradeSubmissionRequest, { message });
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
    <Dialog open={!!submission} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Grade {submission.studentName}</DialogTitle>
          <DialogDescription>
            Submitted {formatDateTime(submission.submittedAt)}
            {submission.isLate ? " (late)" : ""}.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2 rounded-lg border p-4">
          <p className="text-sm font-medium">Submitted work</p>
          <p className="text-sm whitespace-pre-wrap text-muted-foreground">
            {submission.content?.trim() ? submission.content : "No written answer."}
          </p>
          {submission.attachmentUrl ? (
            <a
              href={submission.attachmentUrl}
              target="_blank"
              rel="noreferrer"
              className="text-sm text-primary underline-offset-4 hover:underline"
            >
              Open attachment
            </a>
          ) : null}
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="marks"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Marks (out of {maxMarks})</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={0}
                      max={maxMarks}
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
            <FormField
              control={form.control}
              name="feedback"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Feedback</FormLabel>
                  <FormControl>
                    <Textarea {...field} value={field.value ?? ""} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <DialogFooter>
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending ? "Saving..." : "Save grade"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}