namespace AssignmentSystem.Application.DTOs.Auth;

public record ApproveUserRequest(Guid UserId, bool Approve);
