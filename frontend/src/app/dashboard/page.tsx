import type { Metadata } from "next";
import { GraduationCapIcon } from "lucide-react";
import { pageTitle } from "@/lib/app";
import { requireSession } from "@/lib/auth/session";
import { logoutAction } from "@/lib/actions/auth.actions";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ModeToggle } from "@/components/mode-toggle";

export const metadata: Metadata = {
  title: pageTitle("Dashboard"),
};

export default async function DashboardPage() {
  const session = await requireSession();

  return (
    <div className="relative flex min-h-svh flex-col items-center justify-center gap-6 bg-muted/40 p-6">
      <ModeToggle className="absolute top-6 right-6" />
      <Card className="w-full max-w-sm shadow-sm">
        <CardHeader className="flex flex-col items-center gap-3 text-center">
          <span className="flex size-11 items-center justify-center rounded-xl bg-primary text-primary-foreground">
            <GraduationCapIcon className="size-5.5" />
          </span>
          <div className="space-y-1.5">
            <CardTitle className="text-xl">Welcome, {session.fullName}</CardTitle>
            <CardDescription>
              The {session.role.toLowerCase()} workspace is coming soon.
            </CardDescription>
          </div>
          <Badge variant="secondary">{session.role}</Badge>
        </CardHeader>
        <CardContent>
          <form action={logoutAction}>
            <Button type="submit" variant="outline" className="w-full">
              Sign out
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
