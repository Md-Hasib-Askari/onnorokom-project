import type { Metadata } from "next";
import { pageTitle } from "@/lib/app";
import { requireSession } from "@/lib/auth/session";
import { ForcedPasswordChangeForm } from "@/components/auth/forced-password-change-form";

export const metadata: Metadata = {
  title: pageTitle("Change password"),
};

export default async function ChangePasswordPage() {
  const session = await requireSession();
  return <ForcedPasswordChangeForm role={session.role} />;
}