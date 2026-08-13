"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

/**
 * `icon` is a rendered element rather than a component type: the item lists are built in
 * Server Components, and a component reference cannot cross the server/client boundary
 * while an already-rendered element can.
 */
export type SidebarNavItem = {
  href: string;
  label: string;
  icon: React.ReactNode;
  enabled: boolean;
};

function matchesPath(pathname: string, href: string): boolean {
  return pathname === href || pathname.startsWith(`${href}/`);
}

export function SidebarNav({
  items,
  onNavigate,
}: {
  items: SidebarNavItem[];
  /** Called when a link is activated; the mobile drawer uses it to close itself. */
  onNavigate?: () => void;
}) {
  const pathname = usePathname();

  /**
   * Longest match wins, so an overview item at `/teacher` does not stay lit while the user
   * is inside `/teacher/assignments`.
   */
  const activeHref = items
    .filter((item) => item.enabled && matchesPath(pathname, item.href))
    .sort((a, b) => b.href.length - a.href.length)[0]?.href;

  return (
    <nav className="flex flex-1 flex-col gap-1.5 overflow-y-auto p-4">
      {items.map((item) => {
        if (!item.enabled) {
          return (
            <span
              key={item.href}
              className="flex items-center gap-3 rounded-lg px-3.5 py-2.5 text-sm text-sidebar-foreground/40"
            >
              {item.icon}
              {item.label}
              <Badge
                variant="outline"
                className="ml-auto border-sidebar-border text-[10px] text-sidebar-foreground/50"
              >
                Soon
              </Badge>
            </span>
          );
        }

        const active = item.href === activeHref;

        return (
          <Link
            key={item.href}
            href={item.href}
            onClick={onNavigate}
            className={cn(
              "flex items-center gap-3 rounded-lg px-3.5 py-2.5 text-sm font-medium text-sidebar-foreground/70 transition-colors hover:bg-sidebar-accent hover:text-sidebar-foreground",
              active && "bg-sidebar-accent text-sidebar-foreground"
            )}
          >
            {item.icon}
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}