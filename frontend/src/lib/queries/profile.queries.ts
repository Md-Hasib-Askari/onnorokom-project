"use client";

import { useQuery } from "@tanstack/react-query";
import { getProfileAction } from "@/lib/actions/profile.actions";

export const profileKeys = {
  all: ["profile"] as const,
};

export const ProfileQueries = {
  useGet() {
    return useQuery({
      queryKey: profileKeys.all,
      queryFn: () => getProfileAction(),
    });
  },
};