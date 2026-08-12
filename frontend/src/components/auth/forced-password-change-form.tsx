"use client";

import { useRouter } from "next/navigation";
import { ShieldAlertIcon } from "lucide-react";
import type { UserRole } from "@/lib/api/schemas/common.schema";
import { roleHome } from "@/lib/auth/constants";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { ChangePasswordForm } from "@/components/profile/change-password-form";

export function ForcedPasswordChangeForm({ role }: { role: UserRole }) {
  const router = useRouter();

  return (
    <div className="space-y-4">
      <Alert>
        <ShieldAlertIcon className="size-4" />
        <AlertTitle>Set a new password</AlertTitle>
        <AlertDescription>
          You must set a new password before you can continue.
        </AlertDescription>
      </Alert>
      <ChangePasswordForm onChanged={() => router.push(roleHome(role))} />
    </div>
  );
}