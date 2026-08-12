using System.Security.Cryptography;

namespace AssignmentSystem.Application.Common;

/// <summary>
/// Produces passwords that satisfy the same complexity rules enforced by
/// <c>UserCreateRequestValidator</c> (upper, lower, digit, special, min length), so an
/// admin-generated password never fails validation on the user's next login.
/// </summary>
public static class RandomPasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Special = "!@#$%^&*?";
    private const string All = Upper + Lower + Digits + Special;

    public static string Generate(int length = 12)
    {
        var chars = new char[length];
        chars[0] = Pick(Upper);
        chars[1] = Pick(Lower);
        chars[2] = Pick(Digits);
        chars[3] = Pick(Special);
        for (var i = 4; i < length; i++)
        {
            chars[i] = Pick(All);
        }

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}