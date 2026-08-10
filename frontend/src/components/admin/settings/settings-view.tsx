"use client";

import { useState } from "react";
import { toast } from "sonner";

import type { RegistrationPolicy } from "@/lib/api/schemas/settings.schema";
import { ERROR_MESSAGES } from "@/lib/messages";
import { AdminSettingMutations } from "@/lib/mutations/admin-settings.mutations";
import { AdminSettingQueries } from "@/lib/queries/admin-settings.queries";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";

/** Placeholder rows shown while the policy loads: one per toggle. */
const SKELETON_ROW_COUNT = 2;

export function SettingsView() {
  const policyQuery = AdminSettingQueries.useRegistrationPolicy();

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-sm text-muted-foreground">
          System-wide rules that apply to everyone using the platform.
        </p>
      </div>

      {policyQuery.isLoading ? (
        <PolicySkeleton />
      ) : policyQuery.isError || !policyQuery.data ? (
        <p className="text-sm text-destructive">Failed to load settings.</p>
      ) : (
        // Remounting on a fresh policy lets the form seed its draft from server state without an
        // effect, and discards any unsaved edits that a refetch has already overtaken.
        <RegistrationPolicyForm
          key={`${policyQuery.data.teacherSelfRegistrationEnabled}-${policyQuery.data.studentSelfRegistrationEnabled}`}
          policy={policyQuery.data}
        />
      )}
    </div>
  );
}

function RegistrationPolicyForm({ policy }: { policy: RegistrationPolicy }) {
  const updateMutation = AdminSettingMutations.useUpdateRegistrationPolicy();
  const [draft, setDraft] = useState<RegistrationPolicy>(policy);

  const isDirty =
    draft.teacherSelfRegistrationEnabled !== policy.teacherSelfRegistrationEnabled ||
    draft.studentSelfRegistrationEnabled !== policy.studentSelfRegistrationEnabled;

  function handleSave() {
    updateMutation.mutate(draft, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Registration settings saved.");
          return;
        }

        toast.error(result.error ?? ERROR_MESSAGES.generic);
      },
      onError: () => {
        toast.error(ERROR_MESSAGES.genericRetry);
      },
    });
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Public sign-up</CardTitle>
        <CardDescription>
          Choose which roles may create their own account. Accounts created this way still wait for
          your approval, and admins are never allowed to self-register.
        </CardDescription>
      </CardHeader>

      <CardContent className="space-y-4">
        <ToggleRow
          id="teacher-self-registration"
          label="Teachers can register"
          description="Teachers can sign up themselves and appear in your pending queue."
          checked={draft.teacherSelfRegistrationEnabled}
          disabled={updateMutation.isPending}
          onCheckedChange={(checked) =>
            setDraft((current) => ({ ...current, teacherSelfRegistrationEnabled: checked }))
          }
        />
        <ToggleRow
          id="student-self-registration"
          label="Students can register"
          description="Students can sign up themselves. You assign their section when you approve them."
          checked={draft.studentSelfRegistrationEnabled}
          disabled={updateMutation.isPending}
          onCheckedChange={(checked) =>
            setDraft((current) => ({ ...current, studentSelfRegistrationEnabled: checked }))
          }
        />
      </CardContent>

      <CardFooter className="justify-end">
        <Button onClick={handleSave} disabled={!isDirty || updateMutation.isPending}>
          {updateMutation.isPending ? "Saving..." : "Save changes"}
        </Button>
      </CardFooter>
    </Card>
  );
}

function ToggleRow({
  id,
  label,
  description,
  checked,
  disabled,
  onCheckedChange,
}: {
  id: string;
  label: string;
  description: string;
  checked: boolean;
  disabled: boolean;
  onCheckedChange: (checked: boolean) => void;
}) {
  return (
    <div className="flex items-start justify-between gap-4 rounded-lg border p-4">
      <div className="space-y-1">
        <Label htmlFor={id} className="text-sm font-medium">
          {label}
        </Label>
        <p className="text-sm text-muted-foreground">{description}</p>
      </div>
      <Switch
        id={id}
        checked={checked}
        disabled={disabled}
        onCheckedChange={onCheckedChange}
        aria-label={label}
      />
    </div>
  );
}

function PolicySkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: SKELETON_ROW_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-20 w-full" />
      ))}
    </div>
  );
}
