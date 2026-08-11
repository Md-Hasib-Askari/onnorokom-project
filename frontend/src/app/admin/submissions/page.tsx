import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { SubmissionsView } from "@/components/admin/submissions/submissions-view";

export const metadata: Metadata = {
  title: pageTitle("Submissions", ADMIN_APP_NAME),
};

export default async function AdminSubmissionsPage() {
  await requireRole(UserRole.Admin);
  return <SubmissionsView />;
}