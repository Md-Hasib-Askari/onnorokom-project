"use client";

import { useState } from "react";
import { toast } from "sonner";

import type { SystemSettings } from "@/lib/api/schemas/settings.schema";
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
import { ErrorState } from "@/components/workspace/error-state";

/** Placeholder rows shown while the settings load: one per toggle. */
const SKELETON_ROW_COUNT = 4;

export function SettingsView() {
  const settingsQuery = AdminSettingQueries.useSystemSettings();

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-sm text-muted-foreground">
          System-wide rules that apply to everyone using the platform.
        </p>
      </div>

      {settingsQuery.isLoading ? (
        <SettingsSkeleton />
      ) : settingsQuery.isError || !settingsQuery.data ? (
        <ErrorState description="Failed to load settings." retry={settingsQuery.refetch} />
      ) : (
        // Remounting on a fresh policy lets the form seed its draft from server state without an
        // effect, and discards any unsaved edits that a refetch has already overtaken.
        <SystemSettingsForm
          key={[
            settingsQuery.data.teacherSelfRegistrationEnabled,
            settingsQuery.data.studentSelfRegistrationEnabled,
            settingsQuery.data.teacherProfileSelfEditEnabled,
            settingsQuery.data.studentProfileSelfEditEnabled,
          ].join("-")}
          settings={settingsQuery.data}
        />
      )}
    </div>
  );
}

function SystemSettingsForm({ settings }: { settings: SystemSettings }) {
  const updateMutation = AdminSettingMutations.useUpdateSystemSettings();
  const [draft, setDraft] = useState<SystemSettings>(settings);

  const isDirty =
    draft.teacherSelfRegistrationEnabled !== settings.teacherSelfRegistrationEnabled ||
    draft.studentSelfRegistrationEnabled !== settings.studentSelfRegistrationEnabled ||
    draft.teacherProfileSelfEditEnabled !== settings.teacherProfileSelfEditEnabled ||
    draft.studentProfileSelfEditEnabled !== settings.studentProfileSelfEditEnabled;

  function handleSave() {
    updateMutation.mutate(draft, {
      onSuccess: (result) => {
        if (result.success) {
          toast.success("Settings saved.");
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
        <CardTitle>System settings</CardTitle>
        <CardDescription>
          These rules apply to everyone on the platform. Changes take effect immediately.
        </CardDescription>
      </CardHeader>

      <CardContent className="space-y-6">
        <div className="space-y-4">
          <div className="space-y-1">
            <h2 className="text-sm font-semibold">Public sign-up</h2>
            <p className="text-sm text-muted-foreground">
              Choose which roles may create their own account. Accounts created this way still wait
              for your approval, and admins are never allowed to self-register.
            </p>
          </div>
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
        </div>

        <div className="space-y-4">
          <div className="space-y-1">
            <h2 className="text-sm font-semibold">Profile self-editing</h2>
            <p className="text-sm text-muted-foreground">
              Choose which roles may edit their own role-specific profile fields. Full name always
              stays editable regardless of these toggles.
            </p>
          </div>
          <ToggleRow
            id="teacher-profile-self-edit"
            label="Teachers can edit their profile"
            description="Teachers can update their department, designation, and other profile fields."
            checked={draft.teacherProfileSelfEditEnabled}
            disabled={updateMutation.isPending}
            onCheckedChange={(checked) =>
              setDraft((current) => ({ ...current, teacherProfileSelfEditEnabled: checked }))
            }
          />
          <ToggleRow
            id="student-profile-self-edit"
            label="Students can edit their profile"
            description="Students can update their guardian info, address, and other profile fields."
            checked={draft.studentProfileSelfEditEnabled}
            disabled={updateMutation.isPending}
            onCheckedChange={(checked) =>
              setDraft((current) => ({ ...current, studentProfileSelfEditEnabled: checked }))
            }
          />
        </div>
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

function SettingsSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: SKELETON_ROW_COUNT }).map((_, index) => (
        <Skeleton key={index} className="h-20 w-full" />
      ))}
    </div>
  );
}
