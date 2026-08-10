"use client";

import { useEffect } from "react";

import { SELECT_PLACEHOLDERS } from "@/lib/messages";
import { AdminSectionQueries } from "@/lib/queries/admin-sections.queries";
import { FormControl } from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface SectionSelectProps {
  gradeId: string | undefined;
  value: string | undefined;
  onValueChange: (value: string) => void;
}

/** Radix shows the placeholder only for an empty value, so no other value may reach the trigger. */
const NO_SECTION = "";

/**
 * Cascading section picker shared by the student create, edit, and approve dialogs.
 * A grade with no sections leaves nothing to pick, so the trigger stays disabled and says why
 * instead of opening an empty list.
 */
export function SectionSelect({ gradeId, value, onValueChange }: SectionSelectProps) {
  const sections = AdminSectionQueries.useByGrade(gradeId);
  const options = sections.data;
  const hasNoSections = !!gradeId && !sections.isLoading && !options?.length;

  // A section picked under a previous grade is not on offer here. Radix would render that value as
  // a blank trigger, hiding the placeholder, so drop it from the display.
  const isSelectable = !!value && !!options?.some((section) => section.id === value);
  const isStale = !!gradeId && !!value && !!options && !isSelectable;

  // Clear it upstream too, so an unpickable section can never be submitted.
  useEffect(() => {
    if (isStale) onValueChange(NO_SECTION);
  }, [isStale, onValueChange]);

  const placeholder = !gradeId
    ? SELECT_PLACEHOLDERS.sectionNeedsGrade
    : sections.isLoading
      ? SELECT_PLACEHOLDERS.loading
      : hasNoSections
        ? SELECT_PLACEHOLDERS.sectionNone
        : SELECT_PLACEHOLDERS.section;

  return (
    <Select
      value={isSelectable ? value : NO_SECTION}
      onValueChange={onValueChange}
      disabled={!gradeId || hasNoSections}
    >
      <FormControl>
        <SelectTrigger className="w-full">
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
      </FormControl>
      <SelectContent>
        {options?.map((section) => (
          <SelectItem key={section.id} value={section.id}>
            {section.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
