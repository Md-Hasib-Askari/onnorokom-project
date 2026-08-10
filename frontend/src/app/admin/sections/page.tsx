import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { SectionsView } from "@/components/admin/sections/sections-view";

export const metadata: Metadata = {
  title: pageTitle("Sections", ADMIN_APP_NAME),
};

export default async function AdminSectionsPage() {
  await requireRole(UserRole.Admin);
  return <SectionsView />;
}