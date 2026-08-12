"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";

import type { Profile } from "@/lib/api/schemas/profile.schema";
import { updateProfileRequestSchema, type UpdateProfileRequest } from "@/lib/api/schemas/profile.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
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

export function EditProfileForm({ profile }: { profile: Profile }) {
  const form = useForm<UpdateProfileRequest>({
    resolver: zodResolver(updateProfileRequestSchema),
    defaultValues: { fullName: profile.fullName },
  });

  const mutation = ProfileMutations.useUpdate();

  function onSubmit(values: UpdateProfileRequest) {
    mutation.mutate(values, {
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

  return (
    <Card>
      <CardHeader>
        <CardTitle>Profile</CardTitle>
        <CardDescription>
          Only your full name can be changed here. Your email and role are managed by an
          administrator.
        </CardDescription>
      </CardHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
          <CardContent className="space-y-4">
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
            <div className="space-y-1.5">
              <Label className="text-muted-foreground">Email</Label>
              <p className="text-sm">{profile.email}</p>
            </div>
            <div className="space-y-1.5">
              <Label className="text-muted-foreground">Role</Label>
              <p className="text-sm">{profile.role}</p>
            </div>
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