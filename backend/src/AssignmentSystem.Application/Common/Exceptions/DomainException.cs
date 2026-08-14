using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Exceptions;

public class DomainException(string message) : Exception(message);

public class DuplicateEmailException(string email)
    : DuplicateEntityException($"A user with email '{email}' already exists.");

public class InvalidCredentialsException() : DomainException("Invalid email or password.");

public class AccountPendingException() : DomainException("Account is pending approval by an administrator.");

public class AccountRejectedException() : DomainException("Account has been rejected by an administrator.");

public class AccountInactiveException() : DomainException("Account is inactive. Contact an administrator.");

public class InvalidRefreshTokenException() : DomainException("Refresh token is invalid or has expired.");

public class InvalidCurrentPasswordException() : DomainException("Current password is incorrect.");

public class InvalidOrExpiredResetCodeException() : DomainException("The code is invalid or has expired.");

public class PasswordResetRateLimitedException()
    : DomainException("A code was already sent recently. Please wait before requesting another.");

/// <summary>
/// Raised when an admin has switched off self-registration for the requested role. Distinct from a
/// validation failure: the payload is well formed, the system is simply closed to it right now.
/// </summary>
public class RegistrationDisabledException(UserRole role)
    : DomainException($"Self-registration is currently closed for the {role} role. Contact an administrator.");

/// <summary>
/// Raised when an admin has switched off self-service profile editing for the caller's role.
/// </summary>
public class ProfileEditDisabledException(UserRole role)
    : DomainException($"Profile editing is currently disabled for the {role} role. Contact an administrator.");

public class EntityNotFoundException(string message) : DomainException(message);

public class DuplicateEntityException(string message) : DomainException(message);

public class InvalidTeacherException(string message) : DomainException(message);

public class EntityInUseException(string message) : DomainException(message);

/// <summary>
/// Raised when the caller is authenticated and the target exists, but the resource is not theirs.
/// Distinct from <see cref="EntityNotFoundException"/>, which is used instead wherever a 403 would
/// itself leak the existence of a record the caller may not know about.
/// </summary>
public class ForbiddenException(string message) : DomainException(message);
