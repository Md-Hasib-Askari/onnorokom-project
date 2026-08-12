/** User-facing copy that would otherwise be duplicated across actions and components. */
export const ERROR_MESSAGES = {
  generic: "Something went wrong.",
  genericRetry: "Something went wrong. Please try again.",
  validation: "Check the highlighted fields.",
  sessionExpired: "Your session has expired. Please log in again.",
} as const;

/** Stands in for a table cell whose value is absent, so every table renders the gap alike. */
export const EMPTY_CELL = "-";

/** Select trigger copy, so every cascading picker reads the same in each of its states. */
export const SELECT_PLACEHOLDERS = {
  loading: "Loading...",
  grade: "Select a grade",
  section: "Select a section",
  sectionNeedsGrade: "Select a grade first",
  sectionNone: "This grade has no sections yet",
  sectionUnassigned: "No sections assigned to you yet",
  subject: "Select a subject",
  subjectNeedsSection: "Select a section first",
  subjectNone: "This section has no subjects yet",
} as const;

/** Form validation copy. Kept next to the error copy so wording stays consistent. */
export const VALIDATION_MESSAGES = {
  fullNameRequired: "Full name is required.",
  emailRequired: "Email is required.",
  emailInvalid: "Enter a valid email address.",
  passwordRequired: "Password is required.",
  currentPasswordRequired: "Enter your current password.",
  newPasswordSameAsCurrent: "New password must be different from your current password.",
  confirmPasswordRequired: "Confirm your password.",
  passwordsDoNotMatch: "Passwords do not match.",
  resetCodeRequired: "Enter the code from your email.",
  resetCodeInvalid: "Enter the 6-digit code from your email.",
  gradeRequired: "Grade is required for students.",
  sectionRequired: "Section is required for students.",
  subjectGradeRequired: "Grade is required.",
  sectionGradeRequired: "Grade is required.",
  gradeNameRequired: "Grade name is required.",
  academicYearRequired: "Academic year is required.",
  subjectNameRequired: "Subject name is required.",
  sectionNameRequired: "Section name is required.",
  subjectRequired: "Subject is required.",
  assignmentTitleRequired: "Assignment title is required.",
  deadlineRequired: "Deadline is required.",
  deadlineInvalid: "Enter a valid date and time.",
  deadlineMustBeFuture: "Deadline must be in the future.",
  maxMarksPositive: "Maximum marks must be greater than zero.",
  marksNotNegative: "Marks cannot be negative.",
  marksAboveMax: (maxMarks: number) => `Marks cannot exceed ${maxMarks}.`,
  submissionWorkRequired: "Enter your work or attach a link.",
  attachmentUrlInvalid: "Attachment link must be a valid http or https URL.",
  passwordTooShort: (minLength: number) => `Must be at least ${minLength} characters.`,
  passwordNeedsUppercase: "Must contain an uppercase letter.",
  passwordNeedsLowercase: "Must contain a lowercase letter.",
  passwordNeedsDigit: "Must contain a digit.",
  passwordNeedsSpecial: "Must contain a special character.",
} as const;
