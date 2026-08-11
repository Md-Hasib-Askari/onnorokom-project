import { ShieldCheckIcon } from "lucide-react";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { ADMIN_APP_NAME } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { ADMIN_NAV_ITEMS } from "@/components/workspace/nav-items";
import { WorkspaceShell } from "@/components/workspace/workspace-shell";

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await requireRole(UserRole.Admin);

  return (
    <WorkspaceShell
      title={ADMIN_APP_NAME}
      icon={<ShieldCheckIcon className="size-5" />}
      navItems={ADMIN_NAV_ITEMS}
      session={session}
    >
      {children}
    </WorkspaceShell>
  );
}