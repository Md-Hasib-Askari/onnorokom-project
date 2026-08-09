import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { AssignmentsView } from "@/components/admin/assignments/assignments-view";

export const metadata: Metadata = {
  title: pageTitle("Assignments", ADMIN_APP_NAME),
};

export default async function AdminAssignmentsPage() {
  await requireRole(UserRole.Admin);
  return <AssignmentsView />;
}