"use client";

import { ProfileQueries } from "@/lib/queries/profile.queries";
import { Skeleton } from "@/components/ui/skeleton";
import { ChangePasswordForm } from "./change-password-form";
import { EditProfileForm } from "./edit-profile-form";

export function ProfileView() {
  const profileQuery = ProfileQueries.useGet();

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">Profile</h1>
        <p className="text-sm text-muted-foreground">Manage your account details and password.</p>
      </div>

      {profileQuery.isLoading ? (
        <ProfileSkeleton />
      ) : profileQuery.isError || !profileQuery.data ? (
        <p className="text-sm text-destructive">Failed to load your profile.</p>
      ) : (
        <div className="space-y-6">
          <EditProfileForm key={profileQuery.data.fullName} profile={profileQuery.data} />
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