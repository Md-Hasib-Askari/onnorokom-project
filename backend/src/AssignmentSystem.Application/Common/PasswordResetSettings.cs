namespace AssignmentSystem.Application.Common;

/// <summary>
/// Lifetimes for the password-reset code. Lives in the Application layer because
/// <c>AuthService</c> reads it; the Infrastructure DI binds it from the <c>PasswordReset</c>
/// configuration section.
/// </summary>
public class PasswordResetSettings
{
    public const string SectionName = "PasswordReset";

    public int CodeLifetimeMinutes { get; set; } = 10;
    public int CodeCooldownSeconds { get; set; } = 60;
}
