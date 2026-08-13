namespace AssignmentSystem.Domain.Enums;

/// <summary>
/// Identifies a single admin-controlled, application-wide policy value. Every key listed here
/// must have a seeded default in the database initializer, otherwise the setting reads as missing
/// on a fresh install and callers fall back to the most restrictive behaviour.
/// </summary>
public enum SystemSettingKey
{
    /// <summary>Whether the public registration endpoint accepts new teacher accounts.</summary>
    TeacherSelfRegistrationEnabled,

    /// <summary>Whether the public registration endpoint accepts new student accounts.</summary>
    StudentSelfRegistrationEnabled,

    /// <summary>Whether teachers can edit their own role-specific profile fields.</summary>
    TeacherProfileSelfEditEnabled,

    /// <summary>Whether students can edit their own role-specific profile fields.</summary>
    StudentProfileSelfEditEnabled
}
