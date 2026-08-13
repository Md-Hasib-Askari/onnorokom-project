import { logoutAction } from "@/lib/actions/auth.actions";
import type { SessionUser } from "@/lib/auth/session-schema";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ModeToggle } from "@/components/mode-toggle";
import { MobileNav } from "./mobile-nav";
import { SidebarNav, type SidebarNavItem } from "./sidebar-nav";

/** Longest set of initials rendered in the sidebar avatar. */
const MAX_INITIALS = 2;

/** Stands in when a name yields no initials at all. */
const INITIALS_FALLBACK = "?";

function initialsOf(fullName: string): string {
  return fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, MAX_INITIALS)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

/**
 * The sidebar-plus-content frame every workspace shares. Each role's layout supplies its own
 * name, mark, and navigation; everything else (session footer, sign out, theme toggle) is
 * identical across roles by design.
 */
export function WorkspaceShell({
  title,
  icon,
  navItems,
  session,
  children,
}: {
  title: string;
  icon: React.ReactNode;
  navItems: SidebarNavItem[];
  session: SessionUser;
  children: React.ReactNode;
}) {
  const initials = initialsOf(session.fullName);

  return (
    <div className="flex min-h-svh flex-col bg-muted/30 md:flex-row">
      <MobileNav title={title} icon={icon} navItems={navItems} session={session} initials={initials} />
      <aside className="sticky top-0 hidden h-svh w-72 shrink-0 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground md:flex">
        <div className="flex h-18 items-center justify-between gap-2.5 px-6">
          <div className="flex items-center gap-2.5">
            <span className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
              {icon}
            </span>
            <span className="text-sm font-semibold tracking-tight">{title}</span>
          </div>
          <ModeToggle className="border-sidebar-border bg-transparent text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-foreground" />
        </div>

        <SidebarNav items={navItems} />

        <div className="border-t border-sidebar-border p-5">
          <div className="mb-4 flex items-center gap-3">
            <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-sidebar-accent text-xs font-semibold">
              {initials || INITIALS_FALLBACK}
            </span>
            <div className="flex min-w-0 flex-col gap-1">
              <span className="truncate text-sm font-medium">{session.fullName}</span>
              <Badge
                variant="outline"
                className="w-fit border-sidebar-border text-[10px] text-sidebar-foreground/60"
              >
                {session.role}
              </Badge>
            </div>
          </div>
          <form action={logoutAction}>
            <Button
              type="submit"
              variant="outline"
              size="sm"
              className="w-full border-sidebar-border bg-transparent text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-foreground"
            >
              Sign out
            </Button>
          </form>
        </div>
      </aside>
      <main className="min-w-0 flex-1 overflow-x-auto">
        <div className="mx-auto max-w-6xl min-w-0 p-8 md:p-12">{children}</div>
      </main>
    </div>
  );
}