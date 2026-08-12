"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { changePasswordAction, updateProfileAction } from "@/lib/actions/profile.actions";
import type { ChangePasswordRequest, UpdateProfileRequest } from "@/lib/api/schemas/profile.schema";
import { profileKeys } from "@/lib/queries/profile.queries";

/** Grouped under one namespace so every profile mutation is defined in a single place. */
export const ProfileMutations = {
  useUpdate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: UpdateProfileRequest) => updateProfileAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: profileKeys.all });
        }
      },
    });
  },

  /** Success reissues the session cookie server-side (see `changePasswordAction`); no cache to invalidate. */
  useChangePassword() {
    return useMutation({
      mutationFn: (values: ChangePasswordRequest) => changePasswordAction(values),
    });
  },
};