import { BackpackIcon } from "lucide-react";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { STUDENT_APP_NAME } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { STUDENT_NAV_ITEMS } from "@/components/workspace/nav-items";
import { WorkspaceShell } from "@/components/workspace/workspace-shell";

export default async function StudentLayout({ children }: { children: React.ReactNode }) {
  const session = await requireRole(UserRole.Student);

  return (
    <WorkspaceShell
      title={STUDENT_APP_NAME}
      icon={<BackpackIcon className="size-5" />}
      navItems={STUDENT_NAV_ITEMS}
      session={session}
    >
      {children}
    </WorkspaceShell>
  );
}