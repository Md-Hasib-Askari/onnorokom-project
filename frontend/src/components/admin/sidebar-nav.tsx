"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  UsersIcon,
  GraduationCapIcon,
  BookOpenIcon,
  ClipboardListIcon,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { ROUTES } from "@/lib/routes";
import { cn } from "@/lib/utils";

type SidebarNavItem = {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  enabled: boolean;
};

const navItems: SidebarNavItem[] = [
  { href: ROUTES.adminUsers, label: "Users", icon: UsersIcon, enabled: true },
  { href: ROUTES.adminGrades, label: "Grades", icon: GraduationCapIcon, enabled: false },
  { href: ROUTES.adminSubjects, label: "Subjects", icon: BookOpenIcon, enabled: false },
  { href: ROUTES.adminAssignments, label: "Assignments", icon: ClipboardListIcon, enabled: false },
];

export function SidebarNav() {
  const pathname = usePathname();

  return (
    <nav className="flex flex-1 flex-col gap-1.5 overflow-y-auto p-4">
      {navItems.map((item) => {
        if (!item.enabled) {
          return (
            <span
              key={item.href}
              className="flex items-center gap-3 rounded-lg px-3.5 py-2.5 text-sm text-sidebar-foreground/40"
            >
              <item.icon className="size-4" />
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

        const active = pathname === item.href || pathname.startsWith(`${item.href}/`);

        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "flex items-center gap-3 rounded-lg px-3.5 py-2.5 text-sm font-medium text-sidebar-foreground/70 transition-colors hover:bg-sidebar-accent hover:text-sidebar-foreground",
              active && "bg-sidebar-accent text-sidebar-foreground"
            )}
          >
            <item.icon className="size-4" />
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
