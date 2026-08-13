"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import {
  EDITABLE_ACCOUNT_STATUSES,
  GENDERS,
  adminUpdateUserSchemaFor,
  type AdminUpdateUserRequest,
  type AdminUserSummary,
} from "@/lib/api/schemas/admin-users.schema";
import { AccountStatus, UserRole } from "@/lib/api/schemas/common.schema";
import { ERROR_MESSAGES, SELECT_PLACEHOLDERS } from "@/lib/messages";
import { AdminUserMutations } from "@/lib/mutations/admin-users.mutations";
import { AdminGradeQueries } from "@/lib/queries/admin-grades.queries";
import { AdminSectionQueries } from "@/lib/queries/admin-sections.queries";
import { AdminUserQueries } from "@/lib/queries/admin-users.queries";
import { SectionSelect } from "@/components/admin/users/section-select";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
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

/** Converts an ISO datetime string to the `yyyy-MM-dd` shape `<input type="date">` expects. */
function toDateInputValue(value: string | null | undefined): string {
  return value ? value.slice(0, 10) : "";
}

function SectionHeader({ children }: { children: React.ReactNode }) {
  return (
    <h4 className="border-b pb-1.5 text-[0.7rem] font-semibold tracking-wider text-foreground/70 uppercase">
      {children}
    </h4>
  );
}

