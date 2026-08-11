import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { STUDENT_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { StudentOverview } from "@/components/student/student-overview";

export const metadata: Metadata = {
  title: pageTitle("Overview", STUDENT_APP_NAME),
};

export default async function StudentOverviewPage() {
  const session = await requireRole(UserRole.Student);
  return <StudentOverview fullName={session.fullName} />;
}