"use client";

import { useState } from "react";
import { MenuIcon } from "lucide-react";

import type { SessionUser } from "@/lib/auth/session-schema";
import { logoutAction } from "@/lib/actions/auth.actions";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ModeToggle } from "@/components/mode-toggle";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { SidebarNav, type SidebarNavItem } from "./sidebar-nav";

interface MobileNavProps {
  title: string;
  icon: React.ReactNode;
  navItems: SidebarNavItem[];
  session: SessionUser;
  /** Initials of the signed-in user, computed by the shell so both navs render the same. */
  initials: string;
}

/**
 * The `md`-and-up sidebar has no mobile counterpart: on phones the aside is hidden and
 * nothing replaced it, leaving signed-in users unable to reach anything but the current
 * page. This bar shows below `md` and opens a slide-in drawer with the same navigation
 * and session footer as the desktop sidebar.
 */
export function MobileNav({ title, icon, navItems, session, initials }: MobileNavProps) {
  const [open, setOpen] = useState(false);

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <header className="sticky top-0 z-40 flex h-16 shrink-0 items-center justify-between gap-2.5 border-b border-sidebar-border bg-sidebar px-4 text-sidebar-foreground md:hidden">
        <div className="flex items-center gap-2.5">
          <SheetTrigger asChild>
            <Button
              variant="ghost"
              size="icon-sm"
              className="border-sidebar-border bg-transparent text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-foreground"
              aria-label="Open navigation menu"
            >
              <MenuIcon className="size-5" />
            </Button>
          </SheetTrigger>
          <span className="flex items-center gap-2.5">
            <span className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
              {icon}
            </span>
            <span className="text-sm font-semibold tracking-tight">{title}</span>
          </span>
        </div>
        <ModeToggle className="border-sidebar-border bg-transparent text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-foreground" />
      </header>

      <SheetContent side="left" className="flex w-72 flex-col bg-sidebar text-sidebar-foreground">
        <SheetHeader className="h-18 items-center justify-between gap-2.5 px-6">
          <div className="flex items-center gap-2.5">
            <span className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
              {icon}
            </span>
            <SheetTitle className="text-sidebar-foreground">{title}</SheetTitle>
          </div>
          <SheetDescription className="sr-only">
            Workspace navigation for {title}
          </SheetDescription>
        </SheetHeader>

        <SidebarNav items={navItems} onNavigate={() => setOpen(false)} />

        <div className="border-t border-sidebar-border p-5">
          <div className="mb-4 flex items-center gap-3">
            <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-sidebar-accent text-xs font-semibold">
              {initials || "?"}
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
      </SheetContent>
    </Sheet>
  );
}
