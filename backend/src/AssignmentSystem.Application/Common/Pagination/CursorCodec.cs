using System.Text;
using System.Text.Json;
using AssignmentSystem.Application.Common.Exceptions;

namespace AssignmentSystem.Application.Common.Pagination;

/// <summary>
/// Encodes and decodes the opaque cursor. The wire format is base64 JSON so the cursor is
/// unforgeable in practice but debuggable; the pair/decoders below are the only way a caller is
/// meant to interpret one. A cursor that does not parse, or whose JSON shape does not match the
/// key type the endpoint expects, throws <see cref="DomainException"/> which the middleware maps
/// to <c>400 Invalid pagination cursor.</c>
/// </summary>
public static class CursorCodec
{
    public const string InvalidCursorMessage = "Invalid pagination cursor.";

    /// <summary>Keyset on <c>(Timestamp, Id)</c>, e.g. assignment lists ordered by created/submitted time.</summary>
    public static string Encode(DateTimeOffset timestamp, Guid id)
    {
        // UtcTicks, not Unix milliseconds: the database stores microsecond
        // precision, and truncating to milliseconds here makes the decoded
        // boundary key slightly smaller than the real key, which re-fetches the
        // boundary row on the next page.
        return EncodeCore(new CursorEnvelope { Key = timestamp.UtcTicks, Id = id });
    }

    /// <summary>Keyset on <c>(String, Id)</c>, e.g. submissions ordered by student name.</summary>
    public static string Encode(string value, Guid id)
    {
        return EncodeCore(new CursorEnvelope { Text = value, Id = id });
    }

    /// <summary>Keyset on <c>(String, String, Id)</c>, e.g. the teacher students roster by section then name.</summary>
    public static string Encode(string first, string second, Guid id)
    {
        return EncodeCore(new CursorEnvelope { Text = first, Secondary = second, Id = id });
    }

    /// <summary>Decodes a <c>(Timestamp, Id)</c> cursor.</summary>
    public static (DateTimeOffset Timestamp, Guid Id) DecodeTimestamp(string cursor)
    {
        var envelope = DecodeCore(cursor);
        return (new DateTimeOffset(envelope.Key, TimeSpan.Zero), envelope.Id);
    }

    /// <summary>Decodes a <c>(String, Id)</c> cursor.</summary>
    public static (string Value, Guid Id) DecodeString(string cursor)
    {
        var envelope = DecodeCore(cursor);
        if (envelope.Text is null)
        {
            throw new DomainException(InvalidCursorMessage);
        }

        return (envelope.Text, envelope.Id);
    }

    /// <summary>Decodes a <c>(String, String, Id)</c> cursor.</summary>
    public static (string First, string Second, Guid Id) DecodeStringPair(string cursor)
    {
        var envelope = DecodeCore(cursor);
        if (envelope.Text is null || envelope.Secondary is null)
        {
            throw new DomainException(InvalidCursorMessage);
        }

        return (envelope.Text, envelope.Secondary, envelope.Id);
    }

    private static string EncodeCore(CursorEnvelope envelope)
    {
        return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(envelope));
    }

    private static CursorEnvelope DecodeCore(string cursor)
    {
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var envelope = JsonSerializer.Deserialize<CursorEnvelope>(bytes);
            if (envelope is null || envelope.Id == Guid.Empty)
            {
                throw new InvalidDataException();
            }

            return envelope;
        }
        catch (Exception ex) when (ex is FormatException or InvalidDataException or JsonException)
        {
            throw new DomainException(InvalidCursorMessage);
        }
    }

    /// <summary>
    /// One key shape fits all three keysets: timestamp cursors use <see cref="Key"/>, string
    /// cursors use <see cref="Text"/>, and the roster cursor uses both <see cref="Text"/> and
    /// <see cref="Secondary"/>. The unused members are simply null.
    /// </summary>
    private sealed record CursorEnvelope
    {
        public long Key { get; init; }
        public string? Text { get; init; }
        public string? Secondary { get; init; }
        public Guid Id { get; init; }
    }
}
