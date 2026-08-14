import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { AdminOverview } from "@/components/admin/admin-overview";

export const metadata: Metadata = {
  title: pageTitle("Overview", ADMIN_APP_NAME),
};

export default async function AdminOverviewPage() {
  const session = await requireRole(UserRole.Admin);
  return <AdminOverview fullName={session.fullName} />;
}
