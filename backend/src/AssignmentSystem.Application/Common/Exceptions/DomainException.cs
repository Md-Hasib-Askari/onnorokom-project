namespace AssignmentSystem.Application.Common.Exceptions;

public class DomainException(string message) : Exception(message);

public class DuplicateEmailException(string email)
    : DomainException($"A user with email '{email}' already exists.");

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.") { }
}

public class AccountPendingException : DomainException
{
    public AccountPendingException()
        : base("Account is pending approval by an administrator.") { }
}

public class AccountRejectedException : DomainException
{
    public AccountRejectedException()
        : base("Account has been rejected by an administrator.") { }
}

public class AccountInactiveException : DomainException
{
    public AccountInactiveException()
        : base("Account is inactive. Contact an administrator.") { }
}

public class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException()
        : base("Refresh token is invalid or has expired.") { }
}

public class EntityNotFoundException(string message) : DomainException(message);
