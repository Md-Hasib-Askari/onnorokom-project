using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

/// <summary>
/// A one-time password issued for the forgot-password flow. The code itself is never stored in
/// plain text, only its hash, so a leaked database does not hand out usable reset codes.
/// </summary>
public class PasswordResetCode : BaseEntity
{
    public Guid AuthUserId { get; private set; }
    public string CodeHash { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public int AttemptCount { get; private set; }

    private PasswordResetCode()
    {
    }

    public static PasswordResetCode Create(Guid authUserId, string codeHash, DateTimeOffset expiresAt)
    {
        return new PasswordResetCode
        {
            AuthUserId = authUserId,
            CodeHash = codeHash,
            ExpiresAt = expiresAt
        };
    }

    public bool IsUsable(DateTimeOffset now) => ConsumedAt is null && ExpiresAt > now && AttemptCount < 5;

    public void RegisterFailedAttempt()
    {
        AttemptCount++;
    }

    public void Consume()
    {
        ConsumedAt = DateTimeOffset.UtcNow;
    }
}