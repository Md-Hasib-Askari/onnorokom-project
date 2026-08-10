"use client";

import Link from "next/link";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { CircleAlertIcon } from "lucide-react";

import { passwordSchema, selfRegisterRoleSchema } from "@/lib/api/schemas/auth.schema";
import type { SelfRegisterRole } from "@/lib/api/schemas/auth.schema";
import { emailSchema, fullNameSchema, UserRole } from "@/lib/api/schemas/common.schema";
import type { RegistrationPolicy } from "@/lib/api/schemas/settings.schema";
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
    role: selfRegisterRoleSchema,
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: VALIDATION_MESSAGES.passwordsDoNotMatch,
    path: ["confirmPassword"],
  });

type RegisterFormValues = z.infer<typeof registerFormSchema>;

/** Label shown for each role a visitor may pick. */
const ROLE_LABELS: Record<SelfRegisterRole, string> = {
  [UserRole.Teacher]: "Teacher",
  [UserRole.Student]: "Student",
};

/** Sentence fragment appended per role, so the description says what approval actually involves. */
const ROLE_APPROVAL_NOTES: Record<SelfRegisterRole, string> = {
  [UserRole.Teacher]: "Teacher accounts are approved by an administrator before you can sign in.",
  [UserRole.Student]:
    "Student accounts are approved by an administrator, who also assigns your section.",
};

export function RegisterForm({ policy }: { policy: RegistrationPolicy }) {
  const openRoles = rolesOpenFor(policy);

  if (openRoles.length === 0) {
    return <RegistrationClosedCard />;
  }

  // The default role, and therefore the form's initial state, depends on what is open. Remounting
  // on a change keeps the defaults honest without an effect that resyncs them.
  return <OpenRegisterForm key={openRoles.join("-")} openRoles={openRoles} />;
}

function OpenRegisterForm({ openRoles }: { openRoles: SelfRegisterRole[] }) {
  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerFormSchema),
    defaultValues: {
      fullName: "",
      email: "",
      password: "",
      confirmPassword: "",
      role: openRoles[0],
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
  const selectedRole = useWatch({ control: form.control, name: "role" });

  return (
    <Card className="shadow-sm">
      <CardHeader className="gap-1.5">
        <CardTitle className="text-xl">Create an account</CardTitle>
        <CardDescription>{ROLE_APPROVAL_NOTES[selectedRole]}</CardDescription>
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
            <FormField
              control={form.control}
              name="role"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>I am a</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={openRoles.length === 1}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {openRoles.map((role) => (
                        <SelectItem key={role} value={role}>
                          {ROLE_LABELS[role]}
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
        <SignInPrompt />
      </CardContent>
    </Card>
  );
}

/**
 * Shown when an admin has closed sign-up to both roles. There is no form to fall back to: the
 * backend would refuse every role, so offering one would only produce a failed submit.
 */
function RegistrationClosedCard() {
  return (
    <Card className="shadow-sm">
      <CardHeader className="gap-1.5">
        <CardTitle className="text-xl">Sign-up is closed</CardTitle>
        <CardDescription>
          New accounts are not open to the public right now. Ask an administrator to create one for
          you.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <SignInPrompt />
      </CardContent>
    </Card>
  );
}

function SignInPrompt() {
  return (
    <p className="mt-4 text-center text-sm text-muted-foreground">
      Already have an account?{" "}
      <Link href={ROUTES.login} className="underline underline-offset-4">
        Sign in
      </Link>
    </p>
  );
}

function rolesOpenFor(policy: RegistrationPolicy): SelfRegisterRole[] {
  const roles: SelfRegisterRole[] = [];
  if (policy.teacherSelfRegistrationEnabled) roles.push(UserRole.Teacher);
  if (policy.studentSelfRegistrationEnabled) roles.push(UserRole.Student);
  return roles;
}
