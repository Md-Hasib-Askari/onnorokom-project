import type { Metadata } from "next";
import { RegisterForm } from "@/components/auth/register-form";
import { getRegistrationPolicyAction } from "@/lib/actions/auth.actions";
import { pageTitle } from "@/lib/app";

export const metadata: Metadata = {
  title: pageTitle("Register"),
};

/**
 * The policy is an admin setting that can change at any time, and prerendering would otherwise bake
 * in whatever the API answered at build time (or the closed fallback, if it was unreachable then).
 */
export const dynamic = "force-dynamic";

export default async function RegisterPage() {
  // Read on the server so the form renders with the right roles already in place, rather than
  // flashing a set of options that a client fetch then corrects.
  const policy = await getRegistrationPolicyAction();
  return <RegisterForm policy={policy} />;
}
