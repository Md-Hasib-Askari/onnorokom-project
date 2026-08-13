import Link from "next/link";
import { GraduationCapIcon, HomeIcon, LogInIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { ROUTES } from "@/lib/routes";

export default function NotFound() {
  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-6 bg-muted/40 p-6">
      <div className="flex flex-col items-center gap-6 text-center">
        <span className="flex size-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
          <GraduationCapIcon className="size-6" aria-hidden />
        </span>
        <div className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">404</p>
          <h1 className="text-3xl font-semibold">Page not found</h1>
          <p className="max-w-md text-sm text-muted-foreground">
            The page you are looking for does not exist or may have been moved.
          </p>
        </div>
        <div className="flex flex-wrap items-center justify-center gap-2">
          <Button asChild>
            <Link href={ROUTES.home}>
              <HomeIcon aria-hidden />
              Back to home
            </Link>
          </Button>
          <Button variant="outline" asChild>
            <Link href={ROUTES.login}>
              <LogInIcon aria-hidden />
              Sign in
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
