"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateSystemSettingsAction } from "@/lib/actions/admin-settings.actions";
import type { SystemSettingsUpdateRequest } from "@/lib/api/schemas/settings.schema";
import { adminSettingsKeys } from "@/lib/queries/admin-settings.queries";

/** Grouped under one namespace so every admin-settings mutation is defined in a single place. */
export const AdminSettingMutations = {
  useUpdateSystemSettings() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: SystemSettingsUpdateRequest) => updateSystemSettingsAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminSettingsKeys.all });
        }
      },
    });
  },
};
