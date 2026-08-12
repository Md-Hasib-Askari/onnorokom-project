"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  EDITABLE_ACCOUNT_STATUSES,
  adminUpdateUserSchemaFor,
  type AdminUpdateUserRequest,
  type AdminUserSummary,
} from "@/lib/api/schemas/admin-users.schema";
import { AccountStatus, UserRole } from "@/lib/api/schemas/common.schema";
import { ERROR_MESSAGES, SELECT_PLACEHOLDERS } from "@/lib/messages";
import { AdminUserMutations } from "@/lib/mutations/admin-users.mutations";
import { AdminGradeQueries } from "@/lib/queries/admin-grades.queries";
import { AdminSectionQueries } from "@/lib/queries/admin-sections.queries";
import { SectionSelect } from "@/components/admin/users/section-select";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
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

interface EditUserDialogProps {
  user: AdminUserSummary | null;
  onOpenChange: (open: boolean) => void;
}

export function EditUserDialog({ user, onOpenChange }: EditUserDialogProps) {
  const grades = AdminGradeQueries.useCurrentYearList();
  const allSections = AdminSectionQueries.useList();
  const mutation = AdminUserMutations.useUpdate();

  // Adjusts state when the `user` prop changes, per React's guidance on resetting
  // state on prop change (done during render, not in an effect, to avoid cascading renders).
  const [prevUserId, setPrevUserId] = useState(user?.id);
  const [gradeOverride, setGradeOverride] = useState<string | undefined>(undefined);
  if (user?.id !== prevUserId) {
    setPrevUserId(user?.id);
    setGradeOverride(undefined);
  }

  const derivedGradeId = allSections.data?.find((section) => section.id === user?.studentSectionId)?.gradeId;
  const studentGradeId = gradeOverride ?? derivedGradeId;

  const form = useForm<AdminUpdateUserRequest>({
    resolver: user ? zodResolver(adminUpdateUserSchemaFor(user.role)) : undefined,
    defaultValues: {
      fullName: "",
      email: "",
      status: AccountStatus.Approved,
      isActive: true,
      studentSectionId: undefined,
      teacherProfile: { teacherCode: "" },
    },
  });

  useEffect(() => {
    if (!user) return;
    form.reset({
      fullName: user.fullName,
      email: user.email,
      status: user.status === AccountStatus.Rejected ? AccountStatus.Rejected : AccountStatus.Approved,
      isActive: user.isActive,
      studentSectionId: user.studentSectionId ?? undefined,
      teacherProfile: { teacherCode: user.teacherCode ?? "" },
    });
  }, [user, form]);

  if (!user) return null;

  function onSubmit(values: AdminUpdateUserRequest) {
    if (!user) return;
    mutation.mutate(
      { id: user.id, role: user.role, values },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("User updated.");
            onOpenChange(false);
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              form.setError(field as keyof AdminUpdateUserRequest, { message });
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
    <Dialog open={!!user} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            Edit user
            <Badge variant="outline">{user.role}</Badge>
          </DialogTitle>
          <DialogDescription>Update account details, status, and activation.</DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="fullName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Full name</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input type="email" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="status"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Status</FormLabel>
                  <Select value={field.value} onValueChange={field.onChange}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {EDITABLE_ACCOUNT_STATUSES.map((status) => (
                        <SelectItem key={status} value={status}>
                          {status}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            {user.role === UserRole.Teacher && (
              <FormField
                control={form.control}
                name="teacherProfile.teacherCode"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Teacher code</FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}
            {user.role === UserRole.Student && (
              <>
                <FormItem>
                  <FormLabel>Grade</FormLabel>
                  <Select
                    value={studentGradeId ?? ""}
                    onValueChange={(value) => {
                      setGradeOverride(value);
                      form.setValue("studentSectionId", undefined);
                    }}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue
                          placeholder={
                            grades.isLoading ? SELECT_PLACEHOLDERS.loading : SELECT_PLACEHOLDERS.grade
                          }
                        />
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
                </FormItem>
                <FormField
                  control={form.control}
                  name="studentSectionId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Section</FormLabel>
                      <SectionSelect
                        gradeId={studentGradeId}
                        value={field.value}
                        onValueChange={field.onChange}
                      />
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </>
            )}
            <FormField
              control={form.control}
              name="isActive"
              render={({ field }) => (
                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3">
                  <div className="space-y-0.5">
                    <FormLabel>Active</FormLabel>
                    <p className="text-sm text-muted-foreground">
                      Inactive accounts cannot sign in even if approved.
                    </p>
                  </div>
                  <FormControl>
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
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