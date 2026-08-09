import { Suspense } from "react";
import type { Metadata } from "next";
import { LoginForm } from "@/components/auth/login-form";
import { pageTitle } from "@/lib/app";

export const metadata: Metadata = {
  title: pageTitle("Sign in"),
};

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}
