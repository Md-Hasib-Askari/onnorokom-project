"use client";

import { useEffect } from "react";
import Link from "next/link";
import { GraduationCapIcon, HomeIcon, RotateCcwIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { ROUTES } from "@/lib/routes";

export default function Error({
  error,
  retry,
}: {
  error: Error & { digest?: string };
  retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-6 bg-muted/40 p-6">
      <div className="flex flex-col items-center gap-6 text-center">
        <span className="flex size-12 items-center justify-center rounded-xl bg-destructive text-destructive-foreground">
          <GraduationCapIcon className="size-6" aria-hidden />
        </span>
        <div className="space-y-2">
          <h1 className="text-3xl font-semibold">Something went wrong</h1>
          <p className="max-w-md text-sm text-muted-foreground">
            An unexpected error occurred while loading this page. Try again, or
            go back home.
          </p>
          {error.digest && (
            <p className="text-xs text-muted-foreground/60">
              Error ID: {error.digest}
            </p>
          )}
        </div>
        <div className="flex flex-wrap items-center justify-center gap-2">
          <Button onClick={retry}>
            <RotateCcwIcon aria-hidden />
            Try again
          </Button>
          <Button variant="outline" asChild>
            <Link href={ROUTES.home}>
              <HomeIcon aria-hidden />
              Back to home
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
