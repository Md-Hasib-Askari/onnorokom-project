"use client";

import { useQuery } from "@tanstack/react-query";
import { getRegistrationPolicyAction } from "@/lib/actions/admin-settings.actions";

export const adminSettingsKeys = {
  all: ["admin", "settings"] as const,
  registrationPolicy: () => [...adminSettingsKeys.all, "registration-policy"] as const,
};

/** Grouped under one namespace so every admin-settings query is defined in a single place. */
export const AdminSettingQueries = {
  useRegistrationPolicy() {
    return useQuery({
      queryKey: adminSettingsKeys.registrationPolicy(),
      queryFn: () => getRegistrationPolicyAction(),
    });
  },
};