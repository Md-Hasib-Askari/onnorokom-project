import type { Metadata } from "next";
import { RegisterForm } from "@/components/auth/register-form";
import { pageTitle } from "@/lib/app";

export const metadata: Metadata = {
  title: pageTitle("Register"),
};

export default function RegisterPage() {
  return <RegisterForm />;
}
