"use client";

import { useRouter } from "next/navigation";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PlusIcon } from "lucide-react";
import { toast } from "sonner";

import {
  assignmentCreateRequestSchema,
  type AssignmentCreateRequest,
} from "@/lib/api/schemas/teacher.schema";
import { toIsoInstant } from "@/lib/datetime";
import { ERROR_MESSAGES, SELECT_PLACEHOLDERS } from "@/lib/messages";
import { TeacherMutations } from "@/lib/mutations/teacher.mutations";
import { TeacherQueries } from "@/lib/queries/teacher.queries";
import { ROUTE_BUILDERS } from "@/lib/routes";
import { sectionOptions, subjectOptions } from "@/lib/teacher-sections";
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
  DialogTrigger,
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
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

/** Marks a fresh assignment starts on, so the common case needs no typing. */
const DEFAULT_MAX_MARKS = 100;

export interface AssignmentTarget {
  sectionId: string;
  subjectId: string;
}

interface CreateAssignmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** Preselects the class and subject when the teacher arrives from "My subjects". */
  defaultTarget?: AssignmentTarget;
}

function emptyValues(defaultTarget?: AssignmentTarget): AssignmentCreateRequest {
  return {
    title: "",
    description: "",
    sectionId: defaultTarget?.sectionId ?? "",
    subjectId: defaultTarget?.subjectId ?? "",
    deadline: "",
    maxMarks: DEFAULT_MAX_MARKS,
    allowLateSubmission: false,
  };
}

export function CreateAssignmentDialog({
  open,
  onOpenChange,
  defaultTarget,
}: CreateAssignmentDialogProps) {
  const router = useRouter();
  const sectionSubjects = TeacherQueries.useSectionSubjects();
  const mutation = TeacherMutations.useCreateAssignment();

  const form = useForm<AssignmentCreateRequest>({
    resolver: zodResolver(assignmentCreateRequestSchema),
    defaultValues: emptyValues(defaultTarget),
  });

  const pairs = sectionSubjects.data ?? [];
  const selectedSectionId = useWatch({ control: form.control, name: "sectionId" });
  const sections = sectionOptions(pairs);
  const subjects = subjectOptions(pairs, selectedSectionId);

  function onSubmit(values: AssignmentCreateRequest) {
    // The form works in the browser's wall clock; the API stores an instant.
    mutation.mutate(
      { ...values, deadline: toIsoInstant(values.deadline) },
      {
        onSuccess: (result) => {
          if (result.success && result.assignmentId) {
            toast.success("Assignment created as a draft.");
            onOpenChange(false);
            form.reset(emptyValues(defaultTarget));
            router.push(ROUTE_BUILDERS.teacherAssignment(result.assignmentId));
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof AssignmentCreateRequest, { message });
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

  function sectionPlaceholder() {
    if (sectionSubjects.isLoading) return SELECT_PLACEHOLDERS.loading;
    return sections.length > 0 ? SELECT_PLACEHOLDERS.section : SELECT_PLACEHOLDERS.sectionUnassigned;
  }

  function subjectPlaceholder() {
    if (!selectedSectionId) return SELECT_PLACEHOLDERS.subjectNeedsSection;
    return subjects.length > 0 ? SELECT_PLACEHOLDERS.subject : SELECT_PLACEHOLDERS.subjectNone;
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        onOpenChange(next);
        if (!next) form.reset(emptyValues(defaultTarget));
      }}
    >
      <DialogTrigger asChild>
        <Button size="sm">
          <PlusIcon className="size-4" />
          New assignment
        </Button>
      </DialogTrigger>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Create assignment</DialogTitle>
          <DialogDescription>
            Assignments start as drafts. Students only see one once it is published.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="sectionId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Class</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={(next) => {
                      field.onChange(next);
                      // The subject list is scoped to the class, so an old pick cannot survive.
                      form.setValue("subjectId", "");
                    }}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={sectionPlaceholder()} />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {sections.map((section) => (
                        <SelectItem key={section.id} value={section.id}>
                          {section.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="subjectId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Subject</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={!selectedSectionId || subjects.length === 0}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={subjectPlaceholder()} />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {subjects.map((subject) => (
                        <SelectItem key={subject.id} value={subject.id}>
                          {subject.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
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
                    <FormLabel>Max marks</FormLabel>
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
                {mutation.isPending ? "Creating..." : "Create assignment"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}