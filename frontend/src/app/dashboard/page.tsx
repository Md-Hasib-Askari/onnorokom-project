import { redirect } from "next/navigation";
import { roleHome } from "@/lib/auth/constants";
import { requireSession } from "@/lib/auth/session";

/**
 * Kept alive for bookmarks and for anything that still links to a role-agnostic landing
 * page. Every workspace now lives under its own prefix, so this only forwards.
 */
export default async function DashboardPage() {
  const session = await requireSession();
  redirect(roleHome(session.role));
}