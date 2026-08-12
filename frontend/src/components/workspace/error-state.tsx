import { AlertTriangleIcon, RotateCcwIcon } from "lucide-react";

import { ERROR_MESSAGES } from "@/lib/messages";
import { Button } from "@/components/ui/button";

/** Full-width failure state for a section that failed to load, with a retry action. */
export function ErrorState({
  description,
  retry,
  compact = false,
}: {
  description: string;
  retry: () => void;
  compact?: boolean;
}) {
  return (
    <div
      className={`flex flex-col items-center justify-center gap-3 rounded-lg border border-dashed text-center ${
        compact ? "py-6" : "py-10"
      }`}
    >
      <AlertTriangleIcon className="size-8 text-destructive" aria-hidden />
      <p className="text-sm text-muted-foreground">{description}</p>
      <Button variant="outline" size="sm" onClick={retry}>
        <RotateCcwIcon className="size-4" aria-hidden />
        {ERROR_MESSAGES.genericRetry}
      </Button>
    </div>
  );
}
