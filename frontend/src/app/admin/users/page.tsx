import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { UsersView } from "@/components/admin/users/users-view";

export const metadata: Metadata = {
  title: pageTitle("Users", ADMIN_APP_NAME),
};

export default async function AdminUsersPage() {
  const session = await requireRole(UserRole.Admin);
  return <UsersView currentUserId={session.userId} />;
}
