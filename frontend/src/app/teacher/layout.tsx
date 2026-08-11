import { PresentationIcon } from "lucide-react";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { TEACHER_APP_NAME } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { TEACHER_NAV_ITEMS } from "@/components/workspace/nav-items";
import { WorkspaceShell } from "@/components/workspace/workspace-shell";

export default async function TeacherLayout({ children }: { children: React.ReactNode }) {
  const session = await requireRole(UserRole.Teacher);

  return (
    <WorkspaceShell
      title={TEACHER_APP_NAME}
      icon={<PresentationIcon className="size-5" />}
      navItems={TEACHER_NAV_ITEMS}
      session={session}
    >
      {children}
    </WorkspaceShell>
  );
}