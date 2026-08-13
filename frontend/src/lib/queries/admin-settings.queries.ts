"use client";

import { useQuery } from "@tanstack/react-query";
import { getSystemSettingsAction } from "@/lib/actions/admin-settings.actions";

export const adminSettingsKeys = {
  all: ["admin", "settings"] as const,
  systemSettings: () => [...adminSettingsKeys.all, "system-settings"] as const,
};

/** Grouped under one namespace so every admin-settings query is defined in a single place. */
export const AdminSettingQueries = {
  useSystemSettings() {
    return useQuery({
      queryKey: adminSettingsKeys.systemSettings(),
      queryFn: () => getSystemSettingsAction(),
    });
  },
};
