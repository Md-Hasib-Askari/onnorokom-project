using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class AdminProfile : BaseEntity
{
    public Guid AuthUserId { get; private set; }
    public string? Position { get; private set; }
    public string? PhoneNumber { get; private set; }

    public virtual AuthUser? AuthUser { get; private set; }

    private AdminProfile()
    {
    }

    public void UpdateDetails(string? position, string? phoneNumber)
    {
        Position = position;
        PhoneNumber = phoneNumber;
    }

    public static AdminProfile Create(Guid authUserId)
    {
        return new AdminProfile
        {
            AuthUserId = authUserId
        };
    }
}
