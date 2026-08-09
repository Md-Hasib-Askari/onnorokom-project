"use client";

import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { CircleAlertIcon } from "lucide-react";

import { passwordSchema } from "@/lib/api/schemas/auth.schema";
import { emailSchema, fullNameSchema, UserRole } from "@/lib/api/schemas/common.schema";
import { VALIDATION_MESSAGES } from "@/lib/messages";
import { AuthMutations } from "@/lib/mutations/auth.mutations";
import { ROUTES } from "@/lib/routes";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
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

const registerFormSchema = z
  .object({
    fullName: fullNameSchema,
    email: emailSchema,
    password: passwordSchema,
    confirmPassword: z.string().min(1, VALIDATION_MESSAGES.confirmPasswordRequired),
    role: z.literal(UserRole.Teacher),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: VALIDATION_MESSAGES.passwordsDoNotMatch,
    path: ["confirmPassword"],
  });

type RegisterFormValues = z.infer<typeof registerFormSchema>;

export function RegisterForm() {
  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerFormSchema),
    defaultValues: {
      fullName: "",
      email: "",
      password: "",
      confirmPassword: "",
      role: UserRole.Teacher,
    },
  });

  const mutation = AuthMutations.useRegister();

  function onSubmit(values: RegisterFormValues) {
    mutation.mutate(
      { fullName: values.fullName, email: values.email, password: values.password, role: values.role },
      {
        onSuccess: (result) => {
          if (!result.success && result.fieldErrors) {
            for (const [field, message] of Object.entries(result.fieldErrors)) {
              if (field in form.getValues()) form.setError(field as keyof RegisterFormValues, { message });
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
        <CardTitle className="text-xl">Create an account</CardTitle>
        <CardDescription>Teacher accounts are approved by an administrator before you can sign in.</CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5" noValidate>
            {topLevelError && (
              <Alert variant="destructive">
                <CircleAlertIcon className="size-4" />
                <AlertTitle>Couldn&apos;t register</AlertTitle>
                <AlertDescription>{topLevelError}</AlertDescription>
              </Alert>
            )}
            <FormField
              control={form.control}
              name="fullName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Full name</FormLabel>
                  <FormControl>
                    <Input autoComplete="name" placeholder="Jane Doe" {...field} />
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
                    <Input type="email" autoComplete="email" placeholder="you@school.edu" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormItem>
              <FormLabel>I am a</FormLabel>
              <Select value={UserRole.Teacher} disabled>
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={UserRole.Teacher}>Teacher</SelectItem>
                  <SelectItem value={UserRole.Student} disabled>
                    Student (ask an admin to create your account)
                  </SelectItem>
                </SelectContent>
              </Select>
            </FormItem>
            <FormField
              control={form.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Password</FormLabel>
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
                  <FormLabel>Confirm password</FormLabel>
                  <FormControl>
                    <Input type="password" autoComplete="new-password" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <Button type="submit" className="w-full" disabled={mutation.isPending}>
              {mutation.isPending ? "Creating account..." : "Create account"}
            </Button>
          </form>
        </Form>
        <p className="mt-4 text-center text-sm text-muted-foreground">
          Already have an account?{" "}
          <Link href={ROUTES.login} className="underline underline-offset-4">
            Sign in
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
