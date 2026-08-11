import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { STUDENT_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { StudentAssignmentDetailView } from "@/components/student/assignment-detail-view";

export const metadata: Metadata = {
  title: pageTitle("Assignment", STUDENT_APP_NAME),
};

export default async function StudentAssignmentPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  await requireRole(UserRole.Student);
  const { id } = await params;
  return <StudentAssignmentDetailView assignmentId={id} />;
}