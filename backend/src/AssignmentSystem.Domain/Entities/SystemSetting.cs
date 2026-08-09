using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

/// <summary>
/// One admin-controlled policy value, stored as a string so a single table serves every
/// <see cref="SystemSettingKey"/> regardless of type. Callers go through the typed factory and
/// accessor rather than touching <see cref="Value"/>, which keeps the encoding in one place.
/// </summary>
public class SystemSetting : BaseEntity
{
    /// <summary>Canonical encodings for boolean settings, so the stored text never varies by caller.</summary>
    private const string TrueValue = "true";
    private const string FalseValue = "false";

    public SystemSettingKey Key { get; private set; }
    public string Value { get; private set; } = null!;

    private SystemSetting()
    {
    }

    /// <summary>
    /// Reads the setting as a boolean. Anything unparseable reads as <c>false</c> so a hand-edited
    /// row degrades to the restrictive answer instead of throwing on every request.
    /// </summary>
    public bool AsBoolean() => bool.TryParse(Value, out var parsed) && parsed;

    public void UpdateBoolean(bool value)
    {
        Value = Encode(value);
    }

    public static SystemSetting CreateBoolean(SystemSettingKey key, bool value)
    {
        return new SystemSetting
        {
            Key = key,
            Value = Encode(value)
        };
    }

    private static string Encode(bool value) => value ? TrueValue : FalseValue;
}
