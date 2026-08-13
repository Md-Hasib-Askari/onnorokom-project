import type { Metadata } from "next";
import { pageTitle } from "@/lib/app";
import { requireSession } from "@/lib/auth/session";
import { ProfileView } from "@/components/profile/profile-view";

export const metadata: Metadata = {
  title: pageTitle("Profile"),
};

export default async function ProfilePage() {
  const session = await requireSession();
  return <ProfileView userId={session.userId} />;
}