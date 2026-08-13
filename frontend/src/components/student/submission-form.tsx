"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import { SubmissionStatus } from "@/lib/api/schemas/admin-assignments.schema";
import {
  submissionRequestSchema,
  type StudentAssignmentDetail,
  type SubmissionRequest,
} from "@/lib/api/schemas/student.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { StudentMutations } from "@/lib/mutations/student.mutations";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

/**
 * Why the form is closed. The server decides *that* it is closed via `canSubmit`/`canEdit`; these
 * only explain it, so a wrong guess here can never let through a write the API would reject.
 */
const LOCK_REASONS = {
  graded: {
    title: "This work has been graded",
    description:
      "Your submission can no longer be changed. Ask your teacher to return it for revision if you need another attempt.",
  },
  deadlinePassed: {
    title: "The deadline has passed",
    description: "This assignment does not accept late submissions.",
  },
  closedByTeacher: {
    title: "Submissions are closed",
    description: "Your teacher has turned off submissions for this assignment.",
  },
} as const;

function lockReason(assignment: StudentAssignmentDetail) {
  if (assignment.submissionStatus === SubmissionStatus.Graded) return LOCK_REASONS.graded;
  if (!assignment.submissionsOpen) return LOCK_REASONS.closedByTeacher;
  return LOCK_REASONS.deadlinePassed;
}

export function SubmissionForm({ assignment }: { assignment: StudentAssignmentDetail }) {
  const submitMutation = StudentMutations.useSubmitAssignment();
  const updateMutation = StudentMutations.useUpdateSubmission();

  const isEditing = assignment.canEdit;
  const mutation = isEditing ? updateMutation : submitMutation;

  const form = useForm<SubmissionRequest>({
    resolver: zodResolver(submissionRequestSchema),
    defaultValues: { content: "", attachmentUrl: "" },
  });

  // The detail query refetches after every write, so the form re-seeds from whatever the server
  // now holds rather than from what was last typed.
  useEffect(() => {
    form.reset({
      content: assignment.content ?? "",
      attachmentUrl: assignment.attachmentUrl ?? "",
    });
  }, [assignment.content, assignment.attachmentUrl, form]);

  if (!assignment.canSubmit && !assignment.canEdit) {
    const reason = lockReason(assignment);
    return (
      <Alert>
        <AlertTitle>{reason.title}</AlertTitle>
        <AlertDescription>{reason.description}</AlertDescription>
      </Alert>
    );
  }

  function onSubmit(values: SubmissionRequest) {
    mutation.mutate(
      { assignmentId: assignment.id, values },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success(isEditing ? "Submission updated." : "Work submitted.");
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof SubmissionRequest, { message });
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
    <div className="space-y-4 rounded-lg border p-6">
      <div className="space-y-1">
        <h2 className="text-lg font-semibold tracking-tight">
          {isEditing ? "Edit your submission" : "Submit your work"}
        </h2>
        <p className="text-sm text-muted-foreground">
          {assignment.isPastDeadline
            ? "The deadline has passed, but late submissions are allowed for this assignment."
            : "Type your answer, attach a link, or do both."}
        </p>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <FormField
            control={form.control}
            name="content"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Your work</FormLabel>
                <FormControl>
                  <Textarea rows={8} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="attachmentUrl"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Attachment link</FormLabel>
                <FormControl>
                  <Input type="url" placeholder="https://" {...field} />
                </FormControl>
                <FormDescription>
                  Optional. Paste a link to a document your teacher can open.
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending
              ? "Saving..."
              : isEditing
                ? "Save changes"
                : "Submit"}
          </Button>
        </form>
      </Form>
    </div>
  );
}