import { z } from "zod";
import { userRoleSchema } from "./common.schema";

// ---- GET /api/admin/stats/overview ----

export const adminRecentPendingSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  role: userRoleSchema,
  createdAt: z.string(),
});
export type AdminRecentPending = z.infer<typeof adminRecentPendingSchema>;

export const adminOverviewSchema = z.object({
  students: z.number(),
  teachers: z.number(),
  admins: z.number(),
  pending: z.number(),
  grades: z.number(),
  sections: z.number(),
  subjects: z.number(),
  assignments: z.number(),
  drafts: z.number(),
  published: z.number(),
  submissions: z.number(),
  graded: z.number(),
  ungraded: z.number(),
  recentPending: z.array(adminRecentPendingSchema),
});
export type AdminOverview = z.infer<typeof adminOverviewSchema>;
