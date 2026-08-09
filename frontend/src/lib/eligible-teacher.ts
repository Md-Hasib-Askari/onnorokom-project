import { AccountStatus, UserRole } from "@/lib/api/schemas/common.schema";

/** Mirrors the backend's `IsUsableTeacher` check (SubjectService.EnsureIsTeacherAsync). */
export function isEligibleTeacher(user: { role: UserRole; status: AccountStatus; isActive: boolean }) {
  return user.role === UserRole.Teacher && user.status === AccountStatus.Approved && user.isActive;
}
