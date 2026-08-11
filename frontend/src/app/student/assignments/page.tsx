import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { STUDENT_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { StudentAssignmentsView } from "@/components/student/assignments-view";

export const metadata: Metadata = {
  title: pageTitle("Assignments", STUDENT_APP_NAME),
};

export default async function StudentAssignmentsPage() {
  await requireRole(UserRole.Student);
  return <StudentAssignmentsView />;
}