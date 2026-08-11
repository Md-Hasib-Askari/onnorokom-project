import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { TEACHER_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { SectionSubjectsView } from "@/components/teacher/section-subjects-view";

export const metadata: Metadata = {
  title: pageTitle("My subjects", TEACHER_APP_NAME),
};

export default async function TeacherSubjectsPage() {
  await requireRole(UserRole.Teacher);
  return <SectionSubjectsView />;
}