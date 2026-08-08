export const APP_NAME = "Onnorokom Assignment System";
export const ADMIN_APP_NAME = "Onnorokom Admin";
export const APP_DESCRIPTION = "Assignment & submission management for schools and colleges";

/** `pageTitle("Sign in")` -> "Sign in | Onnorokom Assignment System". */
export function pageTitle(page: string, suffix: string = APP_NAME): string {
  return `${page} | ${suffix}`;
}

/** `next-themes` values. Kept here so no component spells them out. */
export const THEME = {
  light: "light",
  dark: "dark",
  system: "system",
} as const;

export type Theme = (typeof THEME)[keyof typeof THEME];

/** Attribute `next-themes` writes the resolved theme onto. */
export const THEME_ATTRIBUTE = "class";
