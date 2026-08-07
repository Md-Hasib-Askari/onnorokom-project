namespace AssignmentSystem.Domain.Common;

public interface ICreatable
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
}