"use client";

import { useMutation } from "@tanstack/react-query";
import { loginAction, logoutAction, registerAction } from "@/lib/actions/auth.actions";
import type { LoginRequest, RegisterRequest } from "@/lib/api/schemas/auth.schema";

/** Grouped under one namespace so every auth mutation is defined in a single place. */
export const AuthMutations = {
  useLogin(nextPath?: string) {
    return useMutation({
      mutationFn: (values: LoginRequest) => loginAction(values, nextPath),
    });
  },

  useRegister() {
    return useMutation({
      mutationFn: (values: RegisterRequest) => registerAction(values),
    });
  },

  useLogout() {
    return useMutation({
      mutationFn: () => logoutAction(),
    });
  },
};
