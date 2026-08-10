"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PlusIcon } from "lucide-react";
import { toast } from "sonner";

import {
  sectionCreateRequestSchema,
  type SectionCreateRequest,
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

export function CreateSectionDialog() {
  const [open, setOpen] = useState(false);
  const grades = AdminGradeQueries.useCurrentYearList();
  const mutation = AdminSectionMutations.useCreate();

  const form = useForm<SectionCreateRequest>({
    resolver: zodResolver(sectionCreateRequestSchema),
    defaultValues: { name: "", gradeId: "" },
  });

  function onSubmit(values: SectionCreateRequest) {
    mutation.mutate(values, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Section created.");
          setOpen(false);
          form.reset();
          return;
        }
        if (result.fieldErrors) {
          for (const [field, message] of Object.entries(result.fieldErrors)) {
            form.setError(field as keyof SectionCreateRequest, { message });
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
          New section
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create section</DialogTitle>
          <DialogDescription>Sections split a grade into classes, e.g. Section A, B, C.</DialogDescription>
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
                    <Input {...field} placeholder="Section A" />
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
                {mutation.isPending ? "Creating..." : "Create section"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}