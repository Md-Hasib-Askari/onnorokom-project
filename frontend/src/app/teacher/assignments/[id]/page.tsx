import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { TEACHER_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { AssignmentDetailView } from "@/components/teacher/assignment-detail-view";

export const metadata: Metadata = {
  title: pageTitle("Assignment", TEACHER_APP_NAME),
};

export default async function TeacherAssignmentPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  await requireRole(UserRole.Teacher);
  const { id } = await params;
  return <AssignmentDetailView assignmentId={id} />;
}