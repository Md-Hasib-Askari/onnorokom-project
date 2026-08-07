namespace AssignmentSystem.Domain.Common;

public abstract class BaseEntity : ICreatable, IUpdatable, ISoftDeletable
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    DateTimeOffset ICreatable.CreatedAt { get => CreatedAt; set => CreatedAt = value; }
    string? ICreatable.CreatedBy { get => CreatedBy; set => CreatedBy = value; }
    DateTimeOffset IUpdatable.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value; }
    string? IUpdatable.UpdatedBy { get => UpdatedBy; set => UpdatedBy = value; }
    bool ISoftDeletable.IsDeleted { get => IsDeleted; set => IsDeleted = value; }
    DateTimeOffset? ISoftDeletable.DeletedAt { get => DeletedAt; set => DeletedAt = value; }
    string? ISoftDeletable.DeletedBy { get => DeletedBy; set => DeletedBy = value; }
}