export function EditUserDialog({ user, onOpenChange }: EditUserDialogProps) {
  const grades = AdminGradeQueries.useCurrentYearList();
  const allSections = AdminSectionQueries.useList();
  const detailQuery = AdminUserQueries.useDetail(user?.id);
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
      studentProfile: {
        rollNumber: "",
        dateOfBirth: "",
        gender: undefined,
        guardianName: "",
        guardianPhone: "",
        address: "",
        admissionDate: "",
      },
      teacherProfile: {
        teacherCode: "",
        department: "",
        designation: "",
        qualification: "",
        phoneNumber: "",
        address: "",
        dateOfJoining: "",
      },
      adminProfile: { position: "", phoneNumber: "" },
    },
  });

  const detail = detailQuery.data;

  useEffect(() => {
    if (!user || !detail) return;
    form.reset({
      fullName: detail.fullName,
      email: detail.email,
      status: detail.status === AccountStatus.Rejected ? AccountStatus.Rejected : AccountStatus.Approved,
      isActive: detail.isActive,
      studentSectionId: detail.studentProfile?.sectionId ?? undefined,
      studentProfile: {
        rollNumber: detail.studentProfile?.rollNumber ?? "",
        dateOfBirth: toDateInputValue(detail.studentProfile?.dateOfBirth),
        gender: detail.studentProfile?.gender ?? undefined,
        guardianName: detail.studentProfile?.guardianName ?? "",
        guardianPhone: detail.studentProfile?.guardianPhone ?? "",
        address: detail.studentProfile?.address ?? "",
        admissionDate: toDateInputValue(detail.studentProfile?.admissionDate),
      },
      teacherProfile: {
        teacherCode: detail.teacherProfile?.teacherCode ?? "",
        department: detail.teacherProfile?.department ?? "",
        designation: detail.teacherProfile?.designation ?? "",
        qualification: detail.teacherProfile?.qualification ?? "",
        phoneNumber: detail.teacherProfile?.phoneNumber ?? "",
        address: detail.teacherProfile?.address ?? "",
        dateOfJoining: toDateInputValue(detail.teacherProfile?.dateOfJoining),
      },
      adminProfile: {
        position: detail.adminProfile?.position ?? "",
        phoneNumber: detail.adminProfile?.phoneNumber ?? "",
      },
    });
  }, [user, detail, form]);

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
      <DialogContent className="sm:max-w-xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            Edit user
            <Badge variant="outline">{user.role}</Badge>
          </DialogTitle>
          <DialogDescription>Update account details, status, and activation.</DialogDescription>
        </DialogHeader>
        {detailQuery.isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        ) : (
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
            <div className="space-y-6">
              <section className="space-y-4">
                <h3 className="border-b pb-2 text-sm font-semibold text-foreground">Account</h3>
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
                  <FormField
                    control={form.control}
                    name="isActive"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Active</FormLabel>
                        <div className="flex items-center justify-between gap-3">
                          <p className="text-sm text-muted-foreground">
                            Inactive accounts cannot sign in even if approved.
                          </p>
                          <FormControl>
                            <Switch checked={field.value} onCheckedChange={field.onChange} />
                          </FormControl>
                        </div>
                      </FormItem>
                    )}
                  />
                </div>
              </section>

              {(user.role === UserRole.Teacher ||
                user.role === UserRole.Student ||
                user.role === UserRole.Admin) && (
                <>
                  <section className="space-y-6">
                    <h3 className="border-b pb-2 text-sm font-semibold text-foreground">Profile</h3>

                    {user.role === UserRole.Teacher && (
                      <>
                        <div className="space-y-4">
                          <SectionHeader>Academic</SectionHeader>
                          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
                            <FormField
                              control={form.control}
                              name="teacherProfile.department"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Department</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="teacherProfile.designation"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Designation</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="teacherProfile.qualification"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Qualification</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="teacherProfile.dateOfJoining"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Date of joining</FormLabel>
                                  <FormControl>
                                    <Input type="date" {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                          </div>
                        </div>
                        <div className="space-y-4">
                          <SectionHeader>Contact</SectionHeader>
                          <div className="space-y-4">
                            <FormField
                              control={form.control}
                              name="teacherProfile.phoneNumber"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Phone number</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="teacherProfile.address"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Address</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                          </div>
                        </div>
                      </>
                    )}

                    {user.role === UserRole.Admin && (
                      <div className="space-y-4">
                        <SectionHeader>Contact</SectionHeader>
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                          <FormField
                            control={form.control}
                            name="adminProfile.position"
                            render={({ field }) => (
                              <FormItem>
                                <FormLabel>Position</FormLabel>
                                <FormControl>
                                  <Input {...field} />
                                </FormControl>
                                <FormMessage />
                              </FormItem>
                            )}
                          />
                          <FormField
                            control={form.control}
                            name="adminProfile.phoneNumber"
                            render={({ field }) => (
                              <FormItem>
                                <FormLabel>Phone number</FormLabel>
                                <FormControl>
                                  <Input {...field} />
                                </FormControl>
                                <FormMessage />
                              </FormItem>
                            )}
                          />
                        </div>
                      </div>
                    )}

                    {user.role === UserRole.Student && (
                      <>
                        <div className="space-y-4">
                          <SectionHeader>Academic</SectionHeader>
                          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
                            <FormField
                              control={form.control}
                              name="studentProfile.rollNumber"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Roll number</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="studentProfile.admissionDate"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Admission date</FormLabel>
                                  <FormControl>
                                    <Input type="date" {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                          </div>
                        </div>

                        <div className="space-y-4">
                          <SectionHeader>Personal</SectionHeader>
                          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <FormField
                              control={form.control}
                              name="studentProfile.dateOfBirth"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Date of birth</FormLabel>
                                  <FormControl>
                                    <Input type="date" {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="studentProfile.gender"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Gender</FormLabel>
                                  <Select value={field.value ?? ""} onValueChange={field.onChange}>
                                    <FormControl>
                                      <SelectTrigger className="w-full">
                                        <SelectValue placeholder={SELECT_PLACEHOLDERS.gender} />
                                      </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                      {GENDERS.map((gender) => (
                                        <SelectItem key={gender} value={gender}>
                                          {gender}
                                        </SelectItem>
                                      ))}
                                    </SelectContent>
                                  </Select>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                          </div>
                        </div>

                        <div className="space-y-4">
                          <SectionHeader>Contact</SectionHeader>
                          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <FormField
                              control={form.control}
                              name="studentProfile.guardianName"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Guardian name</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="studentProfile.guardianPhone"
                              render={({ field }) => (
                                <FormItem>
                                  <FormLabel>Guardian phone</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                            <FormField
                              control={form.control}
                              name="studentProfile.address"
                              render={({ field }) => (
                                <FormItem className="sm:col-span-2">
                                  <FormLabel>Address</FormLabel>
                                  <FormControl>
                                    <Input {...field} />
                                  </FormControl>
                                  <FormMessage />
                                </FormItem>
                              )}
                            />
                          </div>
                        </div>
                      </>
                    )}
                  </section>
                </>
              )}
            </div>
            <DialogFooter className="mt-6">
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending ? "Saving..." : "Save changes"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
        )}
      </DialogContent>
    </Dialog>
  );
}