import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { TEACHER_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { TeacherOverview } from "@/components/teacher/teacher-overview";

export const metadata: Metadata = {
  title: pageTitle("Overview", TEACHER_APP_NAME),
};

export default async function TeacherOverviewPage() {
  const session = await requireRole(UserRole.Teacher);
  return <TeacherOverview fullName={session.fullName} />;
}