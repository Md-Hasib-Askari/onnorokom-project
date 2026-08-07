using AssignmentSystem.Application.Common.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Options;

namespace AssignmentSystem.Infrastructure.Security;

public class PasswordHasher(IOptions<BCryptSettings> options) : IPasswordHasher
{
    private readonly int _workFactor = options.Value.WorkFactor;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: _workFactor);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
