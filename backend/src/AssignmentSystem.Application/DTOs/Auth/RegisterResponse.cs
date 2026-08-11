using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Auth;

public record RegisterResponse(Guid Id, string Email, string FullName, UserRole Role, AccountStatus Status);
