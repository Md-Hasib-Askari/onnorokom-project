namespace AssignmentSystem.Application.DTOs.Auth;

/// <summary>
/// <paramref name="StudentSectionId"/> is required only when approving a self-registered student,
/// who has no section yet. Ignored for every other role and for students created by an admin.
/// </summary>
public record ApproveUserRequest(Guid UserId, bool Approve, Guid? StudentSectionId = null);
