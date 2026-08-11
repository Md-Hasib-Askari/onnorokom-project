import type { Metadata } from "next";
import { UserRole } from "@/lib/api/schemas/common.schema";
import { TEACHER_APP_NAME, pageTitle } from "@/lib/app";
import { requireRole } from "@/lib/auth/session";
import { ASSIGNMENT_TARGET_PARAMS } from "@/lib/routes";
import { AssignmentsView } from "@/components/teacher/assignments-view";
import type { AssignmentTarget } from "@/components/teacher/create-assignment-dialog";

export const metadata: Metadata = {
  title: pageTitle("Assignments", TEACHER_APP_NAME),
};

type SearchParams = Record<string, string | string[] | undefined>;

/**
 * "My subjects" links here with both ids set, which opens the create dialog prefilled. A partial
 * or repeated pair is ignored rather than half-applied, so a hand-edited URL just shows the list.
 */
function readTarget(searchParams: SearchParams): AssignmentTarget | undefined {
  const sectionId = searchParams[ASSIGNMENT_TARGET_PARAMS.sectionId];
  const subjectId = searchParams[ASSIGNMENT_TARGET_PARAMS.subjectId];
  if (typeof sectionId !== "string" || typeof subjectId !== "string") return undefined;
  if (!sectionId || !subjectId) return undefined;
  return { sectionId, subjectId };
}

export default async function TeacherAssignmentsPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  await requireRole(UserRole.Teacher);
  const initialTarget = readTarget(await searchParams);
  return <AssignmentsView initialTarget={initialTarget} />;
}