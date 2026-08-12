"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  approveUserAction,
  createUserAction,
  deleteUserAction,
  resetUserPasswordAction,
  updateUserAction,
} from "@/lib/actions/admin-users.actions";
import type {
  AdminCreateUserRequest,
  AdminUpdateUserRequest,
  ApproveUserRequest,
} from "@/lib/api/schemas/admin-users.schema";
import type { UserRole } from "@/lib/api/schemas/common.schema";
import { adminUsersKeys } from "@/lib/queries/admin-users.queries";

/** Grouped under one namespace so every admin-user mutation is defined in a single place. */
export const AdminUserMutations = {
  useApprove() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: ApproveUserRequest) => approveUserAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
        }
      },
    });
  },

  useCreate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (values: AdminCreateUserRequest) => createUserAction(values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
        }
      },
    });
  },

  useUpdate() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({
        id,
        role,
        values,
      }: {
        id: string;
        role: UserRole;
        values: AdminUpdateUserRequest;
      }) => updateUserAction(id, role, values),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
        }
      },
    });
  },

  useDelete() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: (id: string) => deleteUserAction(id),
      onSuccess: (result) => {
        if (result.success) {
          queryClient.invalidateQueries({ queryKey: adminUsersKeys.all });
        }
      },
    });
  },

  useResetPassword() {
    return useMutation({
      mutationFn: (id: string) => resetUserPasswordAction(id),
    });
  },
};
