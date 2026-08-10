import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { SettingsView } from "@/components/admin/settings/settings-view";

export const metadata: Metadata = {
  title: pageTitle("Settings", ADMIN_APP_NAME),
};

export default async function AdminSettingsPage() {
  await requireRole(UserRole.Admin);
  return <SettingsView />;
}