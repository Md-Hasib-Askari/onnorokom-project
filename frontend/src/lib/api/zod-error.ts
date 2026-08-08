import { z } from "zod";

/** First error message per field, for showing under form inputs. */
export function fieldErrorsFrom<T>(error: z.ZodError<T>): Record<string, string> {
  const flat = z.flattenError(error) as { fieldErrors: Record<string, string[] | undefined> };
  const result: Record<string, string> = {};
  for (const field of Object.keys(flat.fieldErrors)) {
    const messages = flat.fieldErrors[field];
    if (messages && messages.length > 0) result[field] = messages[0]!;
  }
  return result;
}
