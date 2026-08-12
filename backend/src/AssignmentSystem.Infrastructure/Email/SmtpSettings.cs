namespace AssignmentSystem.Infrastructure.Email;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = null!;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = "Onnorokom Assignment System";
    public bool EnableSsl { get; set; } = true;
}