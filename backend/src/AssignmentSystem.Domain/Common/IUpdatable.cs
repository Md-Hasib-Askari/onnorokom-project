namespace AssignmentSystem.Domain.Common;

public interface IUpdatable
{
    DateTimeOffset UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}