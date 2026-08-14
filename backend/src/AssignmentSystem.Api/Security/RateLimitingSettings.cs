namespace AssignmentSystem.Api.Security;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";
    public const string AuthPolicyName = "auth";

    public int AuthPermitLimit { get; set; } = 10;
    public int AuthWindowSeconds { get; set; } = 60;
}
