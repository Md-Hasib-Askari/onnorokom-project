import { BackpackIcon, PresentationIcon, ShieldCheckIcon } from "lucide-react";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME, STUDENT_APP_NAME, TEACHER_APP_NAME } from "@/lib/app";
import { requireSession } from "@/lib/auth/session";
import {
  ADMIN_NAV_ITEMS,
  STUDENT_NAV_ITEMS,
  TEACHER_NAV_ITEMS,
} from "@/components/workspace/nav-items";
import { WorkspaceShell } from "@/components/workspace/workspace-shell";

/** `/profile` is shared by every role, so the shell it renders is picked from the signed-in user's own role. */
const WORKSPACE_BY_ROLE = {
  [UserRole.Admin]: {
    title: ADMIN_APP_NAME,
    icon: <ShieldCheckIcon className="size-5" />,
    navItems: ADMIN_NAV_ITEMS,
  },
  [UserRole.Teacher]: {
    title: TEACHER_APP_NAME,
    icon: <PresentationIcon className="size-5" />,
    navItems: TEACHER_NAV_ITEMS,
  },
  [UserRole.Student]: {
    title: STUDENT_APP_NAME,
    icon: <BackpackIcon className="size-5" />,
    navItems: STUDENT_NAV_ITEMS,
  },
} as const;

export default async function ProfileLayout({ children }: { children: React.ReactNode }) {
  const session = await requireSession();
  const workspace = WORKSPACE_BY_ROLE[session.role];

  return (
    <WorkspaceShell title={workspace.title} icon={workspace.icon} navItems={workspace.navItems} session={session}>
      {children}
    </WorkspaceShell>
  );
}