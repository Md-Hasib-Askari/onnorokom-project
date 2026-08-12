"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CircleAlertIcon } from "lucide-react";

import { loginRequestSchema, type LoginRequest } from "@/lib/api/schemas/auth.schema";
import { AuthMutations } from "@/lib/mutations/auth.mutations";
import { NEXT_PATH_PARAM, ROUTES } from "@/lib/routes";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

export function LoginForm() {
  const searchParams = useSearchParams();
  const nextPath = searchParams.get(NEXT_PATH_PARAM) ?? undefined;

  const form = useForm<LoginRequest>({
    resolver: zodResolver(loginRequestSchema),
    defaultValues: { email: "", password: "" },
  });

  const mutation = AuthMutations.useLogin(nextPath);

  function onSubmit(values: LoginRequest) {
    mutation.mutate(values, {
      onSuccess: (result) => {
        if (!result.success && result.fieldErrors) {
          for (const [field, message] of Object.entries(result.fieldErrors)) {
            form.setError(field as keyof LoginRequest, { message });
          }
        }
      },
    });
  }

  const topLevelError = mutation.data && !mutation.data.success ? mutation.data.error : undefined;

  return (
    <Card className="shadow-sm">
      <CardHeader className="gap-1.5">
        <CardTitle className="text-xl">Sign in</CardTitle>
        <CardDescription>Enter your email and password to access your account.</CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5" noValidate>
            {topLevelError && (
              <Alert variant="destructive">
                <CircleAlertIcon className="size-4" />
                <AlertTitle>Couldn&apos;t sign in</AlertTitle>
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
              name="password"
              render={({ field }) => (
                <FormItem>
                  <div className="flex items-center justify-between">
                    <FormLabel>Password</FormLabel>
                    <Link
                      href={ROUTES.forgotPassword}
                      className="text-sm text-muted-foreground underline underline-offset-4"
                    >
                      Forgot password?
                    </Link>
                  </div>
                  <FormControl>
                    <Input type="password" autoComplete="current-password" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <Button type="submit" className="w-full" disabled={mutation.isPending}>
              {mutation.isPending ? "Signing in..." : "Sign in"}
            </Button>
          </form>
        </Form>
        <p className="mt-4 text-center text-sm text-muted-foreground">
          Don&apos;t have an account?{" "}
          <Link href={ROUTES.register} className="underline underline-offset-4">
            Register
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
