"use client";

import { useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CircleAlertIcon, MailCheckIcon } from "lucide-react";

import { forgotPasswordRequestSchema, type ForgotPasswordRequest } from "@/lib/api/schemas/auth.schema";
import { AuthMutations } from "@/lib/mutations/auth.mutations";
import { ROUTES } from "@/lib/routes";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";

export function ForgotPasswordForm() {
  const [sentTo, setSentTo] = useState<string | null>(null);

  if (sentTo) {
    return <CheckEmailCard email={sentTo} />;
  }

  return <RequestCodeCard onSent={setSentTo} />;
}

function RequestCodeCard({ onSent }: { onSent: (email: string) => void }) {
  const form = useForm<ForgotPasswordRequest>({
    resolver: zodResolver(forgotPasswordRequestSchema),
    defaultValues: { email: "" },
  });

  const mutation = AuthMutations.useForgotPassword();

  function onSubmit(values: ForgotPasswordRequest) {
    mutation.mutate(values, {
      onSuccess: (result) => {
        if (result.success) {
          onSent(values.email);
        } else if (result.fieldErrors) {
          for (const [field, message] of Object.entries(result.fieldErrors)) {
            form.setError(field as keyof ForgotPasswordRequest, { message });
          }
        }
      },
    });
  }

  const topLevelError = mutation.data && !mutation.data.success ? mutation.data.error : undefined;

  return (
    <Card className="shadow-sm">
      <CardHeader className="gap-1.5">
        <CardTitle className="text-xl">Forgot password</CardTitle>
        <CardDescription>Enter your email and we&apos;ll send you a reset code.</CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5" noValidate>
            {topLevelError && (
              <Alert variant="destructive">
                <CircleAlertIcon className="size-4" />
                <AlertTitle>Couldn&apos;t send code</AlertTitle>
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
            <Button type="submit" className="w-full" disabled={mutation.isPending}>
              {mutation.isPending ? "Sending..." : "Send reset code"}
            </Button>
          </form>
        </Form>
        <p className="mt-4 text-center text-sm text-muted-foreground">
          Remembered your password?{" "}
          <Link href={ROUTES.login} className="underline underline-offset-4">
            Sign in
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}

function CheckEmailCard({ email }: { email: string }) {
  return (
    <Card className="shadow-sm">
      <CardHeader className="gap-1.5">
        <CardTitle className="text-xl">Check your email</CardTitle>
        <CardDescription>
          If an account exists for {email}, we&apos;ve sent a 6-digit reset code. It expires in 10
          minutes.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <Alert>
          <MailCheckIcon className="size-4" />
          <AlertTitle>Code on its way</AlertTitle>
          <AlertDescription>Have it handy on the next screen.</AlertDescription>
        </Alert>
        <Button asChild className="w-full">
          <Link href={`${ROUTES.resetPassword}?email=${encodeURIComponent(email)}`}>
            Enter reset code
          </Link>
        </Button>
      </CardContent>
    </Card>
  );
}