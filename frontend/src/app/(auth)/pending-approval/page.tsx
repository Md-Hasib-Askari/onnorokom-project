import type { Metadata } from "next";
import Link from "next/link";
import { ClockIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { pageTitle } from "@/lib/app";
import { ROUTES } from "@/lib/routes";

export const metadata: Metadata = {
  title: pageTitle("Registration submitted"),
};

export default function PendingApprovalPage() {
  return (
    <Card>
      <CardHeader className="items-center text-center">
        <div className="mb-2 flex size-12 items-center justify-center rounded-full bg-muted">
          <ClockIcon className="size-6 text-muted-foreground" />
        </div>
        <CardTitle>Registration submitted</CardTitle>
        <CardDescription>
          Your account has been created and is waiting for an administrator to approve it.
          You&apos;ll be able to sign in once it&apos;s approved.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Button asChild className="w-full">
          <Link href={ROUTES.login}>Back to sign in</Link>
        </Button>
      </CardContent>
    </Card>
  );
}
