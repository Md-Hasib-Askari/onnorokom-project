"use client";

import { useEffect } from "react";
import { GraduationCapIcon, RotateCcwIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import "./globals.css";

export default function GlobalError({
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
    <html lang="en" suppressHydrationWarning>
      <body className="bg-muted/40 font-sans antialiased">
        <div className="flex min-h-svh flex-col items-center justify-center gap-6 p-6">
          <div className="flex flex-col items-center gap-6 text-center">
            <span className="flex size-12 items-center justify-center rounded-xl bg-destructive text-destructive-foreground">
              <GraduationCapIcon className="size-6" aria-hidden />
            </span>
            <div className="space-y-2">
              <h1 className="text-3xl font-semibold text-foreground">
                Something went wrong
              </h1>
              <p className="max-w-md text-sm text-muted-foreground">
                An unexpected error occurred while loading this page. Try
                again.
              </p>
            </div>
            <Button onClick={retry}>
              <RotateCcwIcon aria-hidden />
              Try again
            </Button>
          </div>
        </div>
      </body>
    </html>
  );
}
