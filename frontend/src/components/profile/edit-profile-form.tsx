"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import type { Profile } from "@/lib/api/schemas/profile.schema";
import { updateProfileRequestSchema, type UpdateProfileRequest } from "@/lib/api/schemas/profile.schema";
import { GENDERS } from "@/lib/api/schemas/admin-users.schema";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { EMPTY_CELL, ERROR_MESSAGES, SELECT_PLACEHOLDERS } from "@/lib/messages";
import { ProfileMutations } from "@/lib/mutations/profile.mutations";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

/** Converts an ISO datetime string to the `yyyy-MM-dd` shape `<input type="date">` expects. */
function toDateInputValue(value: string | null | undefined): string {
  return value ? value.slice(0, 10) : "";
}

function formatDate(value: string | null | undefined): string {
  return value ? new Date(value).toLocaleDateString() : EMPTY_CELL;
}

function ReadOnlyRow({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <div className="space-y-1.5">
      <Label className="text-muted-foreground">{label}</Label>
      <p className="text-sm">{value ?? EMPTY_CELL}</p>
    </div>
  );
}

function SectionHeader({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="border-b pb-1.5 text-[0.7rem] font-semibold tracking-wider text-foreground/70 uppercase">
      {children}
    </h3>
  );
}

export function EditProfileForm({ profile }: { profile: Profile }) {
  const form = useForm<UpdateProfileRequest>({
    resolver: zodResolver(updateProfileRequestSchema),
    defaultValues: {
      fullName: profile.fullName,
      studentProfile: {
        dateOfBirth: toDateInputValue(profile.studentProfile?.dateOfBirth),
        gender: profile.studentProfile?.gender ?? undefined,
        guardianName: profile.studentProfile?.guardianName ?? "",
        guardianPhone: profile.studentProfile?.guardianPhone ?? "",
        address: profile.studentProfile?.address ?? "",
      },
      teacherProfile: {
        department: profile.teacherProfile?.department ?? "",
        designation: profile.teacherProfile?.designation ?? "",
        qualification: profile.teacherProfile?.qualification ?? "",
        phoneNumber: profile.teacherProfile?.phoneNumber ?? "",
        address: profile.teacherProfile?.address ?? "",
      },
      adminProfile: {
        position: profile.adminProfile?.position ?? "",
        phoneNumber: profile.adminProfile?.phoneNumber ?? "",
      },
    },
  });

  const mutation = ProfileMutations.useUpdate(profile.id);

  function onSubmit(values: UpdateProfileRequest) {
    const payload: UpdateProfileRequest = {
      fullName: values.fullName,
      studentProfile: profile.role === UserRole.Student ? values.studentProfile : undefined,
      teacherProfile: profile.role === UserRole.Teacher ? values.teacherProfile : undefined,
      adminProfile: profile.role === UserRole.Admin ? values.adminProfile : undefined,
    };

    mutation.mutate(payload, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Profile updated.");
          form.reset(values);
          return;
        }
        toast.error(result.error ?? ERROR_MESSAGES.generic);
      },
      onError: () => toast.error(ERROR_MESSAGES.genericRetry),
    });
  }

  const isDirty = form.formState.isDirty;
  const canEditProfile = profile.canEditProfile;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Profile</CardTitle>
        <CardDescription>
          {canEditProfile
            ? "Update your account and profile details."
            : "Your full name can be changed here. An administrator has disabled editing for the rest of your profile."}
        </CardDescription>
      </CardHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
          <CardContent className="space-y-6">
            <div className="space-y-4">
              <SectionHeader>Account</SectionHeader>
              <FormField
                control={form.control}
                name="fullName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Full name</FormLabel>
                    <FormControl>
                      <Input autoComplete="name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <ReadOnlyRow label="Email" value={profile.email} />
                <ReadOnlyRow label="Role" value={profile.role} />
              </div>
            </div>

            {profile.role === UserRole.Student && (
              <>
                <div className="space-y-6">
                  <div className="space-y-4">
                    <SectionHeader>Academic</SectionHeader>
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      <ReadOnlyRow label="Grade" value={profile.studentProfile?.gradeName} />
                      <ReadOnlyRow label="Section" value={profile.studentProfile?.sectionName} />
                      <ReadOnlyRow label="Roll number" value={profile.studentProfile?.rollNumber} />
                      <ReadOnlyRow label="Admission date" value={formatDate(profile.studentProfile?.admissionDate)} />
                    </div>
                  </div>

                  <div className="space-y-4">
                    <SectionHeader>Personal</SectionHeader>
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      {canEditProfile ? (
                        <>
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
                        </>
                      ) : (
                        <>
                          <ReadOnlyRow label="Date of birth" value={formatDate(profile.studentProfile?.dateOfBirth)} />
                          <ReadOnlyRow label="Gender" value={profile.studentProfile?.gender} />
                        </>
                      )}
                    </div>
                  </div>

                  <div className="space-y-4">
                    <SectionHeader>Contact</SectionHeader>
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      {canEditProfile ? (
                        <>
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
                        </>
                      ) : (
                        <>
                          <ReadOnlyRow label="Guardian name" value={profile.studentProfile?.guardianName} />
                          <ReadOnlyRow label="Guardian phone" value={profile.studentProfile?.guardianPhone} />
                          <ReadOnlyRow label="Address" value={profile.studentProfile?.address} />
                        </>
                      )}
                    </div>
                  </div>
                </div>
              </>
            )}

            {profile.role === UserRole.Teacher && (
              <>
                <div className="space-y-6">
                  <div className="space-y-4">
                    <SectionHeader>Academic</SectionHeader>
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      <ReadOnlyRow label="Teacher code" value={profile.teacherProfile?.teacherCode} />
                      <ReadOnlyRow label="Date of joining" value={formatDate(profile.teacherProfile?.dateOfJoining)} />
                      {canEditProfile ? (
                        <>
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
                        </>
                      ) : (
                        <>
                          <ReadOnlyRow label="Department" value={profile.teacherProfile?.department} />
                          <ReadOnlyRow label="Designation" value={profile.teacherProfile?.designation} />
                          <ReadOnlyRow label="Qualification" value={profile.teacherProfile?.qualification} />
                        </>
                      )}
                    </div>
                  </div>

                  <div className="space-y-4">
                    <SectionHeader>Contact</SectionHeader>
                    {canEditProfile ? (
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
                    ) : (
                      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                        <ReadOnlyRow label="Phone number" value={profile.teacherProfile?.phoneNumber} />
                        <ReadOnlyRow label="Address" value={profile.teacherProfile?.address} />
                      </div>
                    )}
                  </div>
                </div>
              </>
            )}

            {profile.role === UserRole.Admin && (
              <>
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
              </>
            )}
          </CardContent>
          <CardFooter className="justify-end">
            <Button type="submit" disabled={!isDirty || mutation.isPending}>
              {mutation.isPending ? "Saving..." : "Save changes"}
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}