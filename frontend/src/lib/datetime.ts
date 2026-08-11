/** Length of a `YYYY-MM-DDTHH:mm` value, which is what `<input type="datetime-local">` reads. */
const DATETIME_LOCAL_LENGTH = 16;
const MINUTE_IN_MS = 60_000;

/**
 * ISO instant to the wall-clock string a `datetime-local` input expects. The input has no notion
 * of a zone, so the UTC offset is folded into the value before it is trimmed.
 */
export function toDateTimeLocalInput(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  const shifted = new Date(date.getTime() - date.getTimezoneOffset() * MINUTE_IN_MS);
  return shifted.toISOString().slice(0, DATETIME_LOCAL_LENGTH);
}

/** Wall-clock string from a `datetime-local` input to the ISO instant the API stores. */
export function toIsoInstant(dateTimeLocal: string): string {
  return new Date(dateTimeLocal).toISOString();
}

/** One place for how timestamps read across the app. */
export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString();
}

export function isPast(iso: string): boolean {
  return new Date(iso).getTime() < Date.now();
}