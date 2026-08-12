import { Loader2Icon } from "lucide-react";

import { SELECT_PLACEHOLDERS } from "@/lib/messages";
import { Button } from "@/components/ui/button";

/** The Load More footer row every paginated workspace list renders under its table. */
export function LoadMoreButton({
  onClick,
  isLoading,
  label,
}: {
  onClick: () => void;
  isLoading: boolean;
  label: string;
}) {
  return (
    <div className="flex justify-center pt-2">
      <Button variant="outline" onClick={onClick} disabled={isLoading}>
        {isLoading ? (
          <>
            <Loader2Icon className="size-4 animate-spin" aria-hidden />
            {SELECT_PLACEHOLDERS.loading}
          </>
        ) : (
          label
        )}
      </Button>
    </div>
  );
}
