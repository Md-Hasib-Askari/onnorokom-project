import type { TeacherSectionSubject } from "@/lib/api/schemas/teacher.schema";
import { EMPTY_CELL } from "@/lib/messages";

const CLASS_SEPARATOR = " / ";

/** "Grade 6 / A", the way a teacher names a class. */
export function classLabel(pair: { gradeName: string | null; sectionName: string | null }): string {
  const parts = [pair.gradeName, pair.sectionName].filter(Boolean);
  return parts.length > 0 ? parts.join(CLASS_SEPARATOR) : EMPTY_CELL;
}

export interface PickerOption {
  id: string;
  label: string;
}

/**
 * The workspace list carries one row per section-subject pair, so a teacher who teaches two
 * subjects in the same section appears twice. Both pickers collapse that back down.
 */
export function sectionOptions(pairs: TeacherSectionSubject[]): PickerOption[] {
  const byId = new Map<string, PickerOption>();
  for (const pair of pairs) {
    if (!byId.has(pair.sectionId)) {
      byId.set(pair.sectionId, { id: pair.sectionId, label: classLabel(pair) });
    }
  }
  return [...byId.values()];
}

export function subjectOptions(pairs: TeacherSectionSubject[], sectionId: string): PickerOption[] {
  const byId = new Map<string, PickerOption>();
  for (const pair of pairs) {
    if (pair.sectionId !== sectionId || byId.has(pair.subjectId)) continue;
    byId.set(pair.subjectId, {
      id: pair.subjectId,
      label: pair.subjectName ?? pair.subjectCode ?? EMPTY_CELL,
    });
  }
  return [...byId.values()];
}