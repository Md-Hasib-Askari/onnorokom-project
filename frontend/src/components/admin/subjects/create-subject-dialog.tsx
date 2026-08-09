"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PlusIcon } from "lucide-react";
import { toast } from "sonner";

import {
  subjectCreateRequestSchema,
  type SubjectCreateRequest,
} from "@/lib/api/schemas/admin-subjects.schema";
import { isEligibleTeacher } from "@/lib/eligible-teacher";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSubjectMutations } from "@/lib/mutations/admin-subjects.mutations";
import { AdminGradeQueries } from "@/lib/queries/admin-grades.queries";
import { AdminUserQueries } from "@/lib/queries/admin-users.queries";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

/** Radix `SelectItem` rejects an empty-string value, so the "no teacher yet" option uses this sentinel. */
const NO_TEACHER_VALUE = "none";

export function CreateSubjectDialog() {
  const [open, setOpen] = useState(false);
  const grades = AdminGradeQueries.useCurrentYearList();
  const users = AdminUserQueries.useList();
  const mutation = AdminSubjectMutations.useCreate();

  const eligibleTeachers = (users.data ?? []).filter(isEligibleTeacher);

  const form = useForm<SubjectCreateRequest>({
    resolver: zodResolver(subjectCreateRequestSchema),
    defaultValues: { name: "", gradeId: "", code: "", teacherId: "" },
  });

  function onSubmit(values: SubjectCreateRequest) {
    mutation.mutate(values, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Subject created.");
          setOpen(false);
          form.reset();
          return;
        }
        if (result.fieldErrors) {
          for (const [field, message] of Object.entries(result.fieldErrors)) {
            form.setError(field as keyof SubjectCreateRequest, { message });
          }
        } else {
          toast.error(result.error ?? ERROR_MESSAGES.generic);
        }
      },
      onError: () => {
        toast.error(ERROR_MESSAGES.genericRetry);
      },
    });
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (!next) form.reset();
      }}
    >
      <DialogTrigger asChild>
        <Button size="sm">
          <PlusIcon className="size-4" />
          New subject
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create subject</DialogTitle>
          <DialogDescription>
            Optionally assign a teacher now, or later from the subject&apos;s row menu.
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
            <FormField
              control={form.control}
              name="teacherId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Teacher</FormLabel>
                  <Select
                    value={field.value || NO_TEACHER_VALUE}
                    onValueChange={(value) =>
                      field.onChange(value === NO_TEACHER_VALUE ? "" : value)
                    }
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={users.isLoading ? "Loading..." : "Select a teacher"} />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value={NO_TEACHER_VALUE}>No teacher yet</SelectItem>
                      {eligibleTeachers.map((teacher) => (
                        <SelectItem key={teacher.id} value={teacher.id}>
                          {teacher.fullName}
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
                {mutation.isPending ? "Creating..." : "Create subject"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}