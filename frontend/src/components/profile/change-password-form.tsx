"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { CircleAlertIcon } from "lucide-react";
import { toast } from "sonner";

import { passwordSchema } from "@/lib/api/schemas/auth.schema";
import { ERROR_MESSAGES, VALIDATION_MESSAGES } from "@/lib/messages";
import { ProfileMutations } from "@/lib/mutations/profile.mutations";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
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
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";

const changePasswordFormSchema = z
  .object({
    currentPassword: z.string().min(1, VALIDATION_MESSAGES.currentPasswordRequired),
    newPassword: passwordSchema,
    confirmPassword: z.string().min(1, VALIDATION_MESSAGES.confirmPasswordRequired),
  })
  .refine((data) => data.newPassword !== data.currentPassword, {
    message: VALIDATION_MESSAGES.newPasswordSameAsCurrent,
    path: ["newPassword"],
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: VALIDATION_MESSAGES.passwordsDoNotMatch,
    path: ["confirmPassword"],
  });

type ChangePasswordFormValues = z.infer<typeof changePasswordFormSchema>;

interface ChangePasswordFormProps {
  /** Called after a successful change, in addition to the default toast + reset. */
  onChanged?: () => void;
}

export function ChangePasswordForm({ onChanged }: ChangePasswordFormProps = {}) {
  const form = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordFormSchema),
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
  });

  const mutation = ProfileMutations.useChangePassword();

  function onSubmit(values: ChangePasswordFormValues) {
    mutation.mutate(
      { currentPassword: values.currentPassword, newPassword: values.newPassword },
      {
        onSuccess: (result) => {
          if (result.success) {
            toast.success("Password changed. Your other sessions have been signed out.");
            form.reset();
            onChanged?.();
            return;
          }
          if (result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              if (field in form.getValues()) {
                form.setError(field as keyof ChangePasswordFormValues, { message });
              }
            }
          }
        },
        onError: () => toast.error(ERROR_MESSAGES.genericRetry),
      }
    );
  }

  const topLevelError = mutation.data && !mutation.data.success ? mutation.data.error : undefined;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Change password</CardTitle>
        <CardDescription>
          Changing your password signs you out of every other device and browser.
        </CardDescription>
      </CardHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
          <CardContent className="space-y-4">
            {topLevelError && (
              <Alert variant="destructive">
                <CircleAlertIcon className="size-4" />
                <AlertTitle>Couldn&apos;t change password</AlertTitle>
                <AlertDescription>{topLevelError}</AlertDescription>
              </Alert>
            )}
            <FormField
              control={form.control}
              name="currentPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Current password</FormLabel>
                  <FormControl>
                    <Input type="password" autoComplete="current-password" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="newPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>New password</FormLabel>
                  <FormControl>
                    <Input type="password" autoComplete="new-password" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="confirmPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Confirm new password</FormLabel>
                  <FormControl>
                    <Input type="password" autoComplete="new-password" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </CardContent>
          <CardFooter className="justify-end">
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Changing password..." : "Change password"}
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}