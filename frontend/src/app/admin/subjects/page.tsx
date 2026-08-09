import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { SubjectsView } from "@/components/admin/subjects/subjects-view";

export const metadata: Metadata = {
  title: pageTitle("Subjects", ADMIN_APP_NAME),
};

export default async function AdminSubjectsPage() {
  await requireRole(UserRole.Admin);
  return <SubjectsView />;
}