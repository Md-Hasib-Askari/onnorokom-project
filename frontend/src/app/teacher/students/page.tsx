import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { TEACHER_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { StudentsView } from "@/components/teacher/students-view";

export const metadata: Metadata = {
  title: pageTitle("Students", TEACHER_APP_NAME),
};

export default async function TeacherStudentsPage() {
  await requireRole(UserRole.Teacher);
  return <StudentsView />;
}
