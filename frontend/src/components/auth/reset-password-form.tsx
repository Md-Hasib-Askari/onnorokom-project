"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { CircleAlertIcon } from "lucide-react";

import { emailSchema } from "@/lib/api/schemas/common.schema";
import { passwordSchema, resetCodeSchema } from "@/lib/api/schemas/auth.schema";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { AuthMutations } from "@/lib/mutations/auth.mutations";
import { ROUTES } from "@/lib/routes";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";

const resetPasswordFormSchema = z
  .object({
    email: emailSchema,
    code: resetCodeSchema,
    newPassword: passwordSchema,
    confirmPassword: z.string().min(1, VALIDATION_MESSAGES.confirmPasswordRequired),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: VALIDATION_MESSAGES.passwordsDoNotMatch,
    path: ["confirmPassword"],
  });

type ResetPasswordFormValues = z.infer<typeof resetPasswordFormSchema>;

export function ResetPasswordForm() {
  const searchParams = useSearchParams();
  const emailFromQuery = searchParams.get("email") ?? "";

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordFormSchema),
    defaultValues: { email: emailFromQuery, code: "", newPassword: "", confirmPassword: "" },
  });

  const mutation = AuthMutations.useResetPassword();

  function onSubmit(values: ResetPasswordFormValues) {
    mutation.mutate(
      { email: values.email, code: values.code, newPassword: values.newPassword },
      {
        onSuccess: (result) => {
          if (!result.success && result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              if (field in form.getValues()) form.setError(field as keyof ResetPasswordFormValues, { message });
            }
          }
        },
      }
    );
  }

  const topLevelError = mutation.data && !mutation.data.success ? mutation.data.error : undefined;

  return (
    <Card className="shadow-sm">
      <CardHeader className="gap-1.5">
        <CardTitle className="text-xl">Reset password</CardTitle>
        <CardDescription>Enter the code we emailed you along with your new password.</CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5" noValidate>
            {topLevelError && (
              <Alert variant="destructive">
                <CircleAlertIcon className="size-4" />
                <AlertTitle>Couldn&apos;t reset password</AlertTitle>
                <AlertDescription>{topLevelError}</AlertDescription>
              </Alert>
            )}
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input type="email" autoComplete="email" placeholder="you@school.edu" {...field} />
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
                  <FormLabel>Reset code</FormLabel>
                  <FormControl>
                    <Input
                      inputMode="numeric"
                      autoComplete="one-time-code"
                      placeholder="123456"
                      {...field}
                    />
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
            <Button type="submit" className="w-full" disabled={mutation.isPending}>
              {mutation.isPending ? "Resetting..." : "Reset password"}
            </Button>
          </form>
        </Form>
        <p className="mt-4 text-center text-sm text-muted-foreground">
          Didn&apos;t get a code?{" "}
          <Link href={ROUTES.forgotPassword} className="underline underline-offset-4">
            Request another
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}