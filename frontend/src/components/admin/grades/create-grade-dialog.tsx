"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PlusIcon } from "lucide-react";
import { toast } from "sonner";

import {
  gradeCreateRequestSchema,
  type GradeCreateRequest,
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
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

export function CreateGradeDialog() {
  const [open, setOpen] = useState(false);
  const mutation = AdminGradeMutations.useCreate();

  const form = useForm<GradeCreateRequest>({
    resolver: zodResolver(gradeCreateRequestSchema),
    defaultValues: { name: "", academicYear: "", description: "" },
  });

  function onSubmit(values: GradeCreateRequest) {
    mutation.mutate(values, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Grade created.");
          setOpen(false);
          form.reset();
          return;
        }
        if (result.fieldErrors) {
          for (const [field, message] of Object.entries(result.fieldErrors)) {
            form.setError(field as keyof GradeCreateRequest, { message });
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
          New grade
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create grade</DialogTitle>
          <DialogDescription>Grades group subjects and students by academic year.</DialogDescription>
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
                    <Input {...field} placeholder="2025-2026" />
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
                {mutation.isPending ? "Creating..." : "Create grade"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}