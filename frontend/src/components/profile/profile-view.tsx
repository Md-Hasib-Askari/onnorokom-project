"use client";

import { ProfileQueries } from "@/lib/queries/profile.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/workspace/error-state";
import { ChangePasswordForm } from "./change-password-form";
import { EditProfileForm } from "./edit-profile-form";

export function ProfileView({ userId }: { userId: string }) {
  const profileQuery = ProfileQueries.useGet(userId);

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Profile</h1>
        <p className="text-sm text-muted-foreground">Manage your account details and password.</p>
      </div>

      {profileQuery.isLoading ? (
        <ProfileSkeleton />
      ) : profileQuery.isError || !profileQuery.data ? (
        <ErrorState description="Failed to load your profile." retry={profileQuery.refetch} />
      ) : (
        <div className="space-y-6">
          <EditProfileForm key={profileQuery.data.id} profile={profileQuery.data} />
          <ChangePasswordForm />
        </div>
      )}
    </div>
  );
}

function ProfileSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-56 w-full" />
      <Skeleton className="h-72 w-full" />
    </div>
  );
}