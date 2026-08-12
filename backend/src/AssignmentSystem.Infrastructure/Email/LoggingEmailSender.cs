using AssignmentSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Infrastructure.Email;

/// <summary>Writes emails to the log instead of sending them. Used in Development, where no real SMTP credentials are configured.</summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Email suppressed (Development): To={ToEmail} Subject={Subject}\n{HtmlBody}",
            toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}