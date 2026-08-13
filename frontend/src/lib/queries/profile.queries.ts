"use client";

import { useQuery } from "@tanstack/react-query";
import { getProfileAction } from "@/lib/actions/profile.actions";

export const profileKeys = {
  all: (userId: string) => ["profile", userId] as const,
};

export const ProfileQueries = {
  useGet(userId: string) {
    return useQuery({
      queryKey: profileKeys.all(userId),
      queryFn: () => getProfileAction(),
      enabled: !!userId,
    });
  },
};