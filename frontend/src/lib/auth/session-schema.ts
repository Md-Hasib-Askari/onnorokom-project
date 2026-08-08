import { z } from "zod";
import { userRoleSchema } from "@/lib/api/schemas/common.schema";

/** Shape of the httpOnly `session` cookie. Written only by our own server after a
 *  successful login/refresh, so parsing failures just mean "no valid session". */
export const sessionUserSchema = z.object({
  userId: z.string(),
  fullName: z.string(),
  email: z.string(),
  role: userRoleSchema,
  accessTokenExpiresAt: z.string(),
});
export type SessionUser = z.infer<typeof sessionUserSchema>;

export function parseSessionCookie(raw: string | undefined): SessionUser | null {
  if (!raw) return null;
  try {
    const result = sessionUserSchema.safeParse(JSON.parse(raw));
    return result.success ? result.data : null;
  } catch {
    return null;
  }
}
