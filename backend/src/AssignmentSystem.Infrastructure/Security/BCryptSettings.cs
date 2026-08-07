namespace AssignmentSystem.Infrastructure.Security;

public class BCryptSettings
{
    public const string SectionName = "BCrypt";

    public int WorkFactor { get; set; } = 12;
}
