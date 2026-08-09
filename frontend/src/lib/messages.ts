/** User-facing copy that would otherwise be duplicated across actions and components. */
export const ERROR_MESSAGES = {
  generic: "Something went wrong.",
  genericRetry: "Something went wrong. Please try again.",
  validation: "Check the highlighted fields.",
  sessionExpired: "Your session has expired. Please log in again.",
} as const;

/** Form validation copy. Kept next to the error copy so wording stays consistent. */
export const VALIDATION_MESSAGES = {
  fullNameRequired: "Full name is required.",
  emailRequired: "Email is required.",
  emailInvalid: "Enter a valid email address.",
  passwordRequired: "Password is required.",
  confirmPasswordRequired: "Confirm your password.",
  passwordsDoNotMatch: "Passwords do not match.",
  gradeRequired: "Grade is required for students.",
  subjectGradeRequired: "Grade is required.",
  gradeNameRequired: "Grade name is required.",
  academicYearRequired: "Academic year is required.",
  subjectNameRequired: "Subject name is required.",
  passwordTooShort: (minLength: number) => `Must be at least ${minLength} characters.`,
  passwordNeedsUppercase: "Must contain an uppercase letter.",
  passwordNeedsLowercase: "Must contain a lowercase letter.",
  passwordNeedsDigit: "Must contain a digit.",
  passwordNeedsSpecial: "Must contain a special character.",
} as const;
