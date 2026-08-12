import type { Metadata } from "next";
import { ForgotPasswordForm } from "@/components/auth/forgot-password-form";
import { pageTitle } from "@/lib/app";

export const metadata: Metadata = {
  title: pageTitle("Forgot password"),
};

export default function ForgotPasswordPage() {
  return <ForgotPasswordForm />;
}