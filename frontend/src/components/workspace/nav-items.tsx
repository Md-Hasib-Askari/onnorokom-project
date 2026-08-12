import {
  BookOpenIcon,
  ClipboardListIcon,
  FileCheckIcon,
  GraduationCapIcon,
  LayersIcon,
  LayoutDashboardIcon,
  SettingsIcon,
  UserCircleIcon,
  UsersIcon,
} from "lucide-react";

import type { SidebarNavItem } from "./sidebar-nav";
import { ROUTES } from "@/lib/routes";

const ICON_CLASS = "size-4";

export const ADMIN_NAV_ITEMS: SidebarNavItem[] = [
  { href: ROUTES.adminUsers, label: "Users", icon: <UsersIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.adminGrades, label: "Grades", icon: <GraduationCapIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.adminSections, label: "Sections", icon: <LayersIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.adminSubjects, label: "Subjects", icon: <BookOpenIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.adminAssignments, label: "Assignments", icon: <ClipboardListIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.adminSubmissions, label: "Submissions", icon: <FileCheckIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.adminSettings, label: "Settings", icon: <SettingsIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.profile, label: "Profile", icon: <UserCircleIcon className={ICON_CLASS} />, enabled: true },
];

export const TEACHER_NAV_ITEMS: SidebarNavItem[] = [
  { href: ROUTES.teacher, label: "Overview", icon: <LayoutDashboardIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.teacherSubjects, label: "My subjects", icon: <BookOpenIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.teacherAssignments, label: "Assignments", icon: <ClipboardListIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.profile, label: "Profile", icon: <UserCircleIcon className={ICON_CLASS} />, enabled: true },
];

export const STUDENT_NAV_ITEMS: SidebarNavItem[] = [
  { href: ROUTES.student, label: "Overview", icon: <LayoutDashboardIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.studentAssignments, label: "Assignments", icon: <ClipboardListIcon className={ICON_CLASS} />, enabled: true },
  { href: ROUTES.profile, label: "Profile", icon: <UserCircleIcon className={ICON_CLASS} />, enabled: true },
];